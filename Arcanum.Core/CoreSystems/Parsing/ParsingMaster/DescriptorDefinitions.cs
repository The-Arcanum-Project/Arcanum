using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

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

   public static readonly FileDescriptor LocationDescriptor = new([DefaultMapPreDescriptor],
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
                                                                     ["game", "main_menu", "setup", "start", "03_markets.txt"],
                                                                     ISavingService.Dummy,
                                                                     new("03_markets", "txt", "#"),
                                                                     new MarketParsing(),
                                                                     false,
                                                                     false);
   
   public static readonly FileDescriptor PopTypeDescriptor = new([],
                                                                     ["game", "in_game", "common", "pop_types"],
                                                                     ISavingService.Dummy,
                                                                     new("01_pop_types", "txt", "#"),
                                                                     new PopTypeParsing(),
                                                                     false);
   
   public static readonly FileDescriptor PopDescriptor = new([PopTypeDescriptor, LocationDescriptor],
                                                                     ["game", "main_menu", "setup", "start", "06_pops.txt"],
                                                                     ISavingService.Dummy,
                                                                     new("06_pops", "txt", "#"),
                                                                     new PopsParsing(),
                                                                     false,
                                                                     false);

   public static readonly FileDescriptor LocationRankDescriptor = new([],
                                                                     ["game", "in_game", "common", "location_ranks"],
                                                                     ISavingService.Dummy,
                                                                     new("location_ranks", "txt", "#"),
                                                                     new LocationRankLoading(),
                                                                     false,
                                                                     false);
   
   public static readonly FileDescriptor RoadsAndCountriesDescriptor = new([LocationDescriptor],
                                                                     ["game", "main_menu", "setup", "start", "10_countries_and_roads.txt"],
                                                                     ISavingService.Dummy,
                                                                     new("10_countries_and_roads", "txt", "#"),
                                                                     new RoadsAndCountriesParsing(),
                                                                     false,
                                                                     false);
   
   public static readonly FileDescriptor CountryRankDescriptor = new([],
                                                                     ["game", "in_game", "common", "country_ranks"],
                                                                     ISavingService.Dummy,
                                                                     new("country_ranks", "txt", "#"),
                                                                     new CountryRankLoading(),
                                                                     false);
   
   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         DefaultMapPreDescriptor, LocationDescriptor, DefaultMapDescriptor, DefinitionsDescriptor,
         AdjacenciesDescriptor, MarketDescriptor, PopTypeDescriptor, PopDescriptor, LocationRankDescriptor,
         RoadsAndCountriesDescriptor, CountryRankDescriptor,
      ];
   }
}