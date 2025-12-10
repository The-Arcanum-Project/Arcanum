namespace ParserGenerator.NexusGeneration;

public static class Formatter
{
   public static void FormatStringArray(IndentBuilder builder, string[] data, int maxPerLine = 5)
   {
      using (builder.Indent())
         for (var i = 0; i < data.Length; i++)
         {
            builder.Append($"{data[i]}, ");

            if ((i + 1) % maxPerLine == 0 && i < data.Length - 1)
               builder.AppendLine();
         }
   }

   public static string EscapeStringForCode(string? code)
   {
      if (string.IsNullOrEmpty(code))
         return "";

      return code!
            .Replace("\\", "\\\\") // Must be done first to avoid double-escaping later chars
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r") // Handle Carriage Return
            .Replace("\n", "\\n") // Handle New Line
            .Replace("\t", "\\t"); // Optional: Handle Tabs
   }
}