using Arcanum.Core.Utils.Parsing.ParsingStep;

namespace Arcanum.Core.Utils.Parsing.Steps;

public class DebugStep : ParsingStepBase
{
   public DebugStep(int totalSteps, string[] dependencies) : base(totalSteps, dependencies)
   {
   }

   protected override bool ExecuteCore(CancellationToken cancellationToken)
   {
      ReportSubStepCompletion(TimeSpan.FromMilliseconds(450), 0);
      Thread.Sleep(450); // Simulate work
      return true;
   }
}