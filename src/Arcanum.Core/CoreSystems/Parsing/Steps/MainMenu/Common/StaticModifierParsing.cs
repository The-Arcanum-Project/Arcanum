using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

[ParserFor(typeof(StaticModifier))]
public partial class StaticModifierParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<StaticModifier>(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       LocationContext ctx,
                                       Eu5FileObj fileObj,
                                       string actionStack,
                                       string source,
                                       ref bool validation,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   StaticModifier target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => ParseProperties(block, target, ctx, source, ref validation, allowUnknownNodes);
}