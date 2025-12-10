using Microsoft.CodeAnalysis;
using ParserGenerator.HelperClasses;

namespace ParserGenerator.NexusGeneration;

public struct NexusPropertyData
{
   private const string PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME =
      "Arcanum.Core.CoreSystems.NUI.Attributes.PropertyConfigAttribute";

   private const string DEFAULT_VALUE_ATTRIBUTE_FULL_NAME = "System.ComponentModel.DefaultValueAttribute";
   private const string DESCRIPTION_ATTRIBUTE_FULL_NAME = "System.ComponentModel.DescriptionAttribute";

   public string PropertyName;
   public string DefaultValue;
   public string? Description;
   public bool IsCollection;
   public INamedTypeSymbol PropertyType;
   public PropertyConfigData PropertyConfigData;
   public ITypeSymbol? CollectionItemType;
   public INamedTypeSymbol? CollectionType;

   public NexusPropertyData(ISymbol symbol,
                            IPropertySymbol propertySymbol,
                            INamedTypeSymbol classSymbol,
                            SourceProductionContext context,
                            INamedTypeSymbol ieu5ObjectSymbol)
   {
      var defaultValueAttribute = Helpers.GetEffectiveAttribute(classSymbol, propertySymbol, DEFAULT_VALUE_ATTRIBUTE_FULL_NAME);
      var propertyConfigAttribute = Helpers.GetEffectiveAttribute(classSymbol, propertySymbol, PROPERTY_CONFIG_ATTRIBUTE_FULL_NAME);
      var descriptionAttribute = Helpers.GetEffectiveAttribute(classSymbol, propertySymbol, DESCRIPTION_ATTRIBUTE_FULL_NAME);

      if (defaultValueAttribute is not { ConstructorArguments.Length: 1 })
      {
         DefaultValue = "CAN_NOT_DETERMINE_DEFAULT_VALUE";
         // add a warning here
         context.ReportDiagnostic(Diagnostic.Create(new("NXGEN002",
                                                        "Missing or invalid DefaultValue attribute",
                                                        $"Property '{propertySymbol.Name}' is missing a DefaultValue attribute or it is invalid. The default value will be set to null.",
                                                        "NexusGenerator",
                                                        DiagnosticSeverity.Warning,
                                                        true),
                                                    Location.None));
      }
      else
         DefaultValue = PropertyConfigHelper.GenerateDefaultValue(ieu5ObjectSymbol, defaultValueAttribute.ConstructorArguments[0], propertySymbol);

      Description = descriptionAttribute is null
                       ? null
                       : descriptionAttribute.ConstructorArguments.Length == 1 &&
                         descriptionAttribute.ConstructorArguments[0].Value is string desc
                          ? desc
                          : null;

      PropertyConfigData = PropertyConfigHelper.GeneratePropertyConfigData(propertySymbol, classSymbol, propertyConfigAttribute);

      PropertyName = propertySymbol.Name;
      PropertyType = propertySymbol.Type as INamedTypeSymbol ?? throw new InvalidOperationException("Property type is not a named type symbol.");

      var typeSymbol = propertySymbol.Type;
      if (typeSymbol.SpecialType == SpecialType.System_String)
      {
         IsCollection = false;
         CollectionItemType = null;
         CollectionType = null;
      }
      // Handle Arrays (e.g. int[])
      else if (typeSymbol is IArrayTypeSymbol arraySymbol)
      {
         IsCollection = true;
         CollectionItemType = arraySymbol.ElementType;
         CollectionType = null; // Arrays don't have a NamedType definition in the same way
      }
      // Handle Generic Collections (List<T>, HashSet<T>, IList<T>, etc.)
      else
      {
         // We search for IEnumerable<T> because List, HashSet, and IList all implement it.
         INamedTypeSymbol? genericEnumerable = null;

         if (typeSymbol is INamedTypeSymbol namedSym)
         {
            // Check if the type ITSELF is IEnumerable<T> (e.g. public IEnumerable<int> MyProp { get; })
            if (namedSym.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            {
               genericEnumerable = namedSym;
            }
            else
            {
               // Check all inherited interfaces
               genericEnumerable = namedSym.AllInterfaces.FirstOrDefault(i =>
                                                                            i.OriginalDefinition.SpecialType ==
                                                                            SpecialType.System_Collections_Generic_IEnumerable_T);
            }
         }

         if (genericEnumerable != null)
         {
            IsCollection = true;
            CollectionItemType = genericEnumerable.TypeArguments.FirstOrDefault();
            CollectionType = typeSymbol as INamedTypeSymbol;
         }
         else
         {
            IsCollection = false;
            CollectionItemType = null;
            CollectionType = null;
         }
      }
   }
}