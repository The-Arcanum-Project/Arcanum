using System.Diagnostics;
using System.Windows;
using Common.UI;
using Common.UI.MBox;
using Timer = System.Timers.Timer;

namespace Arcanum.Core.Utils.PerformanceCounters;

public static class PerformanceCountersHelper
{
   private const int UPDATE_INTERVAL_MS = 1000;
   private static float _memoryUsage;
   private static float _cpuUsage;
   private static float _gpuUsage;
   private static float _vramUsage;

   private static DateTime _lastCpuCheck = DateTime.MinValue;
   private static TimeSpan _lastCpuTime = TimeSpan.Zero;

   private static readonly GpuMonitor GPUMonitor = new();
   private static Process _currentProcess = null!;

   private static IPerformanceMeasured? _window;
   private static PerformanceCounter? _privateWorkingSetCounter;

   private static Timer Updater { get; set; } = null!;
   public static bool HasDedicatedGpu { get; private set; } = true;

   public static async void Initialize(IPerformanceMeasured? window)
   {
      _window = window;

      var initialized = await Task.Run(() => GPUMonitor.Initialize());
      _currentProcess = Process.GetCurrentProcess();

      try
      {
         _privateWorkingSetCounter = new("Process", "Working Set - Private", _currentProcess.ProcessName, true);
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("PRF", LogLevel.WRN, "Could not init RAM counter: " + ex.Message);
      }

      if (!initialized)
      {
         UIHandle.Instance.PopUpHandle.ShowMBox("Could not initialize the GPU performance counter. " +
                                                "This can happen if the application hasn't rendered anything yet.",
                                                "Monitor Error",
                                                MBoxButton.OK,
                                                MessageBoxImage.Warning);
         _gpuUsage = -1;
         _vramUsage = -1;
         HasDedicatedGpu = false;
      }

      Updater = new() { Interval = UPDATE_INTERVAL_MS };
      Updater.Elapsed += OnTimerTick;
      Updater.Start();
   }

   private static void UpdateResources()
   {
      try
      {
         _currentProcess.Refresh();

         if (_privateWorkingSetCounter != null)
            _memoryUsage = _privateWorkingSetCounter.NextValue() / 1024.0f / 1024.0f;
         else
            // Fallback just in case Windows Performance Counters are disabled on the user's PC
            _memoryUsage = _currentProcess.WorkingSet64 / 1024.0f / 1024.0f;

         var now = DateTime.UtcNow;

         if ((now - _lastCpuCheck).TotalSeconds >= UPDATE_INTERVAL_MS / 1000.0)
         {
            var currentCpuTime = _currentProcess.TotalProcessorTime;

            var cpuUsedMs = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
            var totalMsPassed = (now - _lastCpuCheck).TotalMilliseconds;

            _cpuUsage = (float)(cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100);

            _lastCpuCheck = now;
            _lastCpuTime = currentCpuTime;
         }

         if (HasDedicatedGpu)
         {
            var gpuMetrics = GPUMonitor.GetMetrics();
            _gpuUsage = gpuMetrics.GpuUsage;
            _vramUsage = gpuMetrics.VramUsageMb;
         }
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("PRF", LogLevel.ERR, "Error updating performance counters: " + ex);
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

      _window.SetGpuUsage(_gpuUsage < 0 ? "GPU: [N/A]" : "GPU: [" + $"{Math.Round(_gpuUsage, 2):F2}%".PadLeft(6) + "]");

      _window.SetVramUsage(_vramUsage < 0 ? "VRAM: [N/A]" : $"VRAM: [{Math.Round(_vramUsage)} MB]");
   }

   public static void Shutdown()
   {
      if (Updater != null!)
      {
         Updater.Stop();
         Updater.Close();
         Updater.Dispose();
      }

      if (GPUMonitor != null!)
         GPUMonitor.Dispose();

      _currentProcess.Dispose();
      _privateWorkingSetCounter?.Dispose();
   }
}

public interface IPerformanceMeasured
{
   void SetCpuUsage(string cpuUsage);
   void SetMemoryUsage(string memoryUsage);
   void SetGpuUsage(string gpuUsage);
   void SetVramUsage(string vramUsage);
   void SetFps(string fps);
}