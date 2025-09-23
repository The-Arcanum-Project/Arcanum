using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class PureParseLoadingService : FileLoadingService
{
   private const string ACTION_STACK = nameof(PureParseLoadingService);
   public virtual string[] GroupingNodeNames => [];

   public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var rn = PLHelper.ParseFile(fileObj, out var source);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);
      var validation = true;

      LoadSingleFile(rn, ctx, fileObj, ACTION_STACK, source, ref validation, lockObject);

      return validation;
   }

   protected abstract void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject);
}