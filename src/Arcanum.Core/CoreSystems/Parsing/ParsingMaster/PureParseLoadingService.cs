using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class PureParseLoadingService(IEnumerable<IDependencyNode<string>> dependencies)
   : FileLoadingService(dependencies)
{
   private const string ACTION_STACK = nameof(PureParseLoadingService);
   public virtual string[] GroupingNodeNames => [];

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
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