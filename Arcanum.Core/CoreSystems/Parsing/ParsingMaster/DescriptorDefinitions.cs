using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   static DescriptorDefinitions()
   {
      FileDescriptor locationDescriptor = new([],
                                              ["game", "in_game", "map_data", "named_locations"],
                                              ISavingService.Dummy,
                                              new("LocationsDefinition", "txt", "#"),
                                              new LocationFileLoading(),
                                              false);

      FileDescriptors =
      [
         locationDescriptor,
         // Definitions loading: Province, Area, Region, SuperRegion
         new([locationDescriptor],
             ["game", "in_game", "map_data", "definitions.txt"],
             ISavingService.Dummy,
             new("Definitions", "txt", "#"),
             new DefinitionFileLoading(),
             false,
             false),
      ];
   }
}