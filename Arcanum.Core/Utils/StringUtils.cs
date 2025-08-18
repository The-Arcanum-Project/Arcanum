namespace Arcanum.Core.Utils;

public static class StringUtils
{
   public static string TrimQuotes(this string? str)
   {
      if (string.IsNullOrEmpty(str) || str.Length < 2)
         return str ?? string.Empty;

      var span = str.AsSpan();
      return span[0] == '"' && span[^1] == '"' ? span.Slice(1, span.Length - 2).ToString() : str;
   }

}