using System.Text;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
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

      var filedInfos = FileManager.GetAllFileInfosForDirectory(fileDescriptor);
      HashSet<Location> locations = new(estimate > 0 ? estimate : 28_000);

      foreach (var file in filedInfos)
      {
         var context = new LocationContext(0, 0, file.Descriptor.GetFilePath());

         if (!IO.CreateStreamReader(file.Descriptor.GetFilePath(), Encoding.UTF8, out var reader) || reader is null)
         {
            ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                                 context.GetInstance(),
                                                 DiagnosticSeverity.Error,
                                                 nameof(LoadLocations),
                                                 $"Failed to read file '{file.Descriptor.GetFilePath()}'.",
                                                 "Please check the file path."));
            continue;
         }

         using (reader)
            while (reader.ReadLine() is { } line)
            {
               if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
               {
                  context.LineNumber++;
                  continue;
               }

               var match = LocDefinitionRegex().Match(line);
               if (!match.Success)
               {
                  context.ColumnNumber = 0;
                  ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                                       context.GetInstance(),
                                                       DiagnosticSeverity.Warning,
                                                       nameof(LoadLocations),
                                                       $"Failed to parse location definition from line: '{line}'.",
                                                       "The line does not match the expected format of '<location_name> = <color_in_hex>'."));
               }

               if (!int.TryParse(match.Groups[2].Value,
                                 System.Globalization.NumberStyles.HexNumber,
                                 null,
                                 out var colorInt))
               {
                  context.ColumnNumber = match.Groups[2].Index;
                  ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                                       context.GetInstance(),
                                                       DiagnosticSeverity.Warning,
                                                       nameof(LoadLocations),
                                                       $"Failed to parse color value from line: '{line}'.",
                                                       "The color value must be a valid hexadecimal number."));
               }

               if (!locations.Add(new(colorInt, match.Groups[1].Value)))
               {
                  context.ColumnNumber = match.Groups[1].Index;
                  ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                                       context.GetInstance(),
                                                       DiagnosticSeverity.Warning,
                                                       nameof(LoadLocations),
                                                       $"Duplicate location name found: '{match.Groups[1].Value}'.",
                                                       "Location names must be unique."));
               }

               context.LineNumber++;
            }
      }

      return locations;
   }

   [GeneratedRegex(@"^(?:[^#\r\n]*)\b(\w+)\s*=\s*([\da-f]+)", RegexOptions.Compiled)]
   private static partial Regex LocDefinitionRegex();
}