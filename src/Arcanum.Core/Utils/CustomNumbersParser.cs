using System.Globalization;
using System.Text.RegularExpressions;

namespace Arcanum.Core.Utils;

public static class CustomNumberParser
{
   // Regex breakdown:
   // \{                : Match literal '{'
   // (?<var>\w+)       : Group "var": The variable name (x, y, etc)
   // (?:#\.(?<hash>#+))? : Group "hash": Optional part matching "#.###" logic
   // (?::(?<spec>\w+))?  : Group "spec": Optional part matching ":F" logic
   // \}                : Match literal '}'
   private static readonly Regex FormatRegex = new(@"\{(?<var>\w+)(?:#\.(?<hash>#+))?(?::(?<spec>\w+))?\}", RegexOptions.Compiled);

   public static string Format(string template, Dictionary<string, double> values)
   {
      return FormatRegex.Replace(template,
                                 match =>
                                 {
                                    var varName = match.Groups["var"].Value;

                                    if (!values.TryGetValue(varName, out var val))
                                       return match.Value;

                                    var hashes = match.Groups["hash"].Value;
                                    var specifier = match.Groups["spec"].Value;

                                    // Handle {x#.###} logic (Hash counting)
                                    if (!string.IsNullOrEmpty(hashes))
                                    {
                                       var precision = hashes.Length;
                                       return val.ToString("F" + precision, CultureInfo.InvariantCulture);
                                    }

                                    // Handle {y:F} logic (Standard .NET specifiers)
                                    if (!string.IsNullOrEmpty(specifier))
                                       // Note: Standard "F" in .NET defaults to NumberFormatInfo.NumberDecimalDigits (usually 2)
                                       return val.ToString(specifier, CultureInfo.InvariantCulture);

                                    // Default {x} logic (F2 as per your requirement 123.46)
                                    return val.ToString("F2", CultureInfo.InvariantCulture);
                                 });
   }
}