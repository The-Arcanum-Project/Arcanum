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
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   StaticModifier target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}