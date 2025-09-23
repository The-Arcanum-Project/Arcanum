using Microsoft.CodeAnalysis;
using ParserGenerator.HelperClasses;

namespace ParserGenerator.SubClasses;

public class ParserClassMetadata
{
   public bool AllowUnknownNodes { get; set; }
   public Type TargetType { get; }
   /// <summary>
   /// if a block node is encountered with one of these keys and AllowUnknownNodes is false, it will be ignored instead of throwing an exception.
   /// </summary>
   public string[] IgnoredBlockKeys { get; set; }

   /// <summary>
   /// if a content node is encountered with one of these keys and AllowUnknownNodes is false, it will be ignored instead of throwing an exception.
   /// </summary>
   public string[] IgnoredContentKeys { get; set; }

   public bool ContainsOnlyChildObjects { get; set; } = false;
   public string? ChildObjectList { get; set; } = null;

   public ParserClassMetadata(AttributeData attr)
   {
      // Positional argument at index 0
      TargetType = AttributeHelper.GetAttributeArgumentValue<INamedTypeSymbol>(attr, position: 0)?.GetType() ??
                   throw new ArgumentException("TargetType cannot be null");

      // Positional argument at index 1
      AllowUnknownNodes = AttributeHelper.GetAttributeArgumentValue(attr, position: 1, defaultValue: false);

      // Positional argument at index 2 (an array)
      IgnoredBlockKeys = AttributeHelper.GetAttributeArgumentValue<string[]>(attr, position: 2) ?? [];

      // Positional argument at index 3 (an array)
      IgnoredContentKeys = AttributeHelper.GetAttributeArgumentValue<string[]>(attr, position: 3) ?? [];

      // Positional argument at index 4
      ContainsOnlyChildObjects = AttributeHelper.GetAttributeArgumentValue(attr, position: 4, defaultValue: false);

      // Positional argument at index 5
      ChildObjectList = AttributeHelper.GetAttributeArgumentValue<string?>(attr, position: 5);
   }
}