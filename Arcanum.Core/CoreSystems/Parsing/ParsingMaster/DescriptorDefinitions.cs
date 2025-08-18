using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   static DescriptorDefinitions()
   {
      FileDescriptor defaultMapPreDescriptor = new([],
                                                   ["game", "in_game", "map_data", "default.map"],
                                                   ISavingService.Dummy,
                                                   new("default.map", "map", "#"),
                                                   new DefaultMapPreParsingStep(),
                                                   false,
                                                   uniqueId: 'P');

      FileDescriptor defaultMapDescriptor = new([defaultMapPreDescriptor],
                                                ["game", "in_game", "map_data", "default.map"],
                                                ISavingService.Dummy,
                                                new("default.map", "map", "#"),
                                                new DefaultMapParsing(),
                                                false);

      FileDescriptor locationDescriptor = new([defaultMapPreDescriptor],
                                              ["game", "in_game", "map_data", "named_locations"],
                                              ISavingService.Dummy,
                                              new("LocationsDefinition", "txt", "#"),
                                              new LocationFileLoading(),
                                              false);

      FileDescriptor definitionsDescriptor = new([locationDescriptor, defaultMapPreDescriptor],
                                                 ["game", "in_game", "map_data", "definitions.txt"],
                                                 ISavingService.Dummy,
                                                 new("definitions", "txt", "#"),
                                                 new DefinitionFileLoading(),
                                                 false,
                                                 false);

      FileDescriptors = [defaultMapPreDescriptor, locationDescriptor, defaultMapDescriptor, definitionsDescriptor,];
   }
}