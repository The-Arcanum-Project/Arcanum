using System.Collections.Immutable;

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
   /// <param name="name"></param>
   /// <param name="defaultValue">The value to return if the argument is not found.</param>
   /// <param name="attribute"></param>
   /// <param name="position"></param>
   /// <returns>The argument's value cast to T, or the provided default value.</returns>
   public static T? SimpleGetAttributeArgumentValue<T>(AttributeData attribute,
                                                       int position,
                                                       string name,
                                                       T? defaultValue = default)
   {
      object? rawValue = GetAttributeArgumentValue(attribute, position, name);

      if (rawValue is null)
         return defaultValue;

      return (T)rawValue;
   }

   /// <summary>
   /// Safely retrieves and casts an argument's value from an AttributeData instance.
   /// This method correctly handles both single values and array values.
   /// </summary>
   /// <typeparam name="T">The expected type of the argument (e.g., bool, string, or string[]).</typeparam>
   /// <param name="attribute">The AttributeData to inspect.</param>
   /// <param name="position">The zero-based index for a positional constructor argument.</param>
   /// <param name="name">The name for a named argument (property). Ignored if position is used.</param>
   /// <param name="defaultValue">The value to return if the argument is not found or is null.</param>
   /// <returns>The argument's value cast to T, or the provided default value.</returns>
   public static T? GetAttributeArgumentValue<T>(
      AttributeData attribute,
      int position = -1, // Use -1 as a sentinel for "not a positional argument"
      string? name = null,
      T? defaultValue = default)
   {
      // Find the correct TypedConstant from either constructor or named arguments
      TypedConstant argumentConstant;
      if (position >= 0)
      {
         // Positional argument
         if (position >= attribute.ConstructorArguments.Length)
            return defaultValue;

         argumentConstant = attribute.ConstructorArguments[position];
      }
      else if (!string.IsNullOrEmpty(name))
      {
         // Named argument
         argumentConstant = attribute.NamedArguments
                                     .FirstOrDefault(arg => arg.Key == name)
                                     .Value;

         // If the named argument wasn't found, the Value will be null/default.
         if (argumentConstant.IsNull)
            return defaultValue;
      }
      else
      {
         // Invalid usage of the helper
         return defaultValue;
      }

      // --- THE CORE FIX FOR ARRAYS ---

      // Check if the requested type 'T' is an array type.
      if (typeof(T).IsArray)
      {
         // If the argument is not an array kind, something is wrong.
         if (argumentConstant.Kind != TypedConstantKind.Array)
         {
            return defaultValue;
         }

         // The value is in the 'Values' property.
         var arrayValues = argumentConstant.Values;
         if (arrayValues.IsDefault)
            return defaultValue; // Handle empty/null array case

         // Get the element type of the requested array (e.g., 'string' from 'string[]')
         var elementType = typeof(T).GetElementType();
         if (elementType == null)
            return defaultValue;

         // Create a new array of the correct element type
         var resultArray = Array.CreateInstance(elementType, arrayValues.Length);

         // Populate the new array by converting each TypedConstant
         for (var i = 0; i < arrayValues.Length; i++)
         {
            resultArray.SetValue(arrayValues[i].Value, i);
         }

         // Return the populated array, cast to the final type T
         return (T)(object)resultArray;
      }
      else
      {
         // --- This is the logic for non-array types ---
         if (argumentConstant.Value is T typedValue)
         {
            return typedValue;
         }

         return defaultValue;
      }
   }
}