using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ParserGenerator.HelperClasses;

/// <summary>
/// Generates a PropertyConfigData class instance from a PropertyDeclarationSyntax.
/// </summary>
public static class PropertyConfigHelper
{
   private const string PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME =
      "Arcanum.Core.CoreSystems.NUI.Attributes.PropertyConfigAttribute";

   private const string JOMINI_COLOR_TYPE = "Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor.JominiColor";

   public static PropertyConfigData GeneratePropertyConfigData(IPropertySymbol propSymbol,
                                                               INamedTypeSymbol classSymbol,
                                                               SourceProductionContext context)
   {
      var pcad =
         Helpers.GetEffectiveAttribute(classSymbol, propSymbol, PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME) ??
         // Check on the propDirectly
         propSymbol.GetAttributes()
                   .FirstOrDefault(attr =>
                                      attr.AttributeClass?.ToDisplayString() ==
                                      PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME);

      PropertyConfigData pcd;

      if (pcad == null)
         // context.ReportDiagnostic(Diagnostic.Create(new("PG0010",
         //                                                "Missing PropertyConfigAttribute",
         //                                                $"Property '{propSymbol.Name}' is missing the PropertyConfigAttribute.",
         //                                                "SourceGenerator",
         //                                                DiagnosticSeverity.Warning,
         //                                                isEnabledByDefault: true),
         //                                            propSymbol.Locations.FirstOrDefault()));
         pcd = new()
         {
            PropertyType = propSymbol.Type,
            IsReadonly = false,
            IsInlined = false,
            AllowEmpty = false,
            DisableMapInferButtons = false,
            IsRequired = false,
            MinValue = GenerateMinValueForType(propSymbol.Type),
            MaxValue = GenerateMaxValueForType(propSymbol.Type),
            PropertyName = propSymbol.Name,
         };
      else
      {
         pcd = new(pcad, propSymbol);
      }

      return pcd;
   }

   public static string GenerateMinValueForType(ITypeSymbol propertyType)
   {
      if (propertyType.TypeKind is TypeKind.Class or TypeKind.Interface)
         // For reference types, min value is null
         return "null";

      // for number types we want to default to min value
      return propertyType.SpecialType switch
      {
         SpecialType.System_SByte => "sbyte.MinValue",
         SpecialType.System_Byte => "byte.MinValue",
         SpecialType.System_Int16 => "short.MinValue",
         SpecialType.System_UInt16 => "ushort.MinValue",
         SpecialType.System_Int32 => "int.MinValue",
         SpecialType.System_UInt32 => "uint.MinValue",
         SpecialType.System_Int64 => "long.MinValue",
         SpecialType.System_UInt64 => "ulong.MinValue",
         SpecialType.System_Single => "float.MinValue",
         SpecialType.System_Double => "double.MinValue",
         SpecialType.System_Decimal => "decimal.MinValue",
         _ => "default",
      };
   }

   public static string GenerateMaxValueForType(ITypeSymbol propertyType)
   {
      if (propertyType.TypeKind is TypeKind.Class or TypeKind.Interface)
         // For reference types, max value is null
         return "null";

      // for number types we want to default to max value
      return propertyType.SpecialType switch
      {
         SpecialType.System_SByte => "sbyte.MaxValue",
         SpecialType.System_Byte => "byte.MaxValue",
         SpecialType.System_Int16 => "short.MaxValue",
         SpecialType.System_UInt16 => "ushort.MaxValue",
         SpecialType.System_Int32 => "int.MaxValue",
         SpecialType.System_UInt32 => "uint.MaxValue",
         SpecialType.System_Int64 => "long.MaxValue",
         SpecialType.System_UInt64 => "ulong.MaxValue",
         SpecialType.System_Single => "float.MaxValue",
         SpecialType.System_Double => "double.MaxValue",
         SpecialType.System_Decimal => "decimal.MaxValue",
         _ => "default",
      };
   }

   public static string GenerateDefaultValueForType(ITypeSymbol propertyType,
                                                    INamedTypeSymbol? ieu5ObjectSymbol)
   {
      if (propertyType.TypeKind is TypeKind.Class or TypeKind.Interface)
      {
         // We have to check if it is of the ieu5ObjectSymbol type
         if (ieu5ObjectSymbol != null &&
             propertyType.AllInterfaces.Contains(ieu5ObjectSymbol,
                                                 SymbolEqualityComparer.Default))
         {
            // For IEu5Object types we get the empty value from the registry
            var typeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $"EmptyRegistry.Empties[typeof({typeName})]";
         }

         if (Helpers.IsGenericCollection(propertyType, out _))
         {
            var fullCollectionTypeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $"new {fullCollectionTypeName}()";
         }
         // Check if we have a JominiColor

         return propertyType.ToDisplayString() == JOMINI_COLOR_TYPE
                   ? $"global::{JOMINI_COLOR_TYPE}.Empty"
                   // Fallback SHOULD NEVER BE USED HERE
                   : "null!";
      }

      {
         // For other value types we use the default literal
         var typeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         return $"default({typeName})";
      }
   }

