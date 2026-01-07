using Arcanum.Core.CoreSystems.Parsing.Steps.InGame;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.SubClasses;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.GFX.Map;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Setup;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Common.UI;
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
   private static FileLoadingService[] ConsequentialLoadingSteps(List<FileLoadingService> steps)
   {
      for (var index = 1; index < steps.Count; index++)
         steps[index].Dependencies = steps[index].Dependencies.Append(steps[index - 1]).ToArray();

      return steps.ToArray();
   }

   public static List<FileDescriptor> FileDescriptors { get; }
   public static List<FileLoadingService> LoadingStepsList { get; }

   public static readonly FileDescriptor ColorParser = new(["main_menu", "common", "named_colors"],
                                                           new("colors", "txt", "#"),
                                                           [new ColorParser([])],
                                                           false);

   public static readonly FileDescriptor ModifierDefinitionDescriptor =
      new(["main_menu", "common", "modifier_type_definitions"],
          new("modifiers", "txt", "#"),
          [new ModifierParsing([ColorParser.LoadingService[0]])],
          false);

   public static readonly FileDescriptor DesignateHeirReasonDescriptor =
      new(["in_game", "common", "designated_heir_reason"],
          new("designated_heir_reason", "txt", "#"),
          [new DesignateHeirReasonParsing([])],
          false);

   public static readonly FileDescriptor BuildingDescriptor =
      new(["in_game", "common", "building_types"],
          new("buildings", "txt", "#"),
          [new BuildingParsing([])],
          false);

   public static readonly FileDescriptor TraitDescriptor = new(["in_game", "common", "traits"],
                                                               new("traits", "txt", "#"),
                                                               [new TraitParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                               false);

   public static readonly FileDescriptor ParliamentTypeParsingDescriptor =
      new(["in_game", "common", "parliament_types"],
          new("parliament_types", "txt", "#"),
          [new ParliamentTypeParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
          false);

   private static readonly DefaultMapPreParsingStep DefaultMapPreParsing = new([]);

   public static readonly FileDescriptor LocationDescriptor = new(["in_game", "map_data", "named_locations"],
                                                                  new("LocationsDefinition", "txt", "#"),
                                                                  [new LocationFileLoading([DefaultMapPreParsing, ColorParser.LoadingService[0]])],
                                                                  false);

   public static readonly FileDescriptor DefaultMapPreDescriptor = new(["in_game", "map_data", "default.map"],
                                                                       new("default.map", "map", "#"),
                                                                       ConsequentialLoadingSteps([
                                                                          DefaultMapPreParsing, new DefaultMapParsing([LocationDescriptor.LoadingService[0]]),
                                                                       ]),
                                                                       false);

   public static readonly FileDescriptor MapTracingDescriptor = new(["in_game", "map_data", "provinces.png"],
                                                                    new("LocationMap", "png", ""),
                                                                    [new LocationMapTracing([DefaultMapPreParsing, LocationDescriptor.LoadingService[0]])],
                                                                    false);

   public static readonly FileDescriptor DefinitionsDescriptor = new(["in_game", "map_data", "definitions.txt"],
                                                                     new("definitions", "txt", "#"),
                                                                     [
                                                                        new DefinitionsParsing([
                                                                           LocationDescriptor.LoadingService[0], DefaultMapPreDescriptor.LoadingService[0],
                                                                        ]),
                                                                     ],
                                                                     false);

   public static readonly FileDescriptor GameObjecLocatorsDescriptor = new(["in_game", "gfx", "map", "map_objects"],
                                                                           new("CityLocators", "txt", string.Empty),
                                                                           [new CityGameObjectLocatorParsing([LocationDescriptor.LoadingService[0]])],
                                                                           true,
                                                                           [
                                                                              "generated_map_object_locators_city.txt",
                                                                              "generated_map_object_locators_combat.txt",
                                                                              "generated_map_object_locators_volcano_eruption.txt",
                                                                              "generated_map_object_locators_vfx.txt",
                                                                              "generated_map_object_locators_unit_stack.txt",
                                                                           ],
                                                                           IO.IO.Windows1250Encoding);

   public static readonly FileDescriptor AdjacenciesDescriptor = new(["in_game", "map_data", "adjacencies.csv"],
                                                                     new("Adjacencies", "csv", string.Empty),
                                                                     [new AdjacencyFileLoading([LocationDescriptor.LoadingService[0]])],
                                                                     false);

   public static readonly FileDescriptor EstateDescriptor = new(["in_game", "common", "estates"],
                                                                new("estates", "txt", "#"),
                                                                [new EstateParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                                false);

   public static readonly FileDescriptor PopTypeDescriptor = new(["in_game", "common", "pop_types"],
                                                                 new("01_pop_types", "txt", "#"),
                                                                 ConsequentialLoadingSteps([
                                                                    new PopTypeDiscoverer([]),
                                                                    new PopTypesParsing([
                                                                       ColorParser.LoadingService[0], ModifierDefinitionDescriptor.LoadingService[0],
                                                                       EstateDescriptor.LoadingService[0],
                                                                    ]),
                                                                 ]),
                                                                 false);

   public static readonly FileDescriptor LanguageDescriptor = new(["in_game", "common", "languages"],
                                                                  new("languages", "txt", "#"),
                                                                  [new LanguageParsing([ColorParser.LoadingService[0]])],
                                                                  false);

   private static readonly CultureParsing CultureDiscovery =
      new([ColorParser.LoadingService[0], LanguageDescriptor.LoadingService[0]]);

   public static readonly FileDescriptor LocationRankDescriptor = new(["in_game", "common", "location_ranks"],
                                                                      new("location_ranks", "txt", "#"),
                                                                      [
                                                                         new LocationRankParsing([
                                                                            ColorParser.LoadingService[0], ModifierDefinitionDescriptor
                                                                              .LoadingService[0],
                                                                         ]),
                                                                      ],
                                                                      false);

   public static readonly FileDescriptor CountryRankDescriptor = new(["in_game", "common", "country_ranks"],
                                                                     new("country_ranks", "txt", "#"),
                                                                     [
                                                                        new CountryRankLoading([
                                                                           ColorParser.LoadingService[0], ModifierDefinitionDescriptor
                                                                             .LoadingService[0],
                                                                        ]),
                                                                     ],
                                                                     false);

   public static readonly FileDescriptor ReligiousSchoolsDescriptor =
      new(["in_game", "common", "religious_schools"],
          new("religious_schools", "txt", "#"),
          [new ReligiousSchoolsParsing([ColorParser.LoadingService[0]])],
          false);

   public static readonly FileDescriptor InstitutionsDescriptor = new(["in_game", "common", "institution"],
                                                                      new("age_x_institutions", "txt", "#"),
                                                                      [new InstitutionParsing([])],
                                                                      false);

   public static readonly FileDescriptor CultureGroupDescriptor = new(["in_game", "common", "culture_groups"],
                                                                      new("culture_groups", "txt", "#"),
                                                                      [
                                                                         new CultureGroupParsing([
                                                                            CultureDiscovery, ModifierDefinitionDescriptor
                                                                              .LoadingService[0],
                                                                         ]),
                                                                      ],
                                                                      false);

   public static readonly FileDescriptor CultureDescriptor = new(["in_game", "common", "cultures"],
                                                                 new("cultures", "txt", "#"),
                                                                 ConsequentialLoadingSteps([
                                                                    CultureDiscovery,
                                                                    new CultureAfterParsing([
                                                                       CultureGroupDescriptor.LoadingService[0], ColorParser.LoadingService[0],
                                                                       LanguageDescriptor.LoadingService[0],
                                                                    ]),
                                                                 ]),
                                                                 false);

   public static readonly FileDescriptor AgeDescriptor = new(["in_game", "common", "age"],
                                                             new("ages", "txt", "#"),
                                                             [
                                                                new AgeParsing([
                                                                   ModifierDefinitionDescriptor.LoadingService[0], EstateDescriptor.LoadingService[0],
                                                                ]),
                                                             ],
                                                             false);

   public static readonly FileDescriptor ClimateDescriptor = new(["in_game", "common", "climates"],
                                                                 new("climates", "txt", "#"),
                                                                 [new ClimateParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                                 false);

   public static readonly FileDescriptor VegetationDescriptor = new(["in_game", "common", "vegetation"],
                                                                    new("vegetation", "txt", "#"),
                                                                    [new VegetationParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                                    false);

   public static readonly FileDescriptor TopographyDescriptor = new(["in_game", "common", "topography"],
                                                                    new("topography", "txt", "#"),
                                                                    [new TopographyParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                                    false);

   public static readonly FileDescriptor RegenciesDescriptor = new(["in_game", "common", "regencies"],
                                                                   new("regencies", "txt", "#"),
                                                                   [new RegencyParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
                                                                   true);

   public static readonly FileDescriptor ReligiousFactionParsing = new(["in_game", "common", "religious_factions"],
                                                                       new("religious_factions", "txt", "#"),
                                                                       [new ReligiousFactionParsing([])],
                                                                       false);

   public static readonly FileDescriptor ReligiousGroupDescriptor = new(["in_game", "common", "religion_groups"],
                                                                        new("religion_groups", "txt", "#"),
                                                                        [
                                                                           new ReligionGroupParsing([
                                                                              ColorParser.LoadingService[0],
                                                                              ModifierDefinitionDescriptor
                                                                                .LoadingService[0],
                                                                           ]),
                                                                        ],
                                                                        false);

   public static readonly FileDescriptor ReligiousFocusParsing = new(["in_game", "common", "religious_focuses"],
                                                                     new("religious_focuses", "txt", "#"),
                                                                     [new ReligiousFocusParsing([])],
                                                                     false);

   public static readonly FileDescriptor ReligionDescriptor = new(["in_game", "common", "religions"],
                                                                  new("religions", "txt", "#"),
                                                                  [
                                                                     new ReligionDiscovererParsing([]), new ReligionParsing([
                                                                        ReligiousGroupDescriptor.LoadingService[0], ModifierDefinitionDescriptor
                                                                          .LoadingService[0],
                                                                        ColorParser.LoadingService[0], LanguageDescriptor.LoadingService[0],
                                                                        ReligiousFactionParsing.LoadingService[0],
                                                                        // TODO: CountriesDescriptor.LoadingService[0],
                                                                        ReligiousFocusParsing.LoadingService[0],
                                                                     ]),
                                                                  ],
                                                                  false);

   public static readonly FileDescriptor RawMaterialDescriptor = new(["in_game", "common", "goods"],
                                                                     new("raw_materials", "txt", "#"),
                                                                     [new RawMaterialParsing([PopTypeDescriptor.LoadingService[1]])],
                                                                     false);

   public static readonly FileDescriptor SocietalValuesDescriptor =
      new(["in_game", "common", "societal_values"],
          new("societal_values", "txt", "#"),
          [new SocientalValuesParsing([])],
          false);

   public static readonly FileDescriptor StaticModifiersDescriptor =
      new(["main_menu", "common", "static_modifiers"],
          new("static_modifiers", "txt", "#"),
          [new StaticModifierParsing([ModifierDefinitionDescriptor.LoadingService[0]])],
          false);

   public static readonly FileDescriptor LocationTemplateDescriptor =
      new(["in_game", "map_data", "location_templates.txt"],
          new("location_templates", "txt", "#"),
          [
             new LocationTemplateParsing([
                LocationDescriptor.LoadingService[0], ClimateDescriptor.LoadingService[0], VegetationDescriptor.LoadingService[0],
                TopographyDescriptor.LoadingService[0], ReligionDescriptor.LoadingService[0], CultureDescriptor.LoadingService[0],
                RawMaterialDescriptor.LoadingService[0], StaticModifiersDescriptor.LoadingService[0],
             ]),
          ],
          false);

   public static readonly FileDescriptor TownSetupDescriptor =
      new(["in_game", "common", "town_setups"],
          new("town_setup", "txt", "#"),
          [new TownSetupParsing([BuildingDescriptor.LoadingService[0]])],
          false);

   public static readonly FileDescriptor ArtistTypeDescriptor = new(["in_game", "common", "artist_types"],
                                                                    new("artist_types", "txt", "#"),
                                                                    [new ArtistTypeParsing([])],
                                                                    false);

   public static readonly FileDescriptor CountryDefinitionDescriptor =
      new(["in_game", "setup", "countries"],
          new("country_definitions", "txt", "#"),
          [new CountryDefinitionParsing([ReligionDescriptor.LoadingService[0], CultureDescriptor.LoadingService[0], LocationDescriptor.LoadingService[0]])],
          false);

   public static readonly FileDescriptor MainMenuSetupParsingDescriptor =
      new(["main_menu", "setup", "start"],
          new("main_menu_setup", "txt", "#"),
          [
             new SetupParsingStep([
                CountryDefinitionDescriptor.LoadingService[0], LocationDescriptor.LoadingService[0], EstateDescriptor.LoadingService[0],
                TraitDescriptor.LoadingService[0], ReligionDescriptor.LoadingService[0], CultureDescriptor.LoadingService[0],
                ArtistTypeDescriptor.LoadingService[0], PopTypeDescriptor.LoadingService[0], ColorParser.LoadingService[0],
                ReligiousSchoolsDescriptor.LoadingService[0], InstitutionsDescriptor.LoadingService[0], CountryRankDescriptor.LoadingService[0],
                LanguageDescriptor.LoadingService[0], DesignateHeirReasonDescriptor.LoadingService[0], ParliamentTypeParsingDescriptor.LoadingService[0],
                InstitutionsDescriptor.LoadingService[0], SocietalValuesDescriptor.LoadingService[0],
             ]),
          ],
          false);

   //TODO Autogenerate this list
   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         DefaultMapPreDescriptor, LocationDescriptor, MapTracingDescriptor, DefinitionsDescriptor, AdjacenciesDescriptor, PopTypeDescriptor,
         LocationRankDescriptor, CountryRankDescriptor, ReligiousSchoolsDescriptor, InstitutionsDescriptor, CultureDescriptor, ColorParser,
         LanguageDescriptor, AgeDescriptor, ClimateDescriptor, VegetationDescriptor, ModifierDefinitionDescriptor, TopographyDescriptor, RegenciesDescriptor,
         EstateDescriptor, ReligiousGroupDescriptor, ReligionDescriptor, TownSetupDescriptor, ReligiousFactionParsing, ReligiousFocusParsing,
         DesignateHeirReasonDescriptor, TraitDescriptor, ParliamentTypeParsingDescriptor, RawMaterialDescriptor, LocationTemplateDescriptor,
         BuildingDescriptor, StaticModifiersDescriptor, CultureGroupDescriptor, ArtistTypeDescriptor, CountryDefinitionDescriptor,
         MainMenuSetupParsingDescriptor, SocietalValuesDescriptor, GameObjecLocatorsDescriptor,
      ];

      LoadingStepsList = new(FileDescriptors.Count);

      foreach (var descriptor in FileDescriptors)
      {
         var loadingSteps = descriptor.LoadingService;
         LoadingStepsList.AddRange(loadingSteps);
      }

      UIHandle.Instance.MainWindowsHandle.OnOpenMainMenuScreen += () =>
      {
         foreach (var descriptor in FileDescriptors)
            descriptor.Reset();
      };

      foreach (var t in from descriptor in FileDescriptors
                        from t in descriptor.LoadingService.SelectMany(fs => fs.ParsedObjects).Distinct()
                        where !TypeToDescriptor.TryAdd(t, descriptor)
                        select t)
         throw new InvalidOperationException($"Type {t.FullName} is already registered to a FileDescriptor.");
   }

   public static readonly Dictionary<Type, FileDescriptor> TypeToDescriptor = new();
}