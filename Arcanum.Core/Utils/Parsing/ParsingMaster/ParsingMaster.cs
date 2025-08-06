using Arcanum.Core.Utils.Parsing.ParsingStep;
using Arcanum.Core.Utils.Parsing.Steps;
using JetBrains.Annotations;

namespace Arcanum.Core.Utils.Parsing.ParsingMaster;

public class ParsingMaster
{
   private readonly List<ParsingStepBase> _parsingSteps = [];
   
   [UsedImplicitly]
   public EventHandler<ParsingStepBase>? ParsingStepsChanged;
   public EventHandler<(double percentage, int doneSteps)>? StepProcessChanged;
   public EventHandler<TimeSpan>? StepDurationEstimationChanged;
   public EventHandler<double>? TotalProgressChanged;

   /// <summary>
   /// Here ALL loading steps should be added.
   /// </summary>
   private ParsingMaster()
   {
      _parsingSteps.Add(new DebugStep(1, []));
   }

   private static readonly Lazy<ParsingMaster> LazyInstance = new(() => new());
   public static ParsingMaster Instance => LazyInstance.Value;
   public int ParsingSteps => _parsingSteps.Count;
   public int ParsingStepsDone { get; private set; }

   public Task ExecuteAllParsingSteps()
   {
      List<TimeSpan> durations = [];
      
      var cts = new CancellationTokenSource();

      ParsingStepsDone = 0;
      foreach (var step in _parsingSteps)
      {
         TotalProgressChanged?.Invoke(this, ParsingStepsDone / (double)ParsingSteps * 100.0);
         ParsingStepsChanged?.Invoke(this, step);
         step.SubStepCompleted += (_, _) =>
         {
            StepProcessChanged?.Invoke(this, (step.SubPercentageCompleted, step.SubStepsDone));
            StepDurationEstimationChanged?.Invoke(this, step.EstimatedRemaining ?? TimeSpan.Zero);
         };
         
         step.Execute(cts.Token);
         
         if (cts.IsCancellationRequested)
            break;
         
         durations.Add(step.Duration);
         ParsingStepsDone++;
      }
      Thread.Sleep(2000);
      
      return Task.CompletedTask;
   }
   
   
}