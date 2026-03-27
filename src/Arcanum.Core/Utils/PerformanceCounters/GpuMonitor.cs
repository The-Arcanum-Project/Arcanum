using System.Diagnostics;

namespace Arcanum.Core.Utils.PerformanceCounters;

public sealed class GpuMonitor : IDisposable
{
   private const string GPU_CATEGORY = "GPU Engine";
   private const string GPU_COUNTER = "Utilization Percentage";
   private const string VRAM_CATEGORY = "GPU Process Memory";
   private const string VRAM_COUNTER = "Dedicated Usage";

   private readonly Dictionary<string, PerformanceCounter> _gpuCounters = new();
   private readonly Dictionary<string, PerformanceCounter> _vramCounters = new();
   private bool _disposed;

   private PerformanceCounterCategory? _gpuCategory;
   private string _pidPrefix = string.Empty;
   private PerformanceCounterCategory? _vramCategory;

   public void Dispose()
   {
      if (_disposed)
         return;

      foreach (var counter in _gpuCounters.Values)
         try
         {
            counter.Dispose();
         }
         catch
         {
            // ignored
         }

      foreach (var counter in _vramCounters.Values)
         try
         {
            counter.Dispose();
         }
         catch
         {
            // ignored
         }

      _gpuCounters.Clear();
      _vramCounters.Clear();

      _disposed = true;
   }

   public bool Initialize()
   {
      if (_disposed)
         throw new ObjectDisposedException(nameof(GpuMonitor));

      try
      {
         var processId = Process.GetCurrentProcess().Id;
         _pidPrefix = $"pid_{processId}_";

         _gpuCategory = new(GPU_CATEGORY);
         _vramCategory = new(VRAM_CATEGORY);

         SyncCounters();
         return true;
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("PerfCounter", LogLevel.ERR, $"Error initializing GPU counter: {ex.Message}");
         Dispose();
         return false;
      }
   }

   /// <summary>
   ///    This dynamically checks if WPF has destroyed old DirectX surfaces
   ///    and created new ones (which happens constantly during window resize).
   /// </summary>
   private void SyncCounters()
   {
      if (_gpuCategory == null || _vramCategory == null)
         return;

      var currentVramInstances = _vramCategory.GetInstanceNames()
                                              .Where(inst => inst.StartsWith(_pidPrefix, StringComparison.OrdinalIgnoreCase))
                                              .ToList();

      var currentGpuInstances = _gpuCategory.GetInstanceNames()
                                            .Where(inst => inst.StartsWith(_pidPrefix, StringComparison.OrdinalIgnoreCase))
                                            .ToList();

      SyncDictionary(_vramCounters, VRAM_CATEGORY, VRAM_COUNTER, currentVramInstances);
      SyncDictionary(_gpuCounters, GPU_CATEGORY, GPU_COUNTER, currentGpuInstances);
   }

   private void SyncDictionary(Dictionary<string, PerformanceCounter> dict, string category, string counterName, List<string> currentInstances)
   {
      // Remove known dead keys (if Windows actually drops them)
      var deadKeys = dict.Keys.Except(currentInstances).ToList();
      foreach (var key in deadKeys)
      {
         dict[key].Dispose();
         dict.Remove(key);
      }

      // Add new keys
      var newKeys = currentInstances.Except(dict.Keys).ToList();
      foreach (var key in newKeys)
         try
         {
            var counter = new PerformanceCounter(category, counterName, key, true);
            counter.NextValue();
            dict.Add(key, counter);
         }
         catch
         {
            // Ignore transient access errors
         }
   }

   public GpuUsageMetrics GetMetrics()
   {
      // Assuming your PerformanceCountersHelper.HasDedicatedGpu check is here
      if (_disposed)
         throw new ObjectDisposedException(nameof(GpuMonitor));

      SyncCounters();

      var metrics = new GpuUsageMetrics { GpuUsage = -1, VramUsageMb = -1 };

      if (_gpuCounters.Count > 0)
      {
         float maxUsage = 0;
         foreach (var kvp in _gpuCounters.ToList()) // ToList allows safe removal
            try
            {
               var val = kvp.Value.NextValue();
               // FIX 3: Detect ghost counters. If it throws an invalid operation, it's dead.
               maxUsage = Math.Max(maxUsage, val);
            }
            catch (InvalidOperationException)
            {
               kvp.Value.Dispose();
               _gpuCounters.Remove(kvp.Key);
            }
            catch
            {
               /* Ignore temporary failures */
            }

         metrics.GpuUsage = maxUsage;
      }

      const long limit = 100L * 1024 * 1024 * 1024;

      if (_vramCounters.Count > 0)
      {
         float totalVramBytes = 0;

         // Group instances by their base name (stripping off the #1, #2 suffix)
         // This prevents summing stale "ghost" instances created during window resize.
         var groupedCounters = _vramCounters.ToList().GroupBy(kvp => kvp.Key.Split('#')[0]);

         foreach (var group in groupedCounters)
         {
            float maxVramForThisGpu = 0;

            foreach (var kvp in group)
               try
               {
                  var val = kvp.Value.NextValue();

                  if (val is > 0 and < limit)
                     // Take the highest value among duplicates to bypass frozen ghost counters
                     maxVramForThisGpu = Math.Max(maxVramForThisGpu, val);
               }
               catch (InvalidOperationException)
               {
                  kvp.Value.Dispose();
                  _vramCounters.Remove(kvp.Key);
               }
               catch
               {
                  /* Ignore temporary failures */
               }

            totalVramBytes += maxVramForThisGpu;
         }

         metrics.VramUsageMb = totalVramBytes / (1024f * 1024f);
      }

      return metrics;
   }
}