using MonitoringAgent.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Register the monitor as a Singleton so it persists between API calls
builder.Services.AddSingleton<SystemMonitor>();

var app = builder.Build();

// 2. Define the API Endpoint
app.MapGet("/stats", (SystemMonitor monitor) =>
{
	// Refresh the values whenever the endpoint is hit
	monitor.UpdateValues();

	// Return an anonymous object which ASP.NET Core automatically 
	// serializes to JSON
	return Results.Ok(new
	{
		Timestamp = DateTime.Now,
		CpuUsagePercent = monitor.currentCPUUsagePercent,
		Ram = new
		{
			Used = monitor.currentUsedRAM,
			Total = monitor.totalRAM,
			Unit = "GB"
		},
		DiskIO = new
		{
			ReadMBps = monitor.currentDiskReadMB,
			WriteMBps = monitor.currentDiskWriteMB
		},
		Disks = monitor.diskUsageCounters.Select(d => new {
			Name = d.driveInfo.Name,
			UsedGiB = d.usedSpace / Math.Pow(1024, 3),
			TotalGiB = d.totalDiskSpace / Math.Pow(1024, 3)
		}),
		Network = monitor.networkCounters.Select(n => new {
			Interface = n.NetworkInterface.Name,
			DownloadMbit = n.CurrentInBytesPerSec * 8f / 1_000_000f,
			UploadMbit = n.CurrentOutBytesPerSec * 8f / 1_000_000f
		})
	});
});

app.Run("http://0.0.0.0:8080");
