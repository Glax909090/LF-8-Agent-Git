using Xunit;
using MonitoringAgent.Utils;
using System;
using System.Linq;

namespace MonitoringAgent.Tests
{
    public class SystemMonitorTests
    {
        //"CPU-Auslastung validieren"
        [Fact]
        public void Test_CpuUsage_Is_In_Range()
        {
            // Arrange
            var monitor = new SystemMonitor();

            // Act
            monitor.UpdateValues();
            float cpu = monitor.currentCPUUsagePercent;

            // Assert: Ein Prozentwert muss zwischen 0 und 100 liegen
            Assert.InRange(cpu, 0f, 100f);
        }

        // 2. "Arbeitsspeicher-Berechnung prüfen"
        [Fact]
        public void Test_RamUsage_Logic()
        {
            // Arrange
            var monitor = new SystemMonitor();

            // Act
            monitor.UpdateValues();

            // Assert: Logik-Check (Man kann nicht mehr RAM nutzen als da ist)
            Assert.True(monitor.totalRAM > 0, "Total RAM sollte erkannt werden.");
            Assert.True(monitor.currentUsedRAM <= monitor.totalRAM, "Used RAM darf nicht größer als Total RAM sein.");
        }

        //  "Hardware-Erkennung Festplatten"
        [Fact]
        public void Test_DiskDetection_Works()
        {
            // Arrange
            var monitor = new SystemMonitor();

            // Act
            // (Disks werden im Konstruktor automatisch geladen)

            // Assert: Prüfen, ob Laufwerke gefunden wurden
            Assert.NotEmpty(monitor.diskUsageCounters);
            var drive = monitor.diskUsageCounters.First();
            Assert.True(drive.totalDiskSpace > 0, "Die erkannte Festplatte sollte Speicherplatz haben.");
        }
    }
}