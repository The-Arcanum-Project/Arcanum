using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.Utils.Parsing;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public static class NamedLocationLoading
{
   // FileDescriptor descriptor
   public static void LoadNamedLocations()
   {
      var path = FileManager.GetDependentPath("game", "in_game", "map_data", "named_locations", "00_default.txt");

      var locations = StringOps.LoadLocations(29_000);
      
      
   }
}