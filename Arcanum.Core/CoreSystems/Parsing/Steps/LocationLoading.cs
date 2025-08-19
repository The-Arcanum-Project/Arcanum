using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class LocationFileLoading : FileLoadingService
{
   private const string DEFAULT_LOCATION_FILE_NAME = "00_default.txt";

   public override List<Type> ParsedObjects => [typeof(Location)];
   public override string GetFileDataDebugInfo() => $"Loaded '{Globals.Locations.Count}'.";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      Dictionary<string, Location> locations =
         new(fileObj.Path.Filename.Equals(DEFAULT_LOCATION_FILE_NAME) ? 29_000 : 100);

      var context = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (!IO.IO.CreateStreamReader(fileObj.Path.FullPath, Encoding.UTF8, out var reader) || reader is null)
      {
         DiagnosticException ex = new(IOError.Instance.FileReadingError, fileObj.Path.FullPath);
         ex.HandleDiagnostic(context, GetActionName());
         return false;
      }

      var isFlawless = true;
      var fInformation = new FileInformation(fileObj.Path.Filename, true, descriptor);

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
               DiagnosticException.LogWarning(context,
                                              ParsingError.Instance.InvalidKeyValuePair,
                                              GetActionName(),
                                              line);
               isFlawless = false;
               continue;
            }

            var keySpan = span[..equalIndex].Trim();
            var endIndex = span.IndexOf('#');
            endIndex = endIndex == -1 ? span.Length - 1 : endIndex;
            var valueSpan = span[(equalIndex + 1)..endIndex].Trim();

            if (keySpan.IsEmpty || valueSpan.IsEmpty)
            {
               DiagnosticException.LogWarning(context,
                                              ParsingError.Instance.InvalidKeyValuePair,
                                              GetActionName(),
                                              line);
               isFlawless = false;
               continue;
            }

            if (!TryParseHexInt(valueSpan, out var colorInt))
            {
               context.ColumnNumber = equalIndex + 1 + GetLeadingSpacesCount(valueSpan);
               DiagnosticException.LogWarning(context,
                                              ParsingError.Instance.HexToIntConversionError,
                                              GetActionName(),
                                              valueSpan.ToString());
               isFlawless = false;
               continue;
            }

            var key = keySpan.ToString();
            context.ColumnNumber = line.IndexOf(key, StringComparison.Ordinal);

            var newLocation = new Location(fInformation, colorInt, key);
            if (!locations.TryAdd(key, newLocation))
            {
               DiagnosticException.LogWarning(context,
                                              ParsingError.Instance.DuplicateLocationDefinition,
                                              GetActionName(),
                                              key);
               isFlawless = false;
            }
         }

      // if it is null we are not in a multithreaded context
      if (lockObject is not null)
         lock (lockObject)
            foreach (var location in locations)
               Globals.Locations[location.Key] = location.Value;
      else
         foreach (var location in locations)
            Globals.Locations[location.Key] = location.Value;

      return isFlawless;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      List<Location> locationsToRemove = [];
      // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
      // No link here to prevent fileObj allocations
      foreach (var location in Globals.Locations.Values)
         if (location.FileInformation.FileName.Equals(fileObj.Path.Filename))
            locationsToRemove.Add(location);

      if (locationsToRemove.Count == 0)
         return true;

      foreach (var locToRemove in locationsToRemove)
         Globals.Locations.Remove(locToRemove.Name);
      
      Globals.Locations.TrimExcess();
      return true;
   }

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