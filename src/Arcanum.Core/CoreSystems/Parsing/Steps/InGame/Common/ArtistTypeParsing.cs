using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ArtistType = Arcanum.Core.GameObjects.InGame.Cultural.ArtistType;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(ArtistType))]
public partial class ArtistTypeParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ArtistType>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ArtistType target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("ArtistTypeParsing does not support ParsePropertiesToObject.");
}