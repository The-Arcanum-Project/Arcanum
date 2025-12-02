using System.Globalization;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

public static class NumberParsing
{
   public static bool TryParseInt(string? input,
                                  ref ParsingContext pc,
                                  out int result,
                                  int minValue = -2_147_483_648,
                                  int maxValue = 2_147_483_647,
                                  int fallback = 0)
   {
      using var context = pc.PushScope();
      if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out result))
      {
         result = fallback;
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidIntMarkup,
                                        input ?? "null");
         return false;
      }

      if (result < minValue || result > maxValue)
      {
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.IntOutOfRange,
                                        input,
                                        minValue,
                                        maxValue);
         result = fallback;
         return false;
      }

      return true;
   }

   public static bool TryParseFloat(string? input,
                                    ref ParsingContext pc,
                                    out float result,
                                    float minValue = float.MinValue,
                                    float maxValue = float.MaxValue,
                                    float fallback = 0f,
                                    int precision = 2)
   {
      using var scope = pc.PushScope();
      if (string.IsNullOrWhiteSpace(input) ||
          !float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
      {
         result = fallback;
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidFloatMarkup,
                                        input ?? "null");
         return false;
      }

      if (result < minValue || result > maxValue)
      {
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.FloatOutOfRange,
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
                                   ref ParsingContext pc,
                                   out bool result,
                                   bool fallback = false)
   {
      using var scope = pc.PushScope();
      if (string.IsNullOrWhiteSpace(input))
      {
         result = fallback;
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBoolMarkup,
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
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBoolMarkup,
                                        input);
         return false;
      }

      return true;
   }
}