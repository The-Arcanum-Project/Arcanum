using System.Text;
using Microsoft.CodeAnalysis;

namespace ParserGenerator.HelperClasses;

public static class GenHelper
{
   public static AttributeData? GetAttributeForKey(this IFieldSymbol fieldSymbol, string attributeName)
   {
      return fieldSymbol.GetAttributes()
                        .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                              attributeName);
   }

   public static AttributeData? GetAttributeForKey(this INamedTypeSymbol value, string key)
   {
      return value.GetAttributes()
                  .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == key);
   }

   public static AttributeData? GetAttributeForKey(this IPropertySymbol propertySymbol, string attributeName)
   {
      return propertySymbol.GetAttributes()
                           .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                 attributeName);
   }

   public static string ToSnakeCase(this string input)
   {
      if (string.IsNullOrEmpty(input))
         return input;

      var sb = new StringBuilder();
      for (var i = 0; i < input.Length; i++)
      {
         var c = input[i];
         if (char.IsUpper(c))
         {
            if (i > 0)
               sb.Append('_');
            sb.Append(char.ToLower(c));
         }
         else
         {
            sb.Append(c);
         }
      }

      return sb.ToString();
   }
}