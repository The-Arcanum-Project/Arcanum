using System.Diagnostics;
using System.IO;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.Utils.Parsing.ParsingStep;

public abstract class ParsingStepBase : IParsingStep
{
   private readonly Stopwatch _stopwatch = new();
   private readonly List<double> _durations = []; // Stores the durations of each step in milliseconds
   private readonly object _lock = new();
   private const double REPORT_THRESHOLD = 1.0;

   private int _doneSteps;
   private List<double>? _stepWeights;

   private double _lastReportedPercentage;
   private double _accumulatedWeightDone;
   private readonly int _totalSteps;

   protected ParsingStepBase(int totalSteps, string[] dependencies)
   {
      _totalSteps = totalSteps;
      Dependencies = dependencies;
      Name = GetType().Name;
   }

   public string[] Dependencies { get; }
   public List<Diagnostic> Diagnostics { get; } = [];
   public TimeSpan Duration { get; private set; }
   public bool IsSuccessful { get; private set; }
   public string Name { get; }

   public IProgress<(double percentage, int doneSteps)>? Progress { get; set; }

   public TimeSpan? EstimatedRemaining
   {
      get
      {
         List<double> durationsSnapshot;
         double accumulatedWeightDoneSnapshot;
         List<double>? stepWeightsSnapshot;
         int totalStepsSnapshot;

         lock (_lock)
         {
            durationsSnapshot = new(_durations);
            accumulatedWeightDoneSnapshot = _accumulatedWeightDone;
            stepWeightsSnapshot = _stepWeights is null ? null : [.._stepWeights];
            totalStepsSnapshot = _totalSteps;
         }

         if (durationsSnapshot.Count == 0 || totalStepsSnapshot <= 0)
            return null;

         var avg = durationsSnapshot.Average();
         var totalWeight = stepWeightsSnapshot?.Sum() ?? totalStepsSnapshot;
         var remainingWeight = totalWeight - accumulatedWeightDoneSnapshot;
         var averageWeight = stepWeightsSnapshot is { Count: > 0 }
                                ? stepWeightsSnapshot.Average()
                                : 1.0;

         if (avg <= 0 || averageWeight <= 0)
            return null;

         var estimatedSeconds = avg * remainingWeight / averageWeight;
         return TimeSpan.FromSeconds(estimatedSeconds);
      }
   }

   private double TotalWeight => _stepWeights?.Sum() ?? _totalSteps;

   /// <summary>
   /// This method is used to initialize the parsing step when being used in a parallel context across multiple files.
   /// It sets the step weights based on the file sizes of the provided files.
   /// </summary>
   /// <param name="files"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public bool Execute(ICollection<string> files, CancellationToken cancellationToken = default)
   {
      _stepWeights = files.Count > 0
                        ? files.Select(f => (double)new FileInfo(f).Length).ToList()
                        : null;

      return Execute(cancellationToken);
   }

   /// <summary>
   /// This is the main method which will be executed to perform the parsing step.
   /// </summary>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   public bool Execute(CancellationToken cancellationToken = default)
   {
      _stopwatch.Restart();
      _durations.Clear();
      _doneSteps = 0;
      _accumulatedWeightDone = 0;
      _lastReportedPercentage = 0;

      try
      {
         IsSuccessful = ExecuteCore(cancellationToken);
         return IsSuccessful;
      }
      finally
      {
         _stopwatch.Stop();
         Duration = _stopwatch.Elapsed;
         ErrorManager.AddToLog(Diagnostics);
      }
   }

   /// <summary>
   /// This method should be overridden to implement the core logic of the parsing step.
   /// </summary>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   protected abstract bool ExecuteCore(CancellationToken cancellationToken);

   /// <summary>
   /// Should be called to report the completion of a sub-step.
   /// This method will update the progress and diagnostics accordingly.
   /// It is thread-safe and can be called from multiple threads.
   /// The stepIndex parameter is used to determine the weight of the step, if applicable.
   /// If the stepWeights are not set, it defaults to a weight of 1.
   /// </summary>
   /// <param name="duration"></param>
   /// <param name="stepIndex"></param>
   protected void ReportSubStepCompletion(TimeSpan duration, int stepIndex)
   {
      lock (_lock)
      {
         _durations.Add(duration.TotalMilliseconds);
         _doneSteps++;

         var weight = _stepWeights != null && stepIndex >= 0 && stepIndex < _stepWeights.Count
                         ? _stepWeights[stepIndex]
                         : 1.0;

         _accumulatedWeightDone += weight;

         if (TotalWeight > 0)
         {
            var percentage = _accumulatedWeightDone / TotalWeight * 100.0;
            if (Math.Abs(percentage - _lastReportedPercentage) >= REPORT_THRESHOLD)
            {
               _lastReportedPercentage = percentage;
               Progress?.Report((percentage, _doneSteps));
            }
         }
      }
   }
}