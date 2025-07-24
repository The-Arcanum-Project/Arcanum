using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Arcanum.Core.Utils;

public static class PerformanceCountersHelper
{
   private static float _memoryUsage;
   private static float _cpuUsage;

   private static Timer Updater { get; set; } = null!;

   private static DateTime _lastCpuCheck = DateTime.MinValue;
   private static TimeSpan _lastCpuTime = TimeSpan.Zero;

   private const int UPDATE_INTERVAL_MS = 1000;

   private static IPerformanceMeasured? _window;

   public static void Initialize(IPerformanceMeasured? window)
   {
      _window = window;

      Updater = new() { Interval = UPDATE_INTERVAL_MS };
      Updater.Elapsed += OnTimerTick;
      Updater.Start();
   }

   private static void UpdateResources()
   {
      var process = Process.GetCurrentProcess();

      _memoryUsage = process.WorkingSet64 / 1024.0f / 1024.0f;

      var now = DateTime.UtcNow;
      // ReSharper disable once PossibleLossOfFraction
      if ((now - _lastCpuCheck).TotalSeconds >= UPDATE_INTERVAL_MS / 1000)
      {
         var currentCpuTime = process.TotalProcessorTime;

         var cpuUsedMs = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
         var totalMsPassed = (now - _lastCpuCheck).TotalMilliseconds;

         _cpuUsage = (float)(cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100);

         _lastCpuCheck = now;
         _lastCpuTime = currentCpuTime;
      }
   }

   private static void OnTimerTick(object? state, EventArgs eventArgs)
   {
      if (_window == null)
         return;

      UpdateResources();
      _window.SetMemoryUsage(_memoryUsage > 1024
                                ? $"RAM: [{Math.Round(_memoryUsage / 1024, 2):F2} GB]"
                                : $"RAM: [{Math.Round(_memoryUsage)} MB]");
      _window.SetCpuUsage("CPU: [" + $"{Math.Round(_cpuUsage, 2):F2}%".PadLeft(6) + "]");
   }
}

public interface IPerformanceMeasured
{
   void SetCpuUsage(string cpuUsage);
   void SetMemoryUsage(string memoryUsage);
}