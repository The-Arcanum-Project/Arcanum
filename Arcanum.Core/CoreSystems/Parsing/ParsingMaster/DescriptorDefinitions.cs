using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using LanguageParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LanguageParsing;
using LocationRankParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LocationRankParsing;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   public static readonly FileDescriptor ColorParser = new([],
                                                           ["game", "main_menu", "common", "named_colors"],
                                                           ISavingService.Dummy,
                                                           new("colors", "txt", "#"),
                                                           new ColorParser(),
                                                           false);

   public static readonly FileDescriptor DefaultMapPreDescriptor = new([],
                                                                       ["game", "in_game", "map_data", "default.map"],
                                                                       ISavingService.Dummy,
                                                                       new("default.map", "map", "#"),
                                                                       new DefaultMapPreParsingStep(),
                                                                       false,
                                                                       uniqueId: 'P');

   public static readonly FileDescriptor DefaultMapDescriptor = new([DefaultMapPreDescriptor],
                                                                    ["game", "in_game", "map_data", "default.map"],
                                                                    ISavingService.Dummy,
                                                                    new("default.map", "map", "#"),
                                                                    new DefaultMapParsing(),
                                                                    false);

   public static readonly FileDescriptor LocationDescriptor = new([DefaultMapPreDescriptor, ColorParser],
                                                                  ["game", "in_game", "map_data", "named_locations"],
                                                                  ISavingService.Dummy,
                                                                  new("LocationsDefinition", "txt", "#"),
                                                                  new LocationFileLoading(),
                                                                  false);

   public static readonly FileDescriptor DefinitionsDescriptor = new([LocationDescriptor, DefaultMapPreDescriptor],
                                                                     ["game", "in_game", "map_data", "definitions.txt"],
                                                                     ISavingService.Dummy,
                                                                     new("definitions", "txt", "#"),
                                                                     new DefinitionFileLoading(),
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor AdjacenciesDescriptor = new([DefaultMapPreDescriptor, LocationDescriptor],
                                                                     ["game", "in_game", "map_data", "adjacencies.csv"],
                                                                     ISavingService.Dummy,
                                                                     new("Adjacencies", "csv", string.Empty),
                                                                     new AdjacencyFileLoading(),
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor MarketDescriptor = new([LocationDescriptor],
                                                                [
                                                                   "game", "main_menu", "setup", "start",
                                                                   "03_markets.txt",
                                                                ],
                                                                ISavingService.Dummy,
                                                                new("03_markets", "txt", "#"),
                                                                new MarketParsing(),
                                                                false,
                                                                false);

   public static readonly FileDescriptor PopTypeDescriptor = new([ColorParser],
                                                                 ["game", "in_game", "common", "pop_types"],
                                                                 ISavingService.Dummy,
                                                                 new("01_pop_types", "txt", "#"),
                                                                 new PopTypeParsing(),
                                                                 false);

   public static readonly FileDescriptor PopDescriptor = new([PopTypeDescriptor, LocationDescriptor, ColorParser],
                                                             ["game", "main_menu", "setup", "start", "06_pops.txt"],
                                                             ISavingService.Dummy,
                                                             new("06_pops", "txt", "#"),
                                                             new PopsParsing(),
                                                             false,
                                                             false);

   public static readonly FileDescriptor LocationRankDescriptor = new([ColorParser],
                                                                      ["game", "in_game", "common", "location_ranks"],
                                                                      ISavingService.Dummy,
                                                                      new("location_ranks", "txt", "#"),
                                                                      new LocationRankParsing(),
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor CountryRankDescriptor = new([ColorParser],
                                                                     ["game", "in_game", "common", "country_ranks"],
                                                                     ISavingService.Dummy,
                                                                     new("country_ranks", "txt", "#"),
                                                                     new CountryRankLoading(),
                                                                     false);

   public static readonly FileDescriptor ReligiousSchoolsDescriptor = new([ColorParser],
                                                                          [
                                                                             "game", "in_game", "common",
                                                                             "religious_schools",
                                                                          ],
                                                                          ISavingService.Dummy,
                                                                          new("religious_schools", "txt", "#"),
                                                                          new ReligiousSchoolsParsing(),
                                                                          false,
                                                                          false);

   public static readonly FileDescriptor InstitutionsDescriptor = new([],
                                                                      ["game", "in_game", "common", "institution",],
                                                                      ISavingService.Dummy,
                                                                      new("age_x_institutions", "txt", "#"),
                                                                      new InstitutionParsing(),
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor InstitutionsAndReligiousSchools =
      new([LocationDescriptor, ReligiousSchoolsDescriptor, InstitutionsDescriptor],
          ["game", "main_menu", "setup", "start", "02_core.txt",],
          ISavingService.Dummy,
          new("02_core", "txt", "#"),
          new
             InstitutionAndReligiousSchoolsParsing(),
          false,
          false);

   public static readonly FileDescriptor RoadsAndCountriesDescriptor =
      new([LocationDescriptor, CountryRankDescriptor, ReligiousSchoolsDescriptor, ColorParser],
          ["game", "main_menu", "setup", "start", "10_countries_and_roads.txt"],
          ISavingService.Dummy,
          new("10_countries_and_roads", "txt", "#"),
          new RoadsAndCountriesParsing(),
          false,
          false);

   public static readonly FileDescriptor CultureDescriptor = new([ColorParser],
                                                                 ["game", "in_game", "common", "cultures"],
                                                                 ISavingService.Dummy,
                                                                 new("cultures", "txt", "#"),
                                                                 new CultureParsing(),
                                                                 false);

   public static readonly FileDescriptor CultureAfterParsingDescriptor = new([CultureDescriptor, ColorParser],
                                                                             ["game", "in_game", "common", "cultures"],
                                                                             ISavingService.Dummy,
                                                                             new("cultures", "txt", "#"),
                                                                             new CultureAfterParsing(),
                                                                             false,
                                                                             uniqueId: 'B');

   public static readonly FileDescriptor LanguageDescriptor = new([ColorParser],
                                                                  ["game", "in_game", "common", "languages"],
                                                                  ISavingService.Dummy,
                                                                  new("languages", "txt", "#"),
                                                                  new LanguageParsing(),
                                                                  false);

   public static readonly FileDescriptor AgeDescriptor = new([],
                                                             ["game", "in_game", "common", "ages"],
                                                             ISavingService.Dummy,
                                                             new("ages", "txt", "#"),
                                                             new AgeParsing(),
                                                             false);

   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         DefaultMapPreDescriptor, LocationDescriptor, DefaultMapDescriptor, DefinitionsDescriptor,
         AdjacenciesDescriptor, MarketDescriptor, PopTypeDescriptor, PopDescriptor, LocationRankDescriptor,
         RoadsAndCountriesDescriptor, CountryRankDescriptor, InstitutionsAndReligiousSchools,
         ReligiousSchoolsDescriptor, InstitutionsDescriptor, CultureDescriptor, ColorParser,
         CultureAfterParsingDescriptor, LanguageDescriptor, AgeDescriptor,
      ];
   }
}