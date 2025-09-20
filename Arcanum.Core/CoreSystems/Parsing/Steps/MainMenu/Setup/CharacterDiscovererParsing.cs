using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class CharacterDiscovererParsing : DiscoverThenParseLoadingService<Character>
{
   public override List<Type> ParsedObjects => [typeof(Character)];

   public override Dictionary<string, Character> GetGlobals() => Globals.Characters;

   public override string[] GroupingNodeNames => ["character_db"];

   protected override void LoadSingleFileProperties(RootNode rn,
                                                    LocationContext ctx,
                                                    Eu5FileObj<Character> fileObj,
                                                    string actionStack,
                                                    string source,
                                                    ref bool validation,
                                                    object? lockObject)
   {
      // We do nothing as we only discover here.
   }
}