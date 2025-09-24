using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class CultureParsing() : DiscoverThenParseLoadingService<Culture>(true);

[ParserFor(typeof(Culture))]
public partial class CultureAfterParsing() : DiscoverThenParseLoadingService<Culture>(false)
{
   protected override void LoadSingleFileProperties(RootNode rn,
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
}