using MonitoringAgent.Utils;

var monitor = new SystemMonitor();
while (true)
{
	monitor.UpdateValues();
	Console.WriteLine($"CPU Usage         : {monitor.currentCPUUsagePercent,5:F1}%");
	Console.WriteLine($"RAM Used          : {monitor.currentUsedRAM,5:F1} / {monitor.totalRAM:F1} GB");
	Console.WriteLine($"Disk Read         : {monitor.currentDiskReadMB,6:F2} MB/s");
	Console.WriteLine($"Disk Write        : {monitor.currentDiskWriteMB,6:F2} MB/s");
	foreach (var diskCounter in monitor.diskUsageCounters)
	{
		Console.WriteLine($"Disk: {diskCounter.driveInfo.Name} Used {(diskCounter.usedSpace / 1024f / 1024f / 1024f),6:F2}GiB / {(diskCounter.totalDiskSpace / 1024f / 1024f / 1024f),6:F2}GiB");
	}
	foreach (var networkCounter in monitor.networkCounters)
	{
		Console.WriteLine($"Network Device: {networkCounter.NetworkInterface.Name} Rx: {(networkCounter.CurrentInBytesPerSec * 8f / 1000f / 1000f),6:F2}Mbit/s; Tx: {(networkCounter.CurrentOutBytesPerSec * 8f / 1000f / 1000f),6:F2}Mbit/s");
	}
	Console.WriteLine("-----------------------------------");
	Thread.Sleep(1000);
}
