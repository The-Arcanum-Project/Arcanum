using System.Diagnostics;

namespace Arcanum.Core.Utils.PerformanceCounters;

public sealed class GpuMonitor : IDisposable
{
   private const string CATEGORY_NAME = "GPU Engine";
   private const string COUNTER_NAME = "Utilization Percentage";

   private const string GPU_MEMORY_CATEGORY = "GPU Process Memory";
   private const string VRAM_COUNTER_NAME = "Dedicated Usage";

   private PerformanceCounter? _vramUsageCounter;
   private readonly List<PerformanceCounter> _gpuCounters = [];
   private bool _disposed;

   /// <summary>
   /// Initializes the monitor for the current process. This may take a moment.
   /// Best to call this from a background thread if startup performance is critical.
   /// </summary>
   public bool Initialize()
   {
      if (_disposed)
         throw new ObjectDisposedException(nameof(GpuMonitor));

      try
      {
         var currentProcess = Process.GetCurrentProcess();
         var processId = currentProcess.Id;

         var category = new PerformanceCounterCategory(CATEGORY_NAME);

         var instanceNames = category.GetInstanceNames()
                                     .Where(inst => inst.Contains($"pid_{processId}"))
                                     .ToList();

         if (instanceNames.Count == 0)
            ArcLog.WriteLine("PerfCounter",
                             LogLevel.WRN,
                             $"No GPU performance counter instances found for PID {processId}.");

         foreach (var instanceName in instanceNames)
            _gpuCounters.Add(new(CATEGORY_NAME, COUNTER_NAME, instanceName));

         var gpuMemoryCat = new PerformanceCounterCategory(GPU_MEMORY_CATEGORY);
         var vramInstanceName = gpuMemoryCat.GetInstanceNames()
                                            .FirstOrDefault(inst => inst.Contains($"pid_{processId}"));

         if (!string.IsNullOrEmpty(vramInstanceName))
            _vramUsageCounter = new(GPU_MEMORY_CATEGORY, VRAM_COUNTER_NAME, vramInstanceName);
         else
            ArcLog.WriteLine("PerfCounter",
                             LogLevel.WRN,
                             $"Could not find GPU Process Memory counter for PID {processId}.");

         // Prime the counters by reading the first value
         foreach (var counter in _gpuCounters)
            counter.NextValue();
         _vramUsageCounter?.NextValue();

         return _gpuCounters.Count > 0 || _vramUsageCounter != null;
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("PerfCounter", LogLevel.ERR, $"Error initializing GPU counter: {ex.Message}");
         Dispose();
         return false;
      }
   }

   /// <summary>
   /// Gets the current GPU and VRAM usage metrics. This is a synchronous call.
   /// </summary>
   public GpuUsageMetrics GetMetrics()
   {
      if (!PerformanceCountersHelper.HasDedicatedGpu)
         return new()
         {
            GpuUsage = -1, VramUsageMb = -1,
         };

      if (_disposed)
         throw new ObjectDisposedException(nameof(GpuMonitor));

      var metrics = new GpuUsageMetrics();

      if (_gpuCounters.Count > 0)
         try
         {
            metrics.GpuUsage = _gpuCounters.Select(c => c.NextValue()).Max();
         }
         catch (InvalidOperationException ex)
         {
            Debug.WriteLine($"Failed to read GPU usage counter: {ex.Message}");
            metrics.GpuUsage = -1;
         }
      else
         metrics.GpuUsage = -1;

      if (_vramUsageCounter != null)
         try
         {
            var vramBytes = _vramUsageCounter.NextValue();
            metrics.VramUsageMb = vramBytes / (1024f * 1024f);
         }
         catch (InvalidOperationException ex)
         {
            Debug.WriteLine($"Failed to read VRAM usage counter: {ex.Message}");
            metrics.VramUsageMb = -1;
         }
      else
         metrics.VramUsageMb = -1;

      return metrics;
   }

   /// <summary>
   /// Cleans up the PerformanceCounter resources.
   /// </summary>
   public void Dispose()
   {
      Dispose(true);
   }

   private void Dispose(bool disposing)
   {
      if (_disposed)
         return;

      if (disposing)
      {
         foreach (var counter in _gpuCounters)
            try
            {
               counter.Dispose();
            }
            catch (Exception ex)
            {
               Debug.WriteLine($"Error disposing GPU counter: {ex.Message}");
            }

         _gpuCounters.Clear();

         try
         {
            _vramUsageCounter?.Dispose();
         }
         catch (Exception ex)
         {
            Debug.WriteLine($"Error disposing VRAM counter: {ex.Message}");
         }

         _vramUsageCounter = null;
      }

      _disposed = true;
   }
}