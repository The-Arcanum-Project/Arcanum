using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using PopType = Arcanum.Core.GameObjects.InGame.Pops.PopType;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class PopTypeDiscoverer(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<PopType>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   PopType target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("PopTypeDiscoverer should only be used in discovery phase.");
}

[ParserFor(typeof(PopType))]
public partial class PopTypesParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<PopType>(false, dependencies)
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
                                                   PopType target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}