using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ReligiousFaction = Arcanum.Core.GameObjects.Religious.ReligiousFaction;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.SubClasses;

public class ReligiousFactionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousFaction>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ReligiousFaction target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
   {
      throw new NotSupportedException("ReligiousFactionParsing should only be used in discovery phase.");
   }
}