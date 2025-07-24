using System.Diagnostics;
using System.IO;
using System.Text;
using Arcanum.Core.Utils.Parsing;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public static class NamedLocationLoading
{
   
   // FileDescriptor descriptor
   public static void LoadNamedLocations()
   {
      var path = "S:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\named_locations\\00_default.txt";
      var locationSpan = IO.IO.ReadAllTextAsSpan(path, Encoding.UTF8);

      var locations = StringOps.SplitLocationColors(path, 43_000); //Estimated vanilla location count
      
      var sb = new StringBuilder();
      foreach (var location in locations)
         sb.AppendLine($"Name: {location.Item1} with: {location.Item2}");
      
      IO.IO.WriteAllText(Path.Combine(IO.IO.GetArcanumDataPath, "Debug/NamedLocations.txt"), sb.ToString(), Encoding.UTF8);

      Debug.WriteLine($"Found {locations.Count} named locations in {path}");
   }
   
}