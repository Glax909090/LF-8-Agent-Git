using System.Net.NetworkInformation;
using System.Diagnostics;

public class NetworkPerformanceCounter
{
	public readonly NetworkInterface NetworkInterface;

	private long _previousBytesReceived = 0;
	private long _previousBytesSent = 0;
	private DateTime _previousTime = DateTime.UtcNow;

	public double CurrentInBytesPerSec { get; private set; } = 0;
	public double CurrentOutBytesPerSec { get; private set; } = 0;

	public NetworkPerformanceCounter(NetworkInterface ni)
	{
		NetworkInterface = ni;

		var stats = ni.GetIPStatistics();
		_previousBytesReceived = stats.BytesReceived;
		_previousBytesSent = stats.BytesSent;
		_previousTime = DateTime.UtcNow;

		Console.WriteLine($"✅ Added: {ni.Name}  (Description: {ni.Description})");
	}

	public void UpdateCounter()
	{
		var stats = NetworkInterface.GetIPv4Statistics();

		long currentReceived = stats.BytesReceived;
		long currentSent = stats.BytesSent;
		DateTime now = DateTime.UtcNow;

		double secondsElapsed = (now - _previousTime).TotalSeconds;

		if (secondsElapsed > 0)
		{
			CurrentInBytesPerSec = (currentReceived - _previousBytesReceived) / secondsElapsed;
			CurrentOutBytesPerSec = (currentSent - _previousBytesSent) / secondsElapsed;
		}

		// Update for next call
		_previousBytesReceived = currentReceived;
		_previousBytesSent = currentSent;
		_previousTime = now;
	}
}
