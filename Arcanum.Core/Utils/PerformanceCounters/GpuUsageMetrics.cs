namespace Arcanum.Core.Utils.PerformanceCounters;

public class GpuUsageMetrics
{
   /// <summary>
   /// GPU engine utilization percentage.
   /// </summary>
   public float GpuUsage { get; set; }

   /// <summary>
   /// Dedicated VRAM usage in Megabytes (MB).
   /// </summary>
   public float VramUsageMb { get; set; }
}