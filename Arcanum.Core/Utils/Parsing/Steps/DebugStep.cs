using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Parsing.ParsingStep;

namespace Arcanum.Core.Utils.Parsing.Steps;

public class DebugStep : ParsingStepBase
{
   public DebugStep() : base(FileDescriptor.Dummy, false)
   {
   }
}