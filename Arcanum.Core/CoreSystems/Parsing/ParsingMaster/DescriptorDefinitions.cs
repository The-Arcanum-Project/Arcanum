using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   static DescriptorDefinitions()
   {
      FileDescriptor defaultMapDescriptor = new([],
                                                ["game", "in_game", "map_data", "default.map"],
                                                ISavingService.Dummy,
                                                new("default.map", "map", "#"),
                                                new DefaultMapParsing(),
                                                false);
      
      FileDescriptor locationDescriptor = new([defaultMapDescriptor],
                                              ["game", "in_game", "map_data", "named_locations"],
                                              ISavingService.Dummy,
                                              new("LocationsDefinition", "txt", "#"),
                                              new LocationFileLoading(),
                                              false);


      FileDescriptor definitionsDescriptor = new([locationDescriptor, defaultMapDescriptor],
                                                 ["game", "in_game", "map_data", "definitions.txt"],
                                                 ISavingService.Dummy,
                                                 new("definitions", "txt", "#"),
                                                 new DefinitionFileLoading(),
                                                 false,
                                                 false);

      FileDescriptors =
      [
         locationDescriptor,
         defaultMapDescriptor,
         definitionsDescriptor,
        
      ];
   }
}