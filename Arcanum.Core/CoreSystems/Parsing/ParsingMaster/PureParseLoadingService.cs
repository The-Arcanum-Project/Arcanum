using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class PureParseLoadingService : FileLoadingService
{
   private const string ACTION_STACK = nameof(PureParseLoadingService);
   public virtual string[] GroupingNodeNames => [];

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;

      LoadSingleFile(rn, ctx, fileObj, ACTION_STACK, source, ref validation, lockObject);

      return validation;
   }

   protected abstract void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject);
}