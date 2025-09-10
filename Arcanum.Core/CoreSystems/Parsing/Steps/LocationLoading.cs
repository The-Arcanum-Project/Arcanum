using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
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

      var fInformation = new FileInformation(fileObj.Path.Filename, true, descriptor);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      var isFlawless = true;

      var sw = System.Diagnostics.Stopwatch.StartNew();
      var rns = Parser.Parse(fileObj, out var source);
      sw.Stop();
      Debug.WriteLine($"[Parsing] Parsed '{fileObj.Path.Filename}' in {sw.ElapsedMilliseconds} ms.");

      rns.IsNodeEmptyDiagnostic(ctx, ref isFlawless);

      long nodeTicks = 0;
      long identifierTicks = 0;
      long hexParseTicks = 0;
      long addTicks = 0;

      sw.Restart(); // Start timing the whole loop
      var sw2 = new Stopwatch();

      foreach (var statement in rns.Statements)
      {
         sw2.Restart();
         if (!statement.IsContentNode(ctx, nameof(LocationFileLoading), source, ref isFlawless, out var cn))
         {
            nodeTicks += sw2.ElapsedTicks;
            continue;
         }

         nodeTicks += sw2.ElapsedTicks;

         sw2.Restart();
         if (!cn.GetBothIdentifiers(ctx,
                                    nameof(LocationFileLoading),
                                    source,
                                    ref isFlawless,
                                    out var left,
                                    out var right))
         {
            identifierTicks += sw2.ElapsedTicks;
            continue;
         }

         identifierTicks += sw2.ElapsedTicks;

         sw2.Restart();
         if (!TryParseHexInt(right, out var color))
         {
            ctx.SetPosition(cn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.HexToIntConversionError,
                                           GetActionName(),
                                           right);
            isFlawless = false;
            hexParseTicks += sw2.ElapsedTicks;
            continue;
         }

         hexParseTicks += sw2.ElapsedTicks;

         sw2.Restart();
         var location = new Location(fInformation, color, left);
         if (!Globals.Locations.TryAdd(left, location))
         {
            ctx.SetPosition(cn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateLocationDefinition,
                                           GetActionName(),
                                           left);
            isFlawless = false;
         }

         addTicks += sw2.ElapsedTicks;
      }

      sw.Stop(); // Stop main timer

      // --- Convert total Ticks to Milliseconds for display ---
      // (ticks * 1000) / Stopwatch.Frequency = milliseconds
      var nodeTime = (double)nodeTicks * 1000 / Stopwatch.Frequency;
      var identifierTime = (double)identifierTicks * 1000 / Stopwatch.Frequency;
      var hexParseTime = (double)hexParseTicks * 1000 / Stopwatch.Frequency;
      var addTime = (double)addTicks * 1000 / Stopwatch.Frequency;

      Debug.WriteLine($"[Processing] Processed '{fileObj.Path.Filename}' in {sw.ElapsedMilliseconds} ms.");
      Debug.WriteLine($" - Nodes: {nodeTime:F2} ms.");
      Debug.WriteLine($" - Identifiers: {identifierTime:F2} ms.");
      Debug.WriteLine($" - HexParse: {hexParseTime:F2} ms.");
      Debug.WriteLine($" - Add: {addTime:F2} ms.");

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