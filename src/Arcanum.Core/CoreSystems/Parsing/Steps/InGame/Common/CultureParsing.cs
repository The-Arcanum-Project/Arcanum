using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Cultural;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class CultureParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Culture>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Culture target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("CultureParsing should only be used in discovery phase.");
}

[ParserFor(typeof(Culture))]
public partial class CultureAfterParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Culture>(false, dependencies)
{
   protected override void LoadSingleFileProperties(RootNode rn,
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
                                                   Culture target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}