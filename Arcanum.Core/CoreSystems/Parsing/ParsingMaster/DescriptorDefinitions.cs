using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using LanguageParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LanguageParsing;
using LocationRankParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LocationRankParsing;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   public static readonly FileDescriptor ColorParser = new([],
                                                           ["game", "main_menu", "common", "named_colors"],
                                                           new("colors", "txt", "#"),
                                                           new ColorParser(),
                                                           false);

   public static readonly FileDescriptor ModifierDefinitionDescriptor = new([ColorParser],
                                                                            [
                                                                               "game", "main_menu", "common",
                                                                               "modifier_type_definitions"
                                                                            ],
                                                                            new("modifiers", "txt", "#"),
                                                                            new Steps.MainMenu.Common.ModifierParsing(),
                                                                            false);

   public static readonly FileDescriptor DefaultMapPreDescriptor = new([],
                                                                       ["game", "in_game", "map_data", "default.map"],
                                                                       new("default.map", "map", "#"),
                                                                       new DefaultMapPreParsingStep(),
                                                                       false,
                                                                       uniqueId: 'P');

   public static readonly FileDescriptor DefaultMapDescriptor = new([DefaultMapPreDescriptor],
                                                                    ["game", "in_game", "map_data", "default.map"],
                                                                    new("default.map", "map", "#"),
                                                                    new DefaultMapParsing(),
                                                                    false);

   public static readonly FileDescriptor LocationDescriptor = new([DefaultMapPreDescriptor, ColorParser],
                                                                  ["game", "in_game", "map_data", "named_locations"],
                                                                  new("LocationsDefinition", "txt", "#"),
                                                                  new LocationFileLoading(),
                                                                  false);

   public static readonly FileDescriptor DefinitionsDescriptor = new([LocationDescriptor, DefaultMapPreDescriptor],
                                                                     ["game", "in_game", "map_data", "definitions.txt"],
                                                                     new("definitions", "txt", "#"),
                                                                     new DefinitionFileLoading(),
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor AdjacenciesDescriptor = new([DefaultMapPreDescriptor, LocationDescriptor],
                                                                     ["game", "in_game", "map_data", "adjacencies.csv"],
                                                                     new("Adjacencies", "csv", string.Empty),
                                                                     new AdjacencyFileLoading(),
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor MarketDescriptor = new([LocationDescriptor],
                                                                [
                                                                   "game", "main_menu", "setup", "start",
                                                                   "03_markets.txt",
                                                                ],
                                                                new("03_markets", "txt", "#"),
                                                                new MarketParsing(),
                                                                false,
                                                                false);

   public static readonly FileDescriptor PopTypeDescriptor = new([ColorParser],
                                                                 ["game", "in_game", "common", "pop_types"],
                                                                 new("01_pop_types", "txt", "#"),
                                                                 new PopTypeParsing(),
                                                                 false);

   public static readonly FileDescriptor PopDescriptor = new([PopTypeDescriptor, LocationDescriptor, ColorParser],
                                                             ["game", "main_menu", "setup", "start", "06_pops.txt"],
                                                             new("06_pops", "txt", "#"),
                                                             new PopsParsing(),
                                                             false,
                                                             false);

   public static readonly FileDescriptor LocationRankDescriptor = new([ColorParser, ModifierDefinitionDescriptor],
                                                                      ["game", "in_game", "common", "location_ranks"],
                                                                      new("location_ranks", "txt", "#"),
                                                                      new LocationRankParsing(),
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor CountryRankDescriptor = new([ColorParser, ModifierDefinitionDescriptor],
                                                                     ["game", "in_game", "common", "country_ranks"],
                                                                     new("country_ranks", "txt", "#"),
                                                                     new CountryRankLoading(),
                                                                     false);

   public static readonly FileDescriptor ReligiousSchoolsDescriptor = new([ColorParser],
                                                                          [
                                                                             "game", "in_game", "common",
                                                                             "religious_schools",
                                                                          ],
                                                                          new("religious_schools", "txt", "#"),
                                                                          new ReligiousSchoolsParsing(),
                                                                          false,
                                                                          false);

   public static readonly FileDescriptor InstitutionsDescriptor = new([],
                                                                      ["game", "in_game", "common", "institution",],
                                                                      new("age_x_institutions", "txt", "#"),
                                                                      new InstitutionParsing(),
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor InstitutionsAndReligiousSchools =
      new([LocationDescriptor, ReligiousSchoolsDescriptor, InstitutionsDescriptor],
          ["game", "main_menu", "setup", "start", "02_core.txt",],
          new("02_core", "txt", "#"),
          new
             InstitutionAndReligiousSchoolsParsing(),
          false,
          false);

   public static readonly FileDescriptor RoadsAndCountriesDescriptor =
      new([LocationDescriptor, CountryRankDescriptor, ReligiousSchoolsDescriptor, ColorParser],
          ["game", "main_menu", "setup", "start", "10_countries_and_roads.txt"],
          new("10_countries_and_roads", "txt", "#"),
          new RoadsAndCountriesParsing(),
          false,
          false);

   public static readonly FileDescriptor CultureDescriptor = new([ColorParser],
                                                                 ["game", "in_game", "common", "cultures"],
                                                                 new("cultures", "txt", "#"),
                                                                 new CultureParsing(),
                                                                 false);

   public static readonly FileDescriptor CultureAfterParsingDescriptor = new([CultureDescriptor, ColorParser],
                                                                             ["game", "in_game", "common", "cultures"],
                                                                             new("cultures", "txt", "#"),
                                                                             new CultureAfterParsing(),
                                                                             false,
                                                                             uniqueId: 'B');

   public static readonly FileDescriptor LanguageDescriptor = new([ColorParser],
                                                                  ["game", "in_game", "common", "languages"],
                                                                  new("languages", "txt", "#"),
                                                                  new LanguageParsing(),
                                                                  false);

   public static readonly FileDescriptor AgeDescriptor = new([ModifierDefinitionDescriptor],
                                                             ["game", "in_game", "common", "age"],
                                                             new("ages", "txt", "#"),
                                                             new AgeParsing(),
                                                             false);

   public static readonly FileDescriptor ClimateDescriptor = new([],
                                                                 ["game", "in_game", "common", "climates"],
                                                                 new("climates", "txt", "#"),
                                                                 new ClimateParsing(),
                                                                 false);

   public static readonly FileDescriptor VegetationDescriptor = new([],
                                                                    ["game", "in_game", "common", "vegetation"],
                                                                    new("vegetation", "txt", "#"),
                                                                    new VegetationParsing(),
                                                                    false);

   public static readonly FileDescriptor TopographyDescriptor = new([],
                                                                    ["game", "in_game", "common", "topography"],
                                                                    new("topography", "txt", "#"),
                                                                    new TopographyParsing(),
                                                                    false);

   public static readonly FileDescriptor RegenciesDescriptor = new([ModifierDefinitionDescriptor],
                                                                   ["game", "in_game", "common", "regencies"],
                                                                   new("regencies", "txt", "#"),
                                                                   new RegencyParsing(),
                                                                   true);

   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         DefaultMapPreDescriptor, LocationDescriptor, DefaultMapDescriptor, DefinitionsDescriptor,
         AdjacenciesDescriptor, MarketDescriptor, PopTypeDescriptor, PopDescriptor, LocationRankDescriptor,
         RoadsAndCountriesDescriptor, CountryRankDescriptor, InstitutionsAndReligiousSchools,
         ReligiousSchoolsDescriptor, InstitutionsDescriptor, CultureDescriptor, ColorParser,
         CultureAfterParsingDescriptor, LanguageDescriptor, AgeDescriptor, ClimateDescriptor, VegetationDescriptor,
         ModifierDefinitionDescriptor, TopographyDescriptor, RegenciesDescriptor,
      ];
   }
}