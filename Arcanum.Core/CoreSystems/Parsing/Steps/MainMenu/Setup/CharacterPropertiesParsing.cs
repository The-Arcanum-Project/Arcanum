using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Court;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class CharacterDiscovererParsing() : DiscoverThenParseLoadingService<Character>(true)
{
   public override string[] GroupingNodeNames => ["character_db"];
}

[ParserFor(typeof(Character))]
public partial class CharacterPropertiesParsing() : DiscoverThenParseLoadingService<Character>(false)
{
   private const string GROUPING_NODE_KEY = "character_db";

   protected override void LoadSingleFileProperties(RootNode rn,
                                                    LocationContext ctx,
                                                    Eu5FileObj<Character> fileObj,
                                                    string actionStack,
                                                    string source,
                                                    ref bool validation,
                                                    object? lockObject)
   {
      if (!SimpleObjectParser.StripGroupingNodes(rn,
                                                 ctx,
                                                 actionStack,
                                                 source,
                                                 ref validation,
                                                 GROUPING_NODE_KEY,
                                                 out var sns))
         return;

      SimpleObjectParser.ParseDiscoveredObjectProperties(sns,
                                                         ctx,
                                                         actionStack,
                                                         source,
                                                         ref validation,
                                                         ParseProperties,
                                                         GetGlobals());
   }
}