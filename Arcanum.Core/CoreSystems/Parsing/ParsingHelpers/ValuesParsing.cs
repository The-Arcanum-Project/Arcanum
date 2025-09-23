using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class ValuesParsing
{
   public static bool ParseBool(string value,
                                LocationContext ctx,
                                string actionName,
                                out bool result,
                                bool defaultValue = false)
   {
      if (string.IsNullOrWhiteSpace(value))
      {
         result = defaultValue;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.BoolParsingError,
                                        actionName,
                                        value);
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

      result = defaultValue;
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.BoolParsingError,
                                     actionName,
                                     value);
      return false;
   }

   public static bool ParseLocation(string key, LocationContext ctx, string actionName, out Location location)
   {
      if (!Globals.Locations.TryGetValue(key, out location!))
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        key);
         location = Location.Empty;
         return false;
      }

      return true;
   }
}