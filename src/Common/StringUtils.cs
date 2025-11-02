namespace Common;

public static class StringUtils
{
   /// <summary>
   /// Trims quotes from the start and end of a string if they are present.
   /// </summary>
   /// <param name="str"></param>
   /// <returns></returns>
   public static string TrimQuotes(this string? str)
   {
      if (string.IsNullOrEmpty(str) || str.Length < 2)
         return str ?? string.Empty;

      var span = str.AsSpan();
      return span[0] == '"' && span[^1] == '"' ? span.Slice(1, span.Length - 2).ToString() : str;
   }
}