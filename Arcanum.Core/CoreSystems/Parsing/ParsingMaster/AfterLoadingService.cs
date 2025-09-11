using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class AfterLoadingService : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [];

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
      => throw new NotImplementedException("This method should not be called in AfterLoadingService.");

   protected abstract bool AfterLoadingStep();

   public override DefaultParsingStep GetParsingStep(FileDescriptor descriptor)
      => new AfterParsingStep(AfterLoadingStep, descriptor);
}