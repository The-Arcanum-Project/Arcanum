using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public static class DescriptorDefinitions
{
   public static List<FileDescriptor> FileDescriptors { get; }

   static DescriptorDefinitions()
   {
      FileDescriptors =
      [
         new([],
             ["game", "in_game", "map_data", "named_locations"],
             ISavingService.Dummy,
             new("LocationsDefinition", "txt", "#"),
             new LocationFileLoading(),
             false),
      ];
   }
}