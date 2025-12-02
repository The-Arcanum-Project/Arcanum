using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class DesignateHeirReasonParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<DesignateHeirReason>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   DesignateHeirReason target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("DesignateHeirReasonParsing should only be used in discovery phase.");
}