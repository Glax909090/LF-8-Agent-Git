using MonitoringAgent.Utils;
using System.Diagnostics;

class SystemMonitor
{
	// We'll initialize them once and reuse
	private static readonly PerformanceCounter cpuCounter;
	private static readonly PerformanceCounter ramAvailableCounter;
	private static readonly PerformanceCounter diskReadCounter;
	private static readonly PerformanceCounter diskWriteCounter;
	private static readonly PerformanceCounter networkInCounter;
	private static readonly PerformanceCounter networkOutCounter;

	static SystemMonitor()
	{
		cpuCounter = new("Processor", "% Processor Time", "_Total");
		ramAvailableCounter = new("Memory", "Available MBytes", "");
		diskReadCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
		diskWriteCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
		//networkInCounter = new("Network Interface", "Bytes Received/sec", GetFirstNetworkInterface());
		//networkOutCounter = new("Network Interface", "Bytes Sent/sec", GetFirstNetworkInterface());

		// Warm-up (very important — first call usually returns 0 or nonsense)
		Thread.Sleep(500);
		cpuCounter.NextValue();
		diskReadCounter.NextValue();
		diskWriteCounter.NextValue();
		//networkInCounter.NextValue();
		//networkOutCounter.NextValue();
	}

	private static string GetFirstNetworkInterface()
	{
		// In real apps you should let user choose or show all → here we take first non-loopback
		/*var category = new PerformanceCounterCategory("Network Interface");
		var instances = category.GetInstances();

		foreach (var name in instances)
		{
			if (!name.Contains("Loopback") && !name.Contains("Pseudo-Interface"))
				return name;
		}

		return instances.Length > 0 ? instances[0] : "";*/
		return "";
	}

	public static void PrintSystemUsage()
	{
		// CPU needs two samples
		float cpu1 = cpuCounter.NextValue();
		Thread.Sleep(1000);                    // ← most common interval
		float cpu2 = cpuCounter.NextValue();
		float cpuPercent = cpu2;               // already averaged over interval

		string driveInfo = "";

		foreach(DriveInfo drive in DriveInfo.GetDrives())
		{
			driveInfo += $"Disk Usage [{drive.Name}]  : {(drive.TotalFreeSpace / 1e+9),5:F1} GB / {(drive.TotalSize / 1e+9),5:F1} GB\n";
			//Console.WriteLine(drive.Name + " : " + drive.TotalSize / 1e+9 + " : " + drive.TotalFreeSpace / 1e+9);
		}

		// RAM
		float availableMB = ramAvailableCounter.NextValue();
		long totalBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes; // ≈ total physical
		float totalGB = totalBytes / (1024f * 1024 * 1024);
		float usedGB = totalGB - (availableMB / 1024f);

		// Disk (bytes/sec — you can convert to MB/s)
		float diskReadKBps = diskReadCounter.NextValue() / 1024f;
		float diskWriteKBps = diskWriteCounter.NextValue() / 1024f;

		// Network
		//float netInKBps = networkInCounter.NextValue() / 1024f;
		//float netOutKBps = networkOutCounter.NextValue() / 1024f;

		Console.WriteLine($"CPU Usage         : {cpuPercent,5:F1}%");
		Console.WriteLine($"RAM Used          : {usedGB,5:F1} / {totalGB:F1} GB");
		Console.WriteLine($"Disk Read         : {diskReadKBps,6:F1} KB/s");
		Console.WriteLine($"Disk Write        : {diskWriteKBps,6:F1} KB/s");
		Console.WriteLine(driveInfo);
		//Console.WriteLine($"Network ↓     : {netInKBps,6:F1} KB/s");
		//Console.WriteLine($"Network ↑     : {netOutKBps,6:F1} KB/s");
	}

	static void Main()
	{
		/*while (true)
		{
			PrintSystemUsage();
			Console.WriteLine("-----------------------------------");
			Thread.Sleep(2000);
		}*/
		var category = new PerformanceCounterCategory("Network Interface");
		string[] instances = category.GetInstanceNames();

		var monitor = new MonitoringAgent.Utils.SystemMonitor();
		while (true)
		{
			monitor.UpdateValues();
			Console.WriteLine($"CPU Usage         : {monitor.currentCPUUsagePercent,5:F1}%");
			Console.WriteLine($"RAM Used          : {monitor.currentUsedRAM,5:F1} / {monitor.totalRAM:F1} GB");
			Console.WriteLine($"Disk Read         : {monitor.currentDiskReadMB,6:F1} MB/s");
			Console.WriteLine($"Disk Write        : {monitor.currentDiskWriteMB,6:F1} MB/s");
			foreach (var diskCounter in monitor.diskUsageCounters)
			{
				Console.WriteLine($"Disk: {diskCounter.driveInfo.Name} Used {diskCounter.usedSpace} / {diskCounter.totalDiskSpace}");
			}
			foreach (var networkCounter in monitor.networkCounters)
			{
				Console.WriteLine($"Network Device: {networkCounter.networkInterface.Name} Rx: {networkCounter.currentIn}; Tx: {networkCounter.currentOut}");
			}
			Thread.Sleep(1000);
		}
	}
}
