using System.Diagnostics;
using System.Windows;
using Timer = System.Timers.Timer;

namespace Arcanum.Core.Utils;

public class PerformanceCountersHelper
{
   private static float _memoryUsage;
   private static float _cpuUsage;

   private static PerformanceCounter? _cpuCounter;
   private static PerformanceCounter? _memoryCounter;

   private static Timer Updater { get; set; } = null!;
   private static string _appName = string.Empty;

   private static IPerformanceMeasured? _window;

   // Initialize the resource usage helper by setting the application name and starting the timer
   public static void Initialize(IPerformanceMeasured? window)
   {
      _window = window;
      _appName = Process.GetCurrentProcess().ProcessName;

      Updater = new() { Interval = 1000 };
      Updater.Elapsed += OnTimerTick;
      var initThread = new Thread(() =>
      {
         _cpuCounter = new("Process", "% Processor Time", _appName, true);
         _memoryCounter = new("Process", "Private Bytes", _appName, true);
      });
      initThread.Start();
      Updater.Start();
   }

   // Update the CPU and memory usage
   private static void UpdateResources()
   {
      if (_cpuCounter == null || _memoryCounter == null)
         return;

      _cpuUsage = _cpuCounter.NextValue() / Environment.ProcessorCount;
      _memoryUsage = _memoryCounter.NextValue() / 1024 / 1024;
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

   public static void Dispose()
   {
      _cpuCounter?.Dispose();
      _memoryCounter?.Dispose();
      Updater.Stop();
      Updater.Dispose();
   }
}

public interface IPerformanceMeasured
{
   void SetCpuUsage(string cpuUsage);
   void SetMemoryUsage(string memoryUsage);
}