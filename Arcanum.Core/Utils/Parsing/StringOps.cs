using System.IO;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.Utils.Parsing;

public static partial class StringOps
{
   public static HashSet<Location> LoadLocations(int estimate = -1)
   {
      
      var fileDescriptor = new FileDescriptor([],
                                              ["game", "in_game", "map_data", "named_locations", "00_default.txt"],
                                              ISavingService.Dummy, // TODO: @Minnator Add a proper saving service
                                              new("LocationsDefinition", ".txt", "#"));

      var fildInfos = FileManager.GetAllFileInfosForDirectory(fileDescriptor);
      
      //TODO: @Minnator finish loading locations from fileInfos
      
      
      var fileContext = new FileInformation();

      HashSet<Location> locations = [];
      // if (estimate > 0)
      //    locations = new(estimate);
      // else
      //    locations = [];
      //
      // var regex = KvpRegex();
      // using var reader = new StreamReader(path);
      //
      // while (reader.ReadLine() is { } line)
      // {
      //    var match = regex.Match(line);
      //    if (match.Success)
      //    {
      //       var key = match.Groups[1].Value;
      //       var value = match.Groups[2].Value;
      //       if (!locations.Add())
      //       {
      //       }
      //    }
      // }

      return locations;
   }

   [GeneratedRegex(@"^(?:[^#\r\n]*)\b(\w+)\s*=\s*([\da-f]+)", RegexOptions.Compiled)]
   private static partial Regex KvpRegex();
}