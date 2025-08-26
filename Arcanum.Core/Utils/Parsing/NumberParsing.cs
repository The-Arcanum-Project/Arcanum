using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.Utils.Parsing;

public static class NumberParsing
{
   public static bool TryParseInt(string? input,
                                  LocationContext context,
                                  out int result,
                                  int minValue = -2_147_483_648,
                                  int maxValue = 2_147_483_647,
                                  int fallback = 0)
   {
      if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out result))
      {
         result = fallback;
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.InvalidIntMarkup,
                                        nameof(TryParseInt).GetType().FullName!,
                                        input ?? "null");
         return false;
      }

      if (result < minValue || result > maxValue)
      {
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.IntOutOfRange,
                                        nameof(TryParseInt).GetType().FullName!,
                                        input,
                                        minValue,
                                        maxValue);
         result = fallback;
         return false;
      }

      return true;
   }

   public static bool TryParseFloat(string? input,
                                    LocationContext context,
                                    out float result,
                                    float minValue = float.MinValue,
                                    float maxValue = float.MaxValue,
                                    float fallback = 0f,
                                    int precision = 2)
   {
      if (string.IsNullOrWhiteSpace(input) || !float.TryParse(input, out result))
      {
         result = fallback;
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.InvalidFloatMarkup,
                                        nameof(TryParseFloat).GetType().FullName!,
                                        input ?? "null");
         return false;
      }

      if (result < minValue || result > maxValue)
      {
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.FloatOutOfRange,
                                        nameof(TryParseFloat).GetType().FullName!,
                                        input,
                                        minValue,
                                        maxValue);
         result = fallback;
         return false;
      }

      result = (float)Math.Round(result, precision);
      return true;
   }

   public static bool TryParseBool(string? input,
                                   LocationContext context,
                                   out bool result,
                                   bool fallback = false)
   {
      if (string.IsNullOrWhiteSpace(input))
      {
         result = fallback;
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.InvalidBoolMarkup,
                                        nameof(TryParseBool).GetType().FullName!,
                                        input ?? "null");
         return false;
      }

      if (input.Equals("yes"))
         result = true;
      else if (input.Equals("no"))
         result = false;
      else
      {
         result = fallback;
         DiagnosticException.LogWarning(context,
                                        ParsingError.Instance.InvalidBoolMarkup,
                                        nameof(TryParseBool).GetType().FullName!,
                                        input);
         return false;
      }

      return true;
   }
}