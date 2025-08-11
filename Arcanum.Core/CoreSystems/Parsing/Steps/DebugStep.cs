using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class DebugStep : ParsingStepBase
{
   public DebugStep() : base(FileDescriptor.Dummy, false)
   {
   }

   public override string GetDebugInfo() => $"Executing DebugStep: {Name}";
}