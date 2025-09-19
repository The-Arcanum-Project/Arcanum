namespace ParserGenerator.HelperClasses;

using Microsoft.CodeAnalysis;
using System.Linq;

public static class AttributeHelper
{
   /// <summary>
   /// Safely retrieves an argument's value from an AttributeData instance.
   /// It checks for named arguments first, then positional arguments, and finally falls back to the parameter's default value.
   /// </summary>
   /// <param name="attribute">The Roslyn AttributeData to inspect.</param>
   /// <param name="position">The zero-based positional index of the argument in the attribute's constructor.</param>
   /// <param name="name">The name of the argument (corresponds to the constructor parameter name).</param>
   /// <returns>The argument's value as an object, or null if not found and no default exists.</returns>
   public static object? GetAttributeArgumentValue(AttributeData attribute, int position, string name)
   {
      // Priority 1: Check for an explicitly set Named Argument.
      // This is the most specific way a user can provide a value.
      var namedArg = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == name);
      if (namedArg.Key is not null)
      {
         return namedArg.Value.Value;
      }

      // Priority 2: Check for a Positional Argument at the given index.
      if (position < attribute.ConstructorArguments.Length)
      {
         return attribute.ConstructorArguments[position].Value;
      }

      // Priority 3: Fallback to the default value from the constructor's signature.
      var constructor = attribute.AttributeConstructor;
      if (constructor is not null && position < constructor.Parameters.Length)
      {
         var parameter = constructor.Parameters[position];
         if (parameter.Name == name && parameter.HasExplicitDefaultValue)
         {
            return parameter.ExplicitDefaultValue;
         }
      }

      // No value was provided and no default exists.
      return null;
   }

   /// <summary>
   /// Safely retrieves and casts an argument's value from an AttributeData instance.
   /// </summary>
   /// <typeparam name="T">The expected type of the argument.</typeparam>
   /// <param name="defaultValue">The value to return if the argument is not found.</param>
   /// <returns>The argument's value cast to T, or the provided default value.</returns>
   public static T? GetAttributeArgumentValue<T>(AttributeData attribute,
                                                 int position,
                                                 string name,
                                                 T? defaultValue = default)
   {
      object? rawValue = GetAttributeArgumentValue(attribute, position, name);

      if (rawValue is null)
      {
         return defaultValue;
      }

      // This cast works for primitive types and for enums (where rawValue is the underlying integer).
      return (T)rawValue;
   }
}