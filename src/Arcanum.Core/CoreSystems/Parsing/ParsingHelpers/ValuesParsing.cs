using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

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

   public static bool ParseLocation(string key, ref ParsingContext pc, out Location location)
   {
      using var scope = pc.PushScope();
      if (!Globals.Locations.TryGetValue(key, out location!))
      {
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidLocationKey,
                                        key);
         location = Location.Empty;
         return false;
      }

      return true;
   }
}