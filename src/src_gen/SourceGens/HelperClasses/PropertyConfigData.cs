using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ParserGenerator.HelperClasses;

public enum AggregateLinkType : byte
{
   None,
   Child,
   Parent,
   ReverseParent,
   ReverseChild,
}

public class PropertyConfigData
{
   public PropertyConfigData()
   {
   }

   public PropertyConfigData(AttributeData attributeData, IPropertySymbol propSymbol)
   {
      var constructorParams = attributeData.AttributeConstructor?.Parameters ??
                              ImmutableArray<IParameterSymbol>.Empty;
      var minValueIndex = constructorParams.IndexOf(constructorParams.FirstOrDefault(p => p.Name == "minValue")!);
      var maxValueIndex = constructorParams.IndexOf(constructorParams.FirstOrDefault(p => p.Name == "maxValue")!);

      // Get the value from ConstructorArguments using the found index.
      var minValueArg = (minValueIndex != -1 && minValueIndex < attributeData.ConstructorArguments.Length)
                           ? attributeData.ConstructorArguments[minValueIndex]
                           : default;

      var maxValueArg = (maxValueIndex != -1 && maxValueIndex < attributeData.ConstructorArguments.Length)
                           ? attributeData.ConstructorArguments[maxValueIndex]
                           : default;

      PropertyType = propSymbol.Type;
      IsReadonly = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 0, "isReadonly");
      IsInlined = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 1, "isInlined");
      AllowEmpty = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 2, "allowEmpty");
      DisableMapInferButtons = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 3, "disableMapInferButtons");
      IsRequired = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 4, "isRequired");
      DefaultValueMethod = AttributeHelper.SimpleGetAttrArgValue<string>(attributeData, 7, "defaultValueMethod") ?? "";
      IgnoreCommand = AttributeHelper.SimpleGetAttrArgValue<bool>(attributeData, 8, "ignoreCommand");
      AggregateLinkType = AttributeHelper.SimpleGetAttrArgValue<AggregateLinkType>(attributeData, 9, "aggreateLinkType");
      AggregateLinkParent = AttributeHelper.SimpleGetAttrArgValue<string?>(attributeData, 10, "aggregateLinktParent");

      // Use the constructor arguments we just found.
      MinValue = PropertyConfigHelper.FormatTypedConstant(minValueArg);
      MaxValue = PropertyConfigHelper.FormatTypedConstant(maxValueArg);
      PropertyName = propSymbol.Name;
   }

   public ITypeSymbol PropertyType { get; set; } = null!;
   public string PropertyName { get; set; } = null!;

   // Attribute data
   public bool IsReadonly { get; set; }
   public bool IsInlined { get; set; }
   public bool AllowEmpty { get; set; }
   public bool DisableMapInferButtons { get; set; }
   public bool IsRequired { get; set; }
   public string MinValue { get; set; } = null!;
   public string MaxValue { get; set; } = null!;
   public string DefaultValueMethod { get; set; } = null!;
   public bool IgnoreCommand { get; set; }
   public AggregateLinkType AggregateLinkType { get; set; } = AggregateLinkType.None;
   public string? AggregateLinkParent { get; set; }
}