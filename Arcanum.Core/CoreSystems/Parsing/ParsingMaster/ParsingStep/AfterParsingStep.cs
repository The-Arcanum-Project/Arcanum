using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingStep;

public class AfterParsingStep(Func<bool> loadingAction, FileDescriptor descriptor)
   : DefaultParsingStep(descriptor, descriptor.IsMultithreadable)
{
   public override bool Execute()
   {
      SetupExecutionContext();

      IsSuccessful = loadingAction.Invoke();

      FinalizeExecutionContext();
      return IsSuccessful;
   }
}