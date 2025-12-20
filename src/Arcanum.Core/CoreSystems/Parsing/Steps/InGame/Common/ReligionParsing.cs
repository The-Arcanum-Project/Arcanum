using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Religion = Arcanum.Core.GameObjects.InGame.Religious.Religion;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class ReligionDiscovererParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Religion>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Religion target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
   {
      throw new NotSupportedException("ReligionDiscovererParsing should only be used in discovery phase.");
   }
}

[ParserFor(typeof(Religion), IgnoredBlockKeys = ["max_religious_figures_for_religion"])]
public partial class ReligionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Religion>(false, dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.ParseDiscoveredObjectProperties(rn,
                                                         ref pc,
                                                         ParseProperties,
                                                         GetGlobals());
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Religion target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}