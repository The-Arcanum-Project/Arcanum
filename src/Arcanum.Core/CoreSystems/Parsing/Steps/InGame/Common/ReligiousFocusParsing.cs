using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ReligiousFocus = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousFocus;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class ReligiousFocusParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousFocus>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ReligiousFocus target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("ReligiousFocusParsing should only be used in discovery phase.");
}