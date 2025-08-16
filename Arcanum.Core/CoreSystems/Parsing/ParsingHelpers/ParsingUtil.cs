using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class ParsingUtil
{
   public static bool TryParseBool(string value, out bool result)
   {
      if (string.IsNullOrWhiteSpace(value))
      {
         result = false;
         return false;
      }

      value = value.Trim().ToLowerInvariant();
      if (string.Equals(value, "yes", StringComparison.Ordinal))
      {
         result = true;
         return true;
      }

      if (string.Equals(value, "no", StringComparison.Ordinal))
      {
         result = false;
         return true;
      }

      result = false;
      return false;
   }

   public static List<Location> ParseLocationList(Content content, LocationContext ctx)
   {
      List<Location> locations = [];

      foreach (var (str, lineNum) in content.GetStringListEnumerator())
         if (Globals.Locations.TryGetValue(str, out var location))
            locations.Add(location);
         else
         {
            ctx.LineNumber = lineNum;
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidLocationKey,
                                           nameof(ParseLocationList),
                                           str);
         }

      return locations;
   }
}