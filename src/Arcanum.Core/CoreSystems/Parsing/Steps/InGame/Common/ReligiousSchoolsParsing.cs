using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ReligiousSchool = Arcanum.Core.GameObjects.Religious.ReligiousSchool;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(ReligiousSchool))]
public partial class ReligiousSchoolsParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousSchool>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ReligiousSchool target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("ReligiousSchoolsParsing should only be used in discovery phase.");
}