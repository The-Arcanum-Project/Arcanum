using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.CoreSystems.SavingSystem.Services;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.Parsing.ParsingStep;

namespace Arcanum.Core.Utils.Parsing.Steps;

public class LocationLoading() : ParsingStepBase(new([],
                                                     ["game", "in_game", "map_data", "named_locations"],
                                                     ISavingService.Dummy,
                                                     new("LocationsDefinition", "txt", "#"),
                                                     new LocationFileLoading()),
                                                 false)
{
   // TODO: @Minnator Add a proper saving service
}

public partial class LocationFileLoading : SingleFileLoadingBase
{
   private const string DEFAULT_LOCATION_FILE_NAME = "00_default.txt";

   [GeneratedRegex(@"^(?:[^#\r\n]*)\b(\w+)\s*=\s*([\da-f]+)", RegexOptions.Compiled)]
   private static partial Regex LocDefinitionRegex();

   public override bool LoadSingleFile(FileObj fileObj, object? lockObject = null)
   {
      var method = MethodBase.GetCurrentMethod();
      if (method?.DeclaringType is null)
         throw new InvalidOperationException("Could not retrieve the current method information.");

      var actionName =
         $"{method.DeclaringType.FullName}.{method.Name}({string.Join(' ', method.GetParameters().Select(p => p.ParameterType.FullName))}";

      HashSet<Location> locations = new(fileObj.Path.Filename.Equals(DEFAULT_LOCATION_FILE_NAME) ? 29_000 : 100);

      var context = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (!IO.CreateStreamReader(fileObj.Path.FullPath, Encoding.UTF8, out var reader) || reader is null)
      {
         ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                              context.GetInstance(),
                                              DiagnosticSeverity.Error,
                                              actionName,
                                              $"Failed to read file '{fileObj.Path.FullPath}'.",
                                              "Please check the file path."));
         return false;
      }

      var isFlawless = true;

      using (reader)
         while (reader.ReadLine() is { } line)
         {
            // Remove leading whitespace as it can affect the comment checking
            line = line.TrimStart();
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
                                                    actionName,
                                                    $"Failed to parse location definition from line: '{line}'.",
                                                    "The line does not match the expected format of '<location_name> = <color_in_hex>'."));
               isFlawless = false;
               continue;
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
                                                    actionName,
                                                    $"Failed to parse color value from line: '{line}'.",
                                                    "The color value must be a valid hexadecimal number."));
               isFlawless = false;
               continue;
            }

            context.ColumnNumber = match.Groups[1].Index;
            var newLocation = new Location(colorInt, match.Groups[1].Value);

            if (Globals.Locations.Contains(newLocation))
            {
               ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                                    context.GetInstance(),
                                                    DiagnosticSeverity.Warning,
                                                    actionName,
                                                    $"Duplicate location name found: '{match.Groups[1].Value}'.",
                                                    "Location names must be unique."));
               isFlawless = false;
               continue;
            }

            locations.Add(newLocation);

            context.LineNumber++;
         }

      // if it is null we are not in a multithreaded context
      if (lockObject is not null)
         lock (lockObject)
            Globals.Locations.UnionWith(locations);
      else
         Globals.Locations.UnionWith(locations);

      return isFlawless;
   }
}