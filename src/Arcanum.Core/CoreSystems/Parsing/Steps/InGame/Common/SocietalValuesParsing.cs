using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(SocientalValue))]
public partial class SocientalValuesParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<SocientalValue>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block, SocientalValue target, ref ParsingContext pc, bool allowUnknownNodes)
      => ParseProperties(block, target, ref pc, allowUnknownNodes);
}