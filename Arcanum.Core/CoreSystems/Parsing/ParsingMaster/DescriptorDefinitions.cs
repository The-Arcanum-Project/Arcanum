using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.Utils.Sorting;
using LanguageParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LanguageParsing;
using LocationRankParsing = Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.LocationRankParsing;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   /// <summary>
   /// Use this to define multiple loading steps that have to be executed after each other in a specific order.
   /// </summary>
   /// <description>
   /// The input is a list of loading steps, each consequent step will automatically depend on the previous one.
   /// Therefore, each step only has to define its own dependencies, the dependency to the previous step is added automatically.
   /// </description>
   /// <param name="steps"></param>
   /// <returns></returns>
   private static FileLoadingService[] ConsequentialLoadingSteps(List<FileLoadingService> steps)
   {
      for (var index = 1; index < steps.Count; index++)
      {
         steps[index].Dependencies = steps[index].Dependencies.Append(steps[index - 1]);
      }

      return steps.ToArray();
   }

   public static List<FileDescriptor> FileDescriptors { get; }
   public static List<FileLoadingService> LoadingStepsList { get; }

   public static readonly FileDescriptor ColorParser = new(["game", "main_menu", "common", "named_colors"],
                                                           new("colors", "txt", "#"),
                                                           [new ColorParser([])],
                                                           false);

   public static readonly FileDescriptor ModifierDefinitionDescriptor =
      new(["game", "main_menu", "common", "modifier_type_definitions"],
          new("modifiers", "txt", "#"),
          [new ModifierParsing([ColorParser.LoadingService[0]])],
          false);

   private static readonly DefaultMapPreParsingStep DefaultMapPreParsing = new([]);

   public static readonly FileDescriptor LocationDescriptor = new(["game", "in_game", "map_data", "named_locations"],
                                                                  new("LocationsDefinition", "txt", "#"),
                                                                  [
                                                                     new LocationFileLoading([
                                                                        DefaultMapPreParsing,
                                                                        ColorParser.LoadingService[0],
                                                                     ]),
                                                                  ],
                                                                  false);

   public static readonly FileDescriptor DefaultMapPreDescriptor = new(["game", "in_game", "map_data", "default.map"],
                                                                       new("default.map", "map", "#"),
                                                                       ConsequentialLoadingSteps([
                                                                          DefaultMapPreParsing,
                                                                          new DefaultMapParsing([
                                                                             LocationDescriptor.LoadingService[0],
                                                                          ]),
                                                                       ]),
                                                                       false);

   public static readonly FileDescriptor DefinitionsDescriptor = new(["game", "in_game", "map_data", "definitions.txt"],
                                                                     new("definitions", "txt", "#"),
                                                                     [
                                                                        new DefinitionsParsing([
                                                                           LocationDescriptor.LoadingService[0],
                                                                           DefaultMapPreDescriptor.LoadingService[0],
                                                                        ]),
                                                                     ],
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor AdjacenciesDescriptor = new(["game", "in_game", "map_data", "adjacencies.csv"],
                                                                     new("Adjacencies", "csv", string.Empty),
                                                                     [new AdjacencyFileLoading([])],
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor MarketDescriptor =
      new(["game", "main_menu", "setup", "start", "03_markets.txt"],
          new("03_markets", "txt", "#"),
          [new MarketManagerParsing([LocationDescriptor.LoadingService[0]])],
          false,
          false);

   public static readonly FileDescriptor EstateDescriptor = new(["game", "in_game", "common", "estates"],
                                                                new("estates", "txt", "#"),
                                                                [new EstateParsing([])],
                                                                false);

   public static readonly FileDescriptor PopTypeDescriptor = new(["game", "in_game", "common", "pop_types"],
                                                                 new("01_pop_types", "txt", "#"),
                                                                 ConsequentialLoadingSteps([
                                                                    new PopTypeDiscoverer([]),
                                                                    new PopTypesParsing([
                                                                       ColorParser.LoadingService[0],
                                                                       ModifierDefinitionDescriptor.LoadingService[0],
                                                                       EstateDescriptor.LoadingService[0],
                                                                    ]),
                                                                 ]),
                                                                 false);

   public static readonly FileDescriptor LanguageDescriptor = new(["game", "in_game", "common", "languages"],
                                                                  new("languages", "txt", "#"),
                                                                  [
                                                                     new LanguageParsing([
                                                                        ColorParser.LoadingService[0],
                                                                     ]),
                                                                  ],
                                                                  false);

   private static readonly CultureParsing CultureDiscovery =
      new([ColorParser.LoadingService[0], LanguageDescriptor.LoadingService[0]]);

   public static readonly FileDescriptor PopDescriptor = new(["game", "main_menu", "setup", "start", "06_pops.txt"],
                                                             new("06_pops", "txt", "#"),
                                                             [
                                                                new PopsParsing([
                                                                   PopTypeDescriptor.LoadingService[0],
                                                                   CultureDiscovery,
                                                                   LocationDescriptor.LoadingService[0],
                                                                   ColorParser.LoadingService[0],
                                                                ]),
                                                             ],
                                                             false,
                                                             false);

   public static readonly FileDescriptor LocationRankDescriptor = new(["game", "in_game", "common", "location_ranks"],
                                                                      new("location_ranks", "txt", "#"),
                                                                      [
                                                                         new LocationRankParsing([
                                                                            ColorParser.LoadingService[0],
                                                                            ModifierDefinitionDescriptor
                                                                              .LoadingService[0],
                                                                         ]),
                                                                      ],
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor CountryRankDescriptor = new(["game", "in_game", "common", "country_ranks"],
                                                                     new("country_ranks", "txt", "#"),
                                                                     [
                                                                        new CountryRankLoading([
                                                                           ColorParser.LoadingService[0],
                                                                           ModifierDefinitionDescriptor
                                                                             .LoadingService[0],
                                                                        ]),
                                                                     ],
                                                                     false);

   public static readonly FileDescriptor ReligiousSchoolsDescriptor =
      new(["game", "in_game", "common", "religious_schools"],
          new("religious_schools", "txt", "#"),
          [new ReligiousSchoolsParsing([ColorParser.LoadingService[0]])],
          false,
          false);

   public static readonly FileDescriptor InstitutionsDescriptor = new(["game", "in_game", "common", "institution"],
                                                                      new("age_x_institutions", "txt", "#"),
                                                                      [new InstitutionParsing([])],
                                                                      false,
                                                                      false);

   public static readonly FileDescriptor InstitutionsAndReligiousSchools =
      new(["game", "main_menu", "setup", "start", "02_core.txt"],
          new("02_core", "txt", "#"),
          [
             new InstitutionStateReligiousSchoolStateParsing([
                LocationDescriptor.LoadingService[0], ReligiousSchoolsDescriptor.LoadingService[0],
                InstitutionsDescriptor.LoadingService[0],
             ]),
          ],
          false,
          false);

   private static readonly FileLoadingService CharacterDiscovery = new CharacterDiscovererParsing([]);

   public static readonly FileDescriptor RoadsAndCountriesDescriptor =
      new(["game", "main_menu", "setup", "start", "10_countries_and_roads.txt"],
          new("10_countries_and_roads", "txt", "#"),
          [
             new RoadsAndCountriesParsing([
                LocationDescriptor.LoadingService[0], CountryRankDescriptor.LoadingService[0],
                ReligiousSchoolsDescriptor.LoadingService[0], ColorParser.LoadingService[0], CharacterDiscovery,
                LanguageDescriptor.LoadingService[0],
             ]),
          ],
          false,
          false);

   public static readonly FileDescriptor CharactersDiscoveryDescriptor =
      new(["game", "main_menu", "setup", "start", "05_characters.txt"],
          new("characters", "txt", "#"),
          ConsequentialLoadingSteps([
             CharacterDiscovery,
             new CharacterPropertiesParsing([
                ColorParser.LoadingService[0], LocationDescriptor.LoadingService[0],
                RoadsAndCountriesDescriptor.LoadingService[0],
             ]),
          ]),
          false);

   public static readonly FileDescriptor CultureDescriptor = new(["game", "in_game", "common", "cultures"],
                                                                 new("cultures", "txt", "#"),
                                                                 ConsequentialLoadingSteps([
                                                                    CultureDiscovery,
                                                                    new CultureAfterParsing([
                                                                       ColorParser.LoadingService[0],
                                                                       LanguageDescriptor.LoadingService[0],
                                                                    ]),
                                                                 ]),
                                                                 false);

   public static readonly FileDescriptor AgeDescriptor = new(["game", "in_game", "common", "age"],
                                                             new("ages", "txt", "#"),
                                                             [
                                                                new AgeParsing([
                                                                   ModifierDefinitionDescriptor.LoadingService[0],
                                                                ]),
                                                             ],
                                                             false);

   public static readonly FileDescriptor ClimateDescriptor = new(["game", "in_game", "common", "climates"],
                                                                 new("climates", "txt", "#"),
                                                                 [new ClimateParsing([])],
                                                                 false);

   public static readonly FileDescriptor VegetationDescriptor = new(["game", "in_game", "common", "vegetation"],
                                                                    new("vegetation", "txt", "#"),
                                                                    [new VegetationParsing([])],
                                                                    false);

   public static readonly FileDescriptor TopographyDescriptor = new(["game", "in_game", "common", "topography"],
                                                                    new("topography", "txt", "#"),
                                                                    [new TopographyParsing([])],
                                                                    false);

   public static readonly FileDescriptor RegenciesDescriptor = new(["game", "in_game", "common", "regencies"],
                                                                   new("regencies", "txt", "#"),
                                                                   [
                                                                      new RegencyParsing([
                                                                         ModifierDefinitionDescriptor.LoadingService[0],
                                                                      ]),
                                                                   ],
                                                                   true);

   public static readonly FileDescriptor DynastyManagerDescriptor =
      new(["game", "main_menu", "setup", "start", "04_dynasties.txt"],
          new("04_dynasties", "txt", "#"),
          [new DynastyManagerParsing([LocationDescriptor.LoadingService[0],]),],
          false,
          false);

   //TODO Autogenerate this list
   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         DefaultMapPreDescriptor, LocationDescriptor, DefinitionsDescriptor, AdjacenciesDescriptor, MarketDescriptor,
         PopTypeDescriptor, PopDescriptor, LocationRankDescriptor, RoadsAndCountriesDescriptor,
         CountryRankDescriptor, InstitutionsAndReligiousSchools, ReligiousSchoolsDescriptor, InstitutionsDescriptor,
         CultureDescriptor, ColorParser, LanguageDescriptor, AgeDescriptor, ClimateDescriptor, VegetationDescriptor,
         ModifierDefinitionDescriptor, TopographyDescriptor, RegenciesDescriptor, CharactersDiscoveryDescriptor,
         DynastyManagerDescriptor, EstateDescriptor,
      ];

      LoadingStepsList = new(FileDescriptors.Count);

      foreach (var descriptor in FileDescriptors)
      {
         var loadingSteps = descriptor.LoadingService;
         LoadingStepsList.AddRange(loadingSteps);
      }
   }
}