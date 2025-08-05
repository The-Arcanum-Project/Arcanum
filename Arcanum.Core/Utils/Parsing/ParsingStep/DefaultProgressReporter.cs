using System.Diagnostics;

namespace Arcanum.Core.Utils.Parsing.ParsingStep;

public class DefaultProgressReporter : IProgress<(double percentage, int doneSteps)>
{
   public void Report((double percentage, int doneSteps) value)
   {
      #if DEBUG
      Debug.WriteLine($"Progress: {value.percentage:P2}, Steps Done: {value.doneSteps}");
      #endif
   }
}