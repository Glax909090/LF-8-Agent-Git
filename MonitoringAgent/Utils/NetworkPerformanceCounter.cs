using System.Diagnostics;
using System.Net.NetworkInformation;

namespace MonitoringAgent.Utils
{
	internal class NetworkPerformanceCounter(NetworkInterface networkInterface)
	{
		public NetworkInterface networkInterface = networkInterface;
		public PerformanceCounter performanceCounterIn = new("Network Interface", "Bytes Received/sec", FindPerfCounterInstanceName(networkInterface));
		public PerformanceCounter performanceCounterOut = new("Network Interface", "Bytes Sent/sec", FindPerfCounterInstanceName(networkInterface));

		public double currentOut = 0;
		public double currentIn = 0;

		public void UpdateCounter()
		{
			currentOut = performanceCounterOut.NextValue();
			currentIn = performanceCounterIn.NextValue();
		}

		public static string? FindPerfCounterInstanceName(NetworkInterface ni)
		{
			var category = new PerformanceCounterCategory("Network Interface");
			var instances = category.GetInstanceNames();

			string desc = ni.Description ?? "";
			string name = ni.Name ?? "";

			// Normalize: convert (DBS) → [DBS] and vice versa, remove extra spaces
			string normalizedDesc = NormalizeBrackets(desc);
			string normalizedName = NormalizeBrackets(name);

			foreach (var inst in instances)
			{
				string normalizedInst = NormalizeBrackets(inst);

				// Exact match after normalization
				if (string.Equals(normalizedInst, normalizedDesc, StringComparison.OrdinalIgnoreCase) ||
					string.Equals(normalizedInst, normalizedName, StringComparison.OrdinalIgnoreCase))
				{
					return inst;   // return the ORIGINAL perf instance name
				}

				// Loose contains match (handles _2, _3, _4, _5 suffixes too)
				if (normalizedInst.Contains(normalizedDesc, StringComparison.OrdinalIgnoreCase) ||
					normalizedDesc.Contains(normalizedInst, StringComparison.OrdinalIgnoreCase) ||
					normalizedInst.Contains(normalizedName, StringComparison.OrdinalIgnoreCase))
				{
					return inst;
				}
			}

			return null;
		}

		// Helper to normalize bracket differences
		private static string NormalizeBrackets(string s)
		{
			if (string.IsNullOrEmpty(s)) return s;

			return s.Replace(" (", " [")
					.Replace(") ", "] ")
					.Replace("(", "[")
					.Replace(")", "]")
					.Trim();
		}
	}
}
