using System.Text;

namespace Arcanum.Core.Utils;

public static class StringUtils
{
   // Converts CamelCase to snake_case
   public static string ToSnakeCase(this string input)
   {
      if (string.IsNullOrEmpty(input))
         return input;

      var stringBuilder = new StringBuilder();
      for (var i = 0; i < input.Length; i++)
      {
         var c = input[i];
         if (char.IsUpper(c))
         {
            // Add an underscore before uppercase letters (except the first one)
            if (i > 0)
               stringBuilder.Append('_');
            stringBuilder.Append(char.ToLower(c));
         }
         else
            stringBuilder.Append(c);
      }

      return stringBuilder.ToString();
   }
}