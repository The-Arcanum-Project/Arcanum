using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingStep;

public class DefaultParsingStep
{
   private readonly Stopwatch _stopwatch = new();
   private readonly List<double> _durations = []; // Stores the durations of each step in milliseconds
   private readonly object _lock = new();

   private const double REPORT_THRESHOLD = 1.0;
   private double _lastReportedPercentage;
   private double _accumulatedWeightDone;

   private int _doneSteps;
   private bool _isSuccessful;
   private int TotalSteps => Descriptor.Files.Count;
   public bool IsMultithreadable { get; }
   private List<double>? StepWeights { get; set; }
   public FileDescriptor Descriptor { get; }
   public List<Diagnostic> Diagnostics { get; } = [];
   public TimeSpan Duration { get; private set; }
   public bool IsSuccessful
   {
      get => _isSuccessful;
      protected set => _isSuccessful = value;
   }
   public string Name { get; }

   public DefaultParsingStep(FileDescriptor descriptor, bool isMultithreadable)
   {
      Descriptor = descriptor;
      IsMultithreadable = isMultithreadable;
      Name = GetType().Name;
   }

   public EventHandler<FileDescriptor>? SubStepCompleted { get; set; }
   private double? _smoothedDurationMs;

   public TimeSpan? EstimatedRemaining
   {
      get
      {
         lock (_lock)
         {
            if (_doneSteps == 0 || TotalSteps <= 0)
               return null;

            var remainingWeight = TotalWeight - _accumulatedWeightDone;
            var averageWeight = StepWeights is { Count: > 0 }
                                   ? StepWeights.Average()
                                   : 1.0;

            if (averageWeight <= 0)
               return null;

            // Use smoothed duration if available, else fallback to simple average
            var avgMs = _smoothedDurationMs ?? _durations.Average();

            if (avgMs <= 0)
               return null;

            var estimatedSeconds = (avgMs / 1000.0) * (remainingWeight / averageWeight);
            return TimeSpan.FromSeconds(estimatedSeconds);
         }
      }
   }

   /// <summary>
   /// Updates the smoothed average duration using an exponential moving average.
   /// Call this inside ReportSubStepCompletion.
   /// </summary>
   private void UpdateSmoothedDuration(double newDurationMs)
   {
      // 20% weight to the new value
      const double smoothingFactor = 0.2;
      if (_smoothedDurationMs.HasValue)
         _smoothedDurationMs = _smoothedDurationMs.Value * (1 - smoothingFactor) + newDurationMs * smoothingFactor;
      else
         _smoothedDurationMs = newDurationMs;
   }

   private double TotalWeight => StepWeights?.Sum() ?? TotalSteps;

   /// <summary>
   /// This is the main method which will be executed to perform the parsing step.
   /// To have a proper estimation of the remaining time, it is recommended to set the step weights when using multiple files or steps.
   /// </summary>
   /// <returns></returns>
   public virtual bool Execute()
   {
      SetupExecutionContext();

      if (IsMultithreadable)
      {
         var files = Descriptor.Files;
         var totalSize = StepWeights!.Sum(f => f > 0 ? f : 1);
         var avgSize = totalSize / files.Count;

         // Large file threshold for determining degree of parallelism
         const long largeFileThreshold = 50 * 1024 * 1024; // 50 MB

         int maxThreads;
         // Few files -> run one per file no need to limit threads as we have less than cores
         if (files.Count <= Environment.ProcessorCount)
            maxThreads = files.Count;
         // Large files -> cap at processor count -> prevent threads from slowing down each other
         else if (avgSize >= largeFileThreshold)
            maxThreads = Environment.ProcessorCount;
         // Small files -> increase up to 4x cores so we can utilize the CPU better as we are limited by IO
         else
            maxThreads = Math.Min(Environment.ProcessorCount * 4, files.Count);

         try
         {
            CancellationTokenSource cts = new();
            var cancellationToken = cts.Token;

            Parallel.For(0,
                         files.Count,
                         new() { MaxDegreeOfParallelism = maxThreads, CancellationToken = cancellationToken },
                         i =>
                         {
                            var file = files[i];
                            var startTime = _stopwatch.Elapsed;
                            if (cancellationToken.IsCancellationRequested)
                            {
                               Volatile.Write(ref _isSuccessful, false);
                               return;
                            }

                            var ex =
                               Descriptor.LoadingService.LoadWithErrorHandling(file,
                                                                               Descriptor,
                                                                               lockObject: _lock);

                            if (ex is { IsCritical: true })
                               cts.Cancel();

                            var stepIndex = Interlocked.Increment(ref _doneSteps) - 1;
                            var weight = StepWeights![i];
                            ReportSubStepCompletion(_stopwatch.Elapsed - startTime, weight, stepIndex);
                         });
         }
         catch (OperationCanceledException)
         {
            Volatile.Write(ref _isSuccessful, false);
            Diagnostics.Add(new(ParsingError.Instance.ParsingBaseStepFailure,
                                LocationContext.Empty,
                                DiagnosticSeverity.Error,
                                $"{GetType().Name} failed to load one or more files.",
                                "One or more files could not be loaded successfully during the parsing step.",
                                $"The parsing step {Name} encountered errors while loading files. Please check the diagnostics for more details."));
         }
      }
      else
      {
         var swInner = Stopwatch.StartNew();
         foreach (var file in Descriptor.Files)
         {
            var startTime = _stopwatch.Elapsed;

            var ex = Descriptor.LoadingService.LoadWithErrorHandling(file, Descriptor);
            if (ex is { IsCritical: true })
               return false;

            if (TotalSteps > 1)
               ReportSubStepCompletion(_stopwatch.Elapsed - startTime, StepWeights![_doneSteps], _doneSteps);
            _doneSteps++;
         }

         swInner.Stop();
         Debug.WriteLine($"Single-threaded parsing step {Name} took {swInner.Elapsed.TotalMilliseconds:#####.0} ms to complete.");
      }

      FinalizeExecutionContext();
      return IsSuccessful;
   }

   protected void FinalizeExecutionContext()
   {
      _stopwatch.Stop();
      Duration = _stopwatch.Elapsed;
   }

   protected void SetupExecutionContext()
   {
      IsSuccessful = true;
      if (!ParsingMaster.ParsingMaster.AreDependenciesLoaded(Descriptor))
         throw
            new InvalidOperationException($"Cannot execute parsing step {Name} because dependencies are not loaded.");

      StepWeights = GetFileWeights();

      _stopwatch.Restart();
      _doneSteps = 0;
      _accumulatedWeightDone = 0.0;
      _lastReportedPercentage = 0.0;
      Duration = TimeSpan.Zero;
      _smoothedDurationMs = null;
      _durations.Clear();
      SubPercentageCompleted = 0.0;
      SubStepsDone = 0;
   }

   public bool UnloadAllFiles()
   {
      foreach (var file in Descriptor.Files)
         if (!Descriptor.LoadingService.UnloadSingleFileContent(file, Descriptor))
            return false;

      return true;
   }

   /// <summary>
   /// Should be called to report the completion of a sub-step.
   /// This method will update the progress and diagnostics accordingly.
   /// It is thread-safe and can be called from multiple threads.
   /// The stepIndex parameter is used to determine the weight of the step, if applicable.
   /// If the stepWeights are not set, it defaults to a weight of 1.
   /// </summary>
   /// <param name="duration"></param>
   /// <param name="weight"></param>
   /// <param name="stepIndex"></param>
   protected void ReportSubStepCompletion(TimeSpan duration, double weight, int stepIndex)
   {
      lock (_lock)
      {
         _durations.Add(duration.TotalMilliseconds);
         UpdateSmoothedDuration(duration.TotalMilliseconds);
         _accumulatedWeightDone += weight;

         if (!(TotalWeight > 0))
            return;

         var percentage = _accumulatedWeightDone / TotalWeight * 100.0;
         if (Math.Abs(percentage - _lastReportedPercentage) >= REPORT_THRESHOLD)
         {
            SubPercentageCompleted = _lastReportedPercentage = percentage;
            SubStepsDone = stepIndex;
            SubStepCompleted?.Invoke(this, Descriptor);
         }
      }
   }

   protected virtual List<double> GetFileWeights()
   {
      return ParsingMaster.ParsingMaster.GetStepWeightsByFileSize(Descriptor);
   }

   public double SubPercentageCompleted { get; private set; }
   public int SubStepsDone { get; private set; }
}