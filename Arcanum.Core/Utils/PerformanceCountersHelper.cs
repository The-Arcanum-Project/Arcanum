using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Arcanum.Core.Utils;

public class PerformanceCountersHelper
{
   private static float _memoryUsage;
   private static float _cpuUsage;
   //
   // private static PerformanceCounter? _cpuCounter;
   private static PerformanceCounter? _memoryCounter;

   private static Timer Updater { get; set; } = null!;
   private static string _appName = string.Empty;

   private static DateTime _lastCpuCheck = DateTime.MinValue;
   private static TimeSpan _lastCpuTime = TimeSpan.Zero;
   private static float _lastCpuValue;

   private static int _updateIntervalMs = 1000;

   
   private static IPerformanceMeasured? _window;

   // Initialize the resource usage helper by setting the application name and starting the timer
   public static void Initialize(IPerformanceMeasured? window)
   {
      _window = window;
      _appName = Process.GetCurrentProcess().ProcessName;

      Updater = new() { Interval = _updateIntervalMs };
      Updater.Elapsed += OnTimerTick;
      
      var initThread = new Thread(() =>
      {
         //_cpuCounter = new("Process", "% Processor Time", _appName, true);
         _memoryCounter = new("Process", "Private Bytes", _appName, true);
      });
      initThread.Start();
      Updater.Start();
   }

   // Update the CPU and memory usage
   private static void UpdateResources()
   {
      if (_memoryCounter == null)
         return;

      // Update memory as before
      _memoryUsage = _memoryCounter.NextValue() / 1024 / 1024;

      // Update CPU only every second
      var now = DateTime.UtcNow;
      // ReSharper disable once PossibleLossOfFraction
      if ((now - _lastCpuCheck).TotalSeconds >= _updateIntervalMs / 1000)
      {
         var process = Process.GetCurrentProcess();
         var currentCpuTime = process.TotalProcessorTime;
        
         var cpuUsedMs = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
         var totalMsPassed = (now - _lastCpuCheck).TotalMilliseconds;
        
         _lastCpuValue = (float)(cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100);
        
         _lastCpuCheck = now;
         _lastCpuTime = currentCpuTime;
      }
    
      _cpuUsage = _lastCpuValue;

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