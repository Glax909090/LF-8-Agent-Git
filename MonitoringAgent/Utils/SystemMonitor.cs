using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Numerics;

namespace MonitoringAgent.Utils
{
	internal class SystemMonitor
	{
		public readonly List<NetworkPerformanceCounter> networkCounters;
		public readonly List<DiskUsageCounter> diskUsageCounters;
		public readonly PerformanceCounter ramAvailableCounter;
		public readonly PerformanceCounter diskWriteCounter;
		public readonly PerformanceCounter diskReadCounter;
		public readonly PerformanceCounter cpuCounter;

		public float currentCPUUsagePercent;
		public float currentDiskWriteMB;
		public float currentDiskReadMB;
		public float currentUsedRAM;
		public float totalRAM;

		public SystemMonitor()
		{
			cpuCounter = new("Processor", "% Processor Time", "_Total");
			ramAvailableCounter = new("Memory", "Available MBytes", "");
			diskReadCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
			diskWriteCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
			networkCounters = [];
			diskUsageCounters = [];
			UpdateNetworkInterfaces();
			UpdateDisks();
		}

		public void UpdateValues()
		{
			foreach (var counter in networkCounters)
			{
				counter.UpdateCounter();
			}
			foreach(var diskCounter in diskUsageCounters)
			{
				diskCounter.UpdateCounter();
			}
			totalRAM = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024f * 1024 * 1024);
			currentUsedRAM = totalRAM - (ramAvailableCounter.NextValue() / 1024f);
			currentDiskWriteMB = diskWriteCounter.NextValue() / 1024f / 1024f;
			currentDiskReadMB = diskReadCounter.NextValue() / 1024f / 1024f;
			currentCPUUsagePercent = cpuCounter.NextValue();
		}

		private void UpdateDisks()
		{
			diskUsageCounters.Clear();
			foreach (var drive in DriveInfo.GetDrives())
			{
				diskUsageCounters.Add(new(drive));
			}
		}

		private void UpdateNetworkInterfaces()
		{
			var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(ni =>
				{
					if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
						return false;

					var name = ni.Name;
					if (name.Contains("WFP", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("QoS", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("Packet Scheduler", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("Filter", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("VirtualBox NDIS", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("LightWeight", StringComparison.OrdinalIgnoreCase) ||
						name.Contains("Native WiFi", StringComparison.OrdinalIgnoreCase) ||
						name.StartsWith("vSwitch", StringComparison.OrdinalIgnoreCase) ||
						name.StartsWith("Local Area Connection*", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}

					return ni.OperationalStatus == OperationalStatus.Up ||
						   ni.OperationalStatus == OperationalStatus.Unknown;
				})
				.ToList();
			networkCounters.Clear();
			foreach (var networkInterface in networkInterfaces)
			{
				string? perfCounterName = NetworkPerformanceCounter.FindPerfCounterInstanceName(networkInterface);
				if (networkInterface.OperationalStatus == OperationalStatus.Up && perfCounterName != null)
				{
					Console.WriteLine(networkInterface.Name + " : " + networkInterface.NetworkInterfaceType);
					NetworkPerformanceCounter counter = new(networkInterface);
					counter.UpdateCounter();
					networkCounters.Add(counter);
				}
			}
		}
	}
}