   public static string GenerateDefaultValue(INamedTypeSymbol? ieu5ObjectSymbol,
                                             TypedConstant arg,
                                             IPropertySymbol propertySymbol)
   {
      if (arg.Value == null)
      {
         // If the value is null we need to check the type of the property
         var propertyType = propertySymbol.Type;
         if (propertyType.TypeKind is TypeKind.Class or TypeKind.Interface)
         {
            // We have to check if it is of the ieu5ObjectSymbol type
            if (ieu5ObjectSymbol != null &&
                propertyType.AllInterfaces.Contains(ieu5ObjectSymbol,
                                                    SymbolEqualityComparer.Default))
            {
               // For IEu5Object types we get the empty value from the registry
               var typeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
               return $"EmptyRegistry.Empties[typeof({typeName})]";
            }

            var propertyConfigAttr = Helpers.GetEffectiveAttribute(propertySymbol.ContainingType,
                                                                   propertySymbol,
                                                                   PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME);

            if (propertyConfigAttr != null)
            {
               var pcd = new PropertyConfigData(propertyConfigAttr, propertySymbol);
               if (pcd.DefaultValueMethod != "")
               {
                  // Use the specified default value method
                  return $"{pcd.DefaultValueMethod}()";
               }
            }

            if (Helpers.IsGenericCollection(propertyType, out _))
            {
               var fullCollectionTypeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
               return $"new {fullCollectionTypeName}()";
            }
            // Check if we have a JominiColor

            return propertyType.ToDisplayString() == JOMINI_COLOR_TYPE
                      ? $"global::{JOMINI_COLOR_TYPE}.Empty"
                      // Fallback SHOULD NEVER BE USED HERE
                      : "null!";
         }

         {
            // For other value types we use the default literal
            var typeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $"default({typeName})";
         }
      }

      return FormatTypedConstant(arg);
   }

   public static string FormatObjectDefaultValue(object? obj)
   {
      if (obj == null)
         return "null";

      return obj switch
      {
         string str => SymbolDisplay.FormatLiteral(str, true),
         char c => SymbolDisplay.FormatLiteral(c, true),
         bool b => b.ToString().ToLower(),
         float f => f.ToString("R", CultureInfo.InvariantCulture) + "f",
         double d => d.ToString("R", CultureInfo.InvariantCulture) + "d",
         decimal m => m.ToString(CultureInfo.InvariantCulture) + "m",
         long l => $"{l}L",
         ulong ul => $"{ul}UL",
         int or uint or short or ushort or byte or sbyte => obj.ToString() ?? "0",
         _ => obj.ToString() ?? "null",
      };
   }

   public static string FormatTypedConstant(TypedConstant constant)
   {
      if (constant.Value == null)
         return "null";

      if (constant.Type?.BaseType?.ToDisplayString() == "System.Enum")
      {
         var enumTypeName = constant.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
         var enumValue = constant.Value?.ToString() ?? "0";

         return $"({enumTypeName}){enumValue}";
      }

      var fmv = constant.Type?.SpecialType switch
      {
         SpecialType.System_String => SymbolDisplay.FormatLiteral(constant.Value.ToString() ?? "string.Empty", true),
         SpecialType.System_Char => SymbolDisplay.FormatLiteral((char)(constant.Value ?? '\0'), true),
         SpecialType.System_Boolean => constant.Value?.ToString()?.ToLower() ?? "false",
         SpecialType.System_Single => ((float?)constant.Value)?.ToString("R", CultureInfo.InvariantCulture) + "f",
         SpecialType.System_Double => ((double?)constant.Value)?.ToString("R", CultureInfo.InvariantCulture) + "d",
         SpecialType.System_Decimal => ((decimal?)constant.Value)?.ToString(CultureInfo.InvariantCulture) + "m",
         SpecialType.System_Int64 => $"{constant.Value}L",
         SpecialType.System_UInt64 => $"{constant.Value}UL",
         SpecialType.System_Int32
         or SpecialType.System_UInt32
         or SpecialType.System_Int16
         or SpecialType.System_UInt16
         or SpecialType.System_Byte
         or SpecialType.System_SByte => constant.Value?.ToString() ?? "0",
         _ => constant.Value != null
                 ? $"({constant.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){constant.Value}"
                 : "null",
      };
      if (constant.Type?.SpecialType == SpecialType.System_String &&
          string.IsNullOrWhiteSpace(fmv.Substring(1, fmv.Length - 2)))
         fmv = "string.Empty";
      return fmv;
   }
}