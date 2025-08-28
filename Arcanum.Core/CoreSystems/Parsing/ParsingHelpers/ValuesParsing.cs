using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

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
}