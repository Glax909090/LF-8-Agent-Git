namespace MonitoringAgent.Utils
{
	public class DiskUsageCounter(DriveInfo driveInfo)
	{
		public DriveInfo driveInfo = driveInfo;
		public long totalDiskSpace = driveInfo.TotalSize;
		public long freeSpace = driveInfo.TotalFreeSpace;
		public long usedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace;

		public void UpdateCounter()
		{
			try
			{
				totalDiskSpace = driveInfo.TotalSize;
				freeSpace = driveInfo.TotalFreeSpace;
				usedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace;
			}
			catch
			{
                usedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace;
                totalDiskSpace = 0;
				freeSpace = 0;
			}
		}
	}
}
