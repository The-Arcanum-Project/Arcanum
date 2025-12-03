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
      var pc = new ParsingContext(ctx, source, ACTION_STACK, ref validation);

      LoadSingleFile(rn, ref pc, fileObj, lockObject);

      return validation;
   }

   protected abstract void LoadSingleFile(RootNode rn,
                                          ref ParsingContext pc,
                                          Eu5FileObj fileObj,
                                          object? lockObject);
}