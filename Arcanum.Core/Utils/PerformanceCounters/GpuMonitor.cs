using System.Diagnostics;

namespace Arcanum.Core.Utils.PerformanceCounters;

public class GpuMonitor : IDisposable
{
   private const string CATEGORY_NAME = "GPU Engine";
   private const string COUNTER_NAME = "Utilization Percentage";

   private const string GPU_MEMORY_CATEGORY = "GPU Process Memory";
   private const string VRAM_COUNTER_NAME = "Dedicated Usage";

   private PerformanceCounter? _vramUsageCounter;
   private readonly List<PerformanceCounter> _gpuCounters = new();

   /// <summary>
   /// Initializes the monitor for the current process. This may take a moment.
   /// Best to call this from a background thread if startup performance is critical.
   /// </summary>
   public bool Initialize()
   {
      try
      {
         var currentProcess = Process.GetCurrentProcess();
         var processId = currentProcess.Id;

         var category = new PerformanceCounterCategory(CATEGORY_NAME);

         var instanceNames = category.GetInstanceNames()
                                     .Where(inst => inst.Contains($"pid_{processId}"))
                                     .ToList();

         if (instanceNames.Count == 0)
         {
            // This can happen if the process has not yet used the GPU.
            // We can try again later if we want.
            Debug.WriteLine($"Could not find GPU performance counter instance for PID {processId}.");
            return false;
         }

         foreach (var counter in instanceNames.Select(instanceName
                                                         => new PerformanceCounter(CATEGORY_NAME,
                                                             COUNTER_NAME,
                                                             instanceName)))
            _gpuCounters.Add(counter);

         var gpuMemoryCat = new PerformanceCounterCategory(GPU_MEMORY_CATEGORY);
         var vramInstanceName = gpuMemoryCat.GetInstanceNames()
                                            .FirstOrDefault(inst => inst.Contains($"pid_{processId}"));

         if (!string.IsNullOrEmpty(vramInstanceName))
            _vramUsageCounter = new(GPU_MEMORY_CATEGORY, VRAM_COUNTER_NAME, vramInstanceName);
         else
            Debug.WriteLine($"Could not find GPU Process Memory counter for PID {processId}.");

         foreach (var counter in _gpuCounters)
            counter.NextValue();
         _vramUsageCounter?.NextValue();

         return _gpuCounters.Count > 0 || _vramUsageCounter != null;
      }
      catch (Exception ex)
      {
         Debug.WriteLine($"Error initializing GPU counter: {ex.Message}");
         return false;
      }
   }

   /// <summary>
   /// Gets the current GPU and VRAM usage metrics. This is a synchronous call.
   /// </summary>
   public GpuUsageMetrics GetMetrics()
   {
      var metrics = new GpuUsageMetrics();

      if (_gpuCounters.Count > 0)
         metrics.GpuUsage = _gpuCounters.Select(c => c.NextValue()).Max();
      else
         metrics.GpuUsage = -1;

      if (_vramUsageCounter != null)
      {
         var vramBytes = _vramUsageCounter.NextValue();
         metrics.VramUsageMb = vramBytes / (1024 * 1024);
      }
      else
      {
         metrics.VramUsageMb = -1;
      }

      return metrics;
   }

   /// <summary>
   /// Cleans up the PerformanceCounter resource.
   /// </summary>
   public void Dispose()
   {
      foreach (var counter in _gpuCounters)
         counter.Dispose();
      _vramUsageCounter?.Dispose();
   }
}