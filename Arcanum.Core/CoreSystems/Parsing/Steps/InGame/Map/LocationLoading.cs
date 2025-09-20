using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class LocationFileLoading : FileLoadingService
{
   public override List<Type> ParsedObjects => [typeof(Location)];
   public override string GetFileDataDebugInfo() => $"Loaded '{Globals.Locations.Count}'.";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var fInformation = new FileInformation(fileObj.Path.Filename, true, descriptor);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      var isFlawless = true;

      var rns = Parser.Parse(fileObj, out var source);
      rns.IsNodeEmptyDiagnostic(ctx, ref isFlawless);

      foreach (var statement in rns.Statements)
      {
         if (!statement.IsContentNode(ctx, nameof(LocationFileLoading), source, ref isFlawless, out var cn))
            continue;

         if (!cn.GetBothIdentifiers(ctx,
                                    nameof(LocationFileLoading),
                                    source,
                                    ref isFlawless,
                                    out var left,
                                    out var right))
            continue;

         if (!TryParseHexInt(right, out var color))
         {
            ctx.SetPosition(cn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.HexToIntConversionError,
                                           GetActionName(),
                                           right);
            isFlawless = false;
            continue;
         }

         if (!Globals.Locations.TryAdd(left, new(fInformation, color, left)))
         {
            ctx.SetPosition(cn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateLocationDefinition,
                                           GetActionName(),
                                           left);
            isFlawless = false;
         }
      }

      return isFlawless;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
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
}