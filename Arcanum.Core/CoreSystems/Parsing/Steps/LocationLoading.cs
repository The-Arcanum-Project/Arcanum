using System.Reflection;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingStep;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class LocationFileLoading : FileLoadingService
{
   private const string DEFAULT_LOCATION_FILE_NAME = "00_default.txt";

   public override string GetFileDataDebugInfo() => $"Loaded '{Globals.Locations.Count}'.";

   public override bool LoadSingleFile(FileObj fileObj, object? lockObject = null)
   {
      var method = MethodBase.GetCurrentMethod();
      if (method?.DeclaringType is null)
         throw new InvalidOperationException("Could not retrieve the current method information.");

      var actionName =
         $"{method.DeclaringType.FullName}.{method.Name}({string.Join(' ', method.GetParameters().Select(p => p.ParameterType.FullName))}";

      HashSet<Location> locations = new(fileObj.Path.Filename.Equals(DEFAULT_LOCATION_FILE_NAME) ? 29_000 : 100);

      var context = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (!IO.IO.CreateStreamReader(fileObj.Path.FullPath, Encoding.UTF8, out var reader) || reader is null)
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
            context.LineNumber++;

            var span = line.AsSpan().TrimStart();
            if (span.Length == 0 || span[0] == '#')
               continue;

            var equalIndex = span.IndexOf('=');
            if (equalIndex <= 0 || equalIndex == span.Length - 1)
            {
               LogWarning(context,
                          actionName,
                          $"Failed to parse location definition from line: '{line}'.",
                          "Expected format '<location_name> = <hex_color>'.");
               isFlawless = false;
               continue;
            }

            var keySpan = span[..equalIndex].Trim();
            var endIndex = span.IndexOf('#');
            endIndex = endIndex == -1 ? span.Length - 1 : endIndex;
            var valueSpan = span[(equalIndex + 1)..endIndex].Trim();

            if (keySpan.IsEmpty || valueSpan.IsEmpty)
            {
               LogWarning(context,
                          actionName,
                          $"Failed to parse location definition from line: '{line}'.",
                          "Location name or color value missing.");
               isFlawless = false;
               continue;
            }

            if (!TryParseHexInt(valueSpan, out var colorInt))
            {
               context.ColumnNumber = equalIndex + 1 + GetLeadingSpacesCount(valueSpan);
               LogWarning(context,
                          actionName,
                          $"Failed to parse color value from line: '{line}'.",
                          "The color value must be a valid hexadecimal number.");
               isFlawless = false;
               continue;
            }

            var key = keySpan.ToString();
            context.ColumnNumber = line.IndexOf(key, StringComparison.Ordinal);

            var newLocation = new Location(colorInt, key);
            if (Globals.Locations.Contains(newLocation))
            {
               LogWarning(context,
                          actionName,
                          $"Duplicate location name found: '{key}'.",
                          "Location names must be unique.");
               isFlawless = false;
               continue;
            }

            locations.Add(newLocation);
         }

      // if it is null we are not in a multithreaded context
      if (lockObject is not null)
         lock (lockObject)
            Globals.Locations.UnionWith(locations);
      else
         Globals.Locations.UnionWith(locations);

      return isFlawless;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj) => throw new NotImplementedException();

   private static void LogWarning(LocationContext ctx, string action, string message, string hint)
      => ErrorManager.AddToLog(new Diagnostic(IOError.Instance.FileReadingError,
                                              ctx.GetInstance(),
                                              DiagnosticSeverity.Warning,
                                              action,
                                              message,
                                              hint));

   private static bool TryParseHexInt(ReadOnlySpan<char> span, out int value)
   {
      // TryParse with NumberStyles.HexNumber is not available on Span until .NET 7+
      // Implement manual parsing here or fallback to string
      value = 0;
      if (span.Length == 0)
         return false;

      var result = 0;
      foreach (var c in span)
      {
         var digit = c switch
         {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => 10 + c - 'a',
            >= 'A' and <= 'F' => 10 + c - 'A',
            _ => -1,
         };

         if (digit == -1)
            return false;

         unchecked
         {
            result = (result << 4) + digit;
         }
      }

      value = result;
      return true;
   }

   private static int GetLeadingSpacesCount(ReadOnlySpan<char> span)
   {
      var count = 0;
      foreach (var c in span)
      {
         if (!char.IsWhiteSpace(c))
            break;

         count++;
      }

      return count;
   }
}