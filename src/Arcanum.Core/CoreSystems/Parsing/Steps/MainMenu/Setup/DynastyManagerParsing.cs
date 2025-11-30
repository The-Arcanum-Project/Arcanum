using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Dynasty))]
public partial class DynastyManagerParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Dynasty>(dependencies)
{
   public override string[] GroupingNodeNames => ["dynasty_manager"];

   public override void LoadSingleFile(RootNode rn,
                                       LocationContext ctx,
                                       Eu5FileObj fileObj,
                                       string actionStack,
                                       string source,
                                       ref bool validation,
                                       object? lockObject)
   {
      if (!ParsingMaster.ParsingMaster.RemoveAllGroupingNodes(rn,
                                                              ctx,
                                                              actionStack,
                                                              source,
                                                              ref validation,
                                                              GroupingNodeNames,
                                                              out var sns))
         return;

      SimpleObjectParser.Parse(fileObj,
                               sns,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Dynasty target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => ParseProperties(block, target, ctx, source, ref validation, allowUnknownNodes);
}