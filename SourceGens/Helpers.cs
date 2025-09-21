// --- START OF FILE Helpers.cs ---

using Microsoft.CodeAnalysis;

namespace ParserGenerator;

public static class Helpers
{
   public const string EXPLICIT_PROPERTIES_ATTRIBUTE_STRING = "Nexus.Core.ExplicitPropertiesAttribute";
   public const string IGNORE_MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.IgnoreModifiableAttribute";
   public const string MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.AddModifiableAttribute";

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
   public static string FormatDefaultValueLiteral(object? value)
   {
      if (value is null)
         return "null";

      if (value is string stringValue)
         return $"@\"{stringValue.Replace("\"", "\"\"")}\"";

      if (value is bool boolValue)
         return boolValue ? "true" : "false";

      if (value.GetType().IsPrimitive || value is decimal)
         return value.ToString();

      return $"\"{value}\"";
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
                                        .ToDictionary(m => m.Name, m => m);
      FilterModifiableMembers(classSymbol, context, interfaceMembers, inclusive, finalMembers, checkedMembers, true);

      var potentialMembers = new Dictionary<string, ISymbol>();

      // --- Gather all potential members from the entire inheritance hierarchy ---
      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         foreach (var member in currentType.GetMembers())
         {
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
         {
            propertySymbol = propertySymbol.OverriddenProperty;
         }

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

   public static bool IsPropertySymbolACollection(IPropertySymbol? propertySymbol)
   {
      if (propertySymbol == null)
         return false;

      if (propertySymbol.Type.SpecialType == SpecialType.System_String)
         return false;

      var type = propertySymbol.Type;

      // Check if the type is an array
      if (type.TypeKind == TypeKind.Array)
         return true;

      // Check if the type implements IEnumerable<T> or IEnumerable
      foreach (var @interface in type.AllInterfaces)
         if (@interface.SpecialType == SpecialType.System_Collections_IEnumerable ||
             @interface.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
            return true;

      return false;
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
}