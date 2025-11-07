// --- START OF FILE Helpers.cs ---

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ParserGenerator;

public static class Helpers
{
   public const string EXPLICIT_PROPERTIES_ATTRIBUTE_STRING = "Nexus.Core.ExplicitPropertiesAttribute";
   public const string IGNORE_MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.IgnoreModifiableAttribute";
   public const string MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.AddModifiableAttribute";

   public static IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> CreateClassSyntaxProvider(
      IncrementalGeneratorInitializationContext context)
   {
      return context.SyntaxProvider
                    .CreateSyntaxProvider(predicate: (node, _) => node is ClassDeclarationSyntax,
                                          transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                    .Collect();
   }

   public static List<INamedTypeSymbol> FindTypesImplementingInterface(
      Compilation compilation,
      ImmutableArray<ClassDeclarationSyntax> classes,
      string genericInterfaceName)
   {
      var emptySymbol = compilation.GetTypeByMetadataName(genericInterfaceName);
      if (emptySymbol == null)
         return [];

      List<INamedTypeSymbol> foundTypesByInterface = [];

      foreach (var classSyntax in classes.Distinct())
      {
         var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);

         if (ModelExtensions.GetDeclaredSymbol(semanticModel, classSyntax) is not INamedTypeSymbol
             {
                TypeKind: TypeKind.Class,
             } classSymbol ||
             classSymbol.IsAbstract)
            continue;

         if (classSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition,
                                            emptySymbol)))
            foundTypesByInterface.Add(classSymbol);
      }

      return foundTypesByInterface;
   }

   public static bool InheritsFrom(this INamedTypeSymbol classSymbol, INamedTypeSymbol baseTypeSymbol)
   {
      var currentBaseType = classSymbol.BaseType;

      while (currentBaseType != null)
      {
         var symbolToCompare = currentBaseType.IsGenericType
                                  ? currentBaseType.OriginalDefinition
                                  : currentBaseType;

         if (SymbolEqualityComparer.Default.Equals(symbolToCompare, baseTypeSymbol))
            return true;

         currentBaseType = currentBaseType.BaseType;
      }

      return false;
   }

   /// <summary>
   /// Formats an object from an attribute argument into its C# source code literal representation.
   /// </summary>
   public static string FormatDefaultValueLiteral(TypedConstant? value)
   {
      if (value is null)
         return "null";

      var arg = value.Value;

      var formattedValue = arg.Type!.SpecialType switch
      {
         SpecialType.System_String => SymbolDisplay.FormatLiteral(arg.Value?.ToString() ?? "string.Empty", true),
         SpecialType.System_Char => SymbolDisplay.FormatLiteral((char)(arg.Value ?? '\0'), true),
         SpecialType.System_Boolean => arg.Value?.ToString()?.ToLower() ?? "false",
         SpecialType.System_Single => ((float?)arg.Value)?.ToString("R", CultureInfo.InvariantCulture) + "f",
         SpecialType.System_Double => ((double?)arg.Value)?.ToString("R", CultureInfo.InvariantCulture) + "d",
         SpecialType.System_Decimal => ((decimal?)arg.Value)?.ToString(CultureInfo.InvariantCulture) + "m",
         SpecialType.System_Int64 => $"{arg.Value}L",
         SpecialType.System_UInt64 => $"{arg.Value}UL",
         SpecialType.System_Int32
         or SpecialType.System_UInt32
         or SpecialType.System_Int16
         or SpecialType.System_UInt16
         or SpecialType.System_Byte
         or SpecialType.System_SByte => arg.Value?.ToString() ?? "0",
         _ => arg.Value != null
                 ? $"({arg.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){arg.Value}"
                 : "null",
      };

      return formattedValue;
   }

   public static List<ISymbol> FindModifiableMembers(INamedTypeSymbol classSymbol,
                                                     SourceProductionContext context)
   {
      // --- Use the SHARED helper to get the list of properties ---
      var inclusive = classSymbol.GetAttributes()
                                 .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                       EXPLICIT_PROPERTIES_ATTRIBUTE_STRING) ==
                      null;

      List<string> checkedMembers = [];
      List<ISymbol> finalMembers = [];

      var interfaceMembers = classSymbol.AllInterfaces
                                        .SelectMany(i => i.GetMembers())
                                        .Where(m => m is IPropertySymbol)
                                        .GroupBy(m => m.Name)
                                        .ToDictionary(g => g.Key, g => g.First());
      // It's good practice to ensure FilterModifiableMembers also filters for properties/fields
      FilterModifiableMembers(classSymbol, context, interfaceMembers, inclusive, finalMembers, checkedMembers, true);

      var potentialMembers = new Dictionary<string, ISymbol>();

      // --- Gather all potential members from the entire inheritance hierarchy ---
      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         foreach (var member in currentType.GetMembers())
         {
            if (member.IsStatic ||
                member.IsImplicitlyDeclared ||
                member is not IPropertySymbol && member is not IFieldSymbol)
               continue;

            // Prioritize members from the most derived class.
            // If a member with the same name exists (due to 'new'), it's already been added.
            if (potentialMembers.ContainsKey(member.Name) || checkedMembers.Contains(member.Name))
               continue;

            potentialMembers.Add(member.Name, member);
         }

         currentType = currentType.BaseType;
      }

      FilterModifiableMembers(classSymbol, context, potentialMembers, inclusive, finalMembers, checkedMembers);

      return finalMembers;
   }

   private static void FilterModifiableMembers(INamedTypeSymbol classSymbol,
                                               SourceProductionContext context,
                                               Dictionary<string, ISymbol> potentialMembers,
                                               bool inclusive,
                                               List<ISymbol> finalMembers,
                                               List<string> checkedMembers,
                                               bool isInterface = false)
   {
      if (classSymbol.Name.Contains("GovernmentState"))
      {
      }

      // --- Iterate through the potential members and apply the rules ---
      foreach (var member in potentialMembers.Values)
      {
         if (member.Name.Contains("UniqueId"))
         {
         }

         checkedMembers.Add(member.Name);
         // Must not be static
         if (member.IsStatic)
            continue;

         // Must be a field or a property
         if (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field)
            continue;

         // Member must be accessible from the derived class (public or protected).
         if (!IsAccessibleFrom(member, classSymbol))
            continue;

         // For properties, must have an accessible setter
         if (member is IPropertySymbol property)
            if (property.SetMethod == null || !IsAccessibleFrom(property.SetMethod, classSymbol))
               continue;

         var (ignoreAttr, addAttr) = FindEffectiveAttributes(classSymbol, member.Name);

         if (isInterface && addAttr == null)
            continue; // Require [AddModifiable] on interface members.

         if (ignoreAttr != null)
         {
            if (inclusive) // In implicit mode, [IgnoreModifiable] excludes it.
               continue;

            // In explicit mode, [IgnoreModifiable] is redundant. Report a warning.
            var location = ignoreAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
            if (location != null)
            {
               var diagnostic = Diagnostic.Create(Diagnostics.RedundantIgnoreAttributeWarning, location);
               context.ReportDiagnostic(diagnostic);
            }

            continue;
         }

         if (!inclusive) // Explicit Mode
         {
            // Must have [AddModifiable] to be included.
            if (addAttr == null)
               continue;
         }
         else // Implicit Mode
         {
            var isOriginallyFromInterface = member.ContainingType.TypeKind == TypeKind.Interface;
            if (isOriginallyFromInterface)
            {
               // It's from an interface. The implementation must have [AddModifiable].
               if (addAttr == null)
                  continue;
            }
            else
            {
               if (addAttr != null &&
                   SymbolEqualityComparer.Default.Equals(member.ContainingType, classSymbol))
               {
                  var location = addAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                  if (location != null)
                  {
                     var diagnostic = Diagnostic.Create(Diagnostics.RedundantAddAttributeWarning, location);
                     context.ReportDiagnostic(diagnostic);
                  }
               }
            }
         }

         finalMembers.Add(member);
      }
   }

   /// <summary>
   /// For a given member name, walks up the type hierarchy of a class (including interfaces)
   /// to find the most-derived declaration attribute.
   /// </summary>
   public static AttributeData? GetEffectiveAttribute(
      INamedTypeSymbol classSymbol,
      IPropertySymbol member,
      string attributeFullName)
   {
      AttributeData? data = null;

      var symbolName = member.Name;

      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         var memberOnType = currentType.GetMembers(symbolName).FirstOrDefault();
         if (memberOnType != null)
            data ??= memberOnType.GetAttributes()
                                 .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                       attributeFullName);

         currentType = currentType.BaseType;
      }

      if (data != null)
         return data;

      foreach (var interFace in classSymbol.AllInterfaces)
      {
         var interfaceMembers = interFace.GetMembers(symbolName).FirstOrDefault();
         if (interfaceMembers != null)
            data ??= interfaceMembers.GetAttributes()
                                     .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                           attributeFullName);
      }

      return data;
   }

   /// <summary>
   /// For a given member name, walks up the type hierarchy of a class (including interfaces)
   /// to find the most-derived declaration and its [IgnoreModifiable] or [AddModifiable] attributes.
   /// </summary>
   private static (AttributeData? Ignore, AttributeData? Add) FindEffectiveAttributes(
      INamedTypeSymbol classSymbol,
      string memberName)
   {
      AttributeData? ignoreAttr = null;
      AttributeData? addAttr = null;

      // Search the class and its base classes first (most-derived wins)
      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         var memberOnType = currentType.GetMembers(memberName).FirstOrDefault();
         if (memberOnType != null)
         {
            // If we haven't found an attribute yet, check this level.
            // This correctly gives precedence to attributes on derived classes.
            ignoreAttr ??= memberOnType.GetAttributes()
                                       .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                             IGNORE_MODIFIABLE_ATTRIBUTE_STRING);
            addAttr ??= memberOnType.GetAttributes()
                                    .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                          MODIFIABLE_ATTRIBUTE_STRING);
         }

         currentType = currentType.BaseType;
      }

      // Only if we haven't found attributes yet, check the interfaces.
      // This ensures class attributes always override interface attributes.
      if (ignoreAttr == null && addAttr == null)
      {
         foreach (var iface in classSymbol.AllInterfaces)
         {
            var memberOnIface = iface.GetMembers(memberName).FirstOrDefault();
            if (memberOnIface != null)
            {
               ignoreAttr ??= memberOnIface.GetAttributes()
                                           .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                                 IGNORE_MODIFIABLE_ATTRIBUTE_STRING);
               addAttr ??= memberOnIface.GetAttributes()
                                        .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                              MODIFIABLE_ATTRIBUTE_STRING);
            }
         }
      }

      return (ignoreAttr, addAttr);
   }

   // You will also need this helper from a previous answer
   private static ISymbol FindOriginalDefinition(ISymbol symbol)
   {
      if (symbol is IPropertySymbol propertySymbol)
      {
         while (propertySymbol.OverriddenProperty != null)
            propertySymbol = propertySymbol.OverriddenProperty;

         return propertySymbol;
      }

      return symbol;
   }

   private static bool IsAccessibleFrom(ISymbol member, INamedTypeSymbol accessingType)
   {
      if (member == null!)
         return false;

      switch (member.DeclaredAccessibility)
      {
         case Accessibility.Public:
         case Accessibility.Protected:
         case Accessibility.ProtectedOrInternal:
            return true;
         case Accessibility.Internal:
         case Accessibility.ProtectedAndInternal:
            return SymbolEqualityComparer.Default.Equals(member.ContainingAssembly,
                                                         accessingType.ContainingAssembly);
         case Accessibility.Private:
         default:
            return false;
      }
   }

   public static string? GetEnumMemberName(TypedConstant enumTypedConstant)
   {
      if (enumTypedConstant.Type is not INamedTypeSymbol enumTypeSymbol)
         return null;

      var enumMemberSymbol = enumTypeSymbol.GetMembers()
                                           .OfType<IFieldSymbol>()
                                           .FirstOrDefault(f => f.ConstantValue != null &&
                                                                f.ConstantValue.Equals(enumTypedConstant.Value));

      return enumMemberSymbol?.Name;
   }

   public static bool IsListOrCollection(IPropertySymbol propertySymbol)
   {
      var type = propertySymbol.Type;

      if (type.TypeKind == TypeKind.Array)
         return false;

      foreach (var @interface in type.AllInterfaces)
         if (@interface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IList<T>" ||
             @interface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.ICollection<T>")
            return true;

      return false;
   }

   public static bool IsGenericCollection(ITypeSymbol typeSymbol, out INamedTypeSymbol? itemType)
   {
      itemType = null;
      if (typeSymbol is not INamedTypeSymbol namedType)
      {
         return false;
      }

      // Find the implementation of IEnumerable<T>
      var ienumerable = namedType.AllInterfaces.FirstOrDefault(i =>
                                                                  i.OriginalDefinition.ToDisplayString() ==
                                                                  "System.Collections.Generic.IEnumerable<T>");

      if (ienumerable != null && ienumerable.TypeArguments.Length == 1)
      {
         // We found it, extract the item type T
         itemType = ienumerable.TypeArguments[0] as INamedTypeSymbol;
         return itemType != null;
      }

      return false;
   }

   public static bool IsCollectionOfObjects(IPropertySymbol property, out INamedTypeSymbol? itemType)
   {
      itemType = null;
      var type = property.Type;

      // Handle string first, as it's a common edge case (implements IEnumerable<char>) ---
      if (type.SpecialType == SpecialType.System_String)
         return false;

      // --- 2. Handle Arrays: T[] ---
      if (type is IArrayTypeSymbol arrayType)
      {
         if (arrayType.ElementType is not INamedTypeSymbol { TypeKind: TypeKind.Class } elementAsNamedType1 ||
             elementAsNamedType1.SpecialType == SpecialType.System_String)
            return false;

         itemType = elementAsNamedType1;
         return true;
      }

      // --- Handle Generic Collections: List<T>, HashSet<T>, etc. ---
      // We look for an implementation of IEnumerable<T>.
      var ienumerableInterface = type.AllInterfaces
                                     .FirstOrDefault(i => i.IsGenericType &&
                                                          i.OriginalDefinition.SpecialType ==
                                                          SpecialType.System_Collections_Generic_IEnumerable_T);

      var typeArgument = ienumerableInterface?.TypeArguments.FirstOrDefault();

      if (typeArgument is not INamedTypeSymbol { TypeKind: TypeKind.Class } elementAsNamedType ||
          elementAsNamedType.SpecialType == SpecialType.System_String)
         return false;

      itemType = elementAsNamedType;
      return true;
   }
}