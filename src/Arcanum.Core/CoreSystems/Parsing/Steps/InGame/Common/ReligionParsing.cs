using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Religion = Arcanum.Core.GameObjects.Religious.Religion;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class ReligionDiscovererParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Religion>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Religion target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
   {
      throw new NotSupportedException("ReligionDiscovererParsing should only be used in discovery phase.");
   }
}

[ParserFor(typeof(Religion), IgnoredBlockKeys = ["max_religious_figures_for_religion"])]
public partial class ReligionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Religion>(false, dependencies)
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      SimpleObjectParser.ParseDiscoveredObjectProperties(rn,
                                                         ctx,
                                                         actionStack,
                                                         source,
                                                         ref validation,
                                                         ParseProperties,
                                                         GetGlobals());
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Religion target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => ParseProperties(block, target, ctx, source, ref validation, allowUnknownNodes);
}