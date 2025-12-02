using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Building))]
public partial class BuildingParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Building>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Building target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
   {
   }
}