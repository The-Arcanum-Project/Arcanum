// --- START OF FILE Helpers.cs ---

using Microsoft.CodeAnalysis;

namespace ParserGenerator;

public static class Helpers
{
   public const string EXPLICIT_PROPERTIES_ATTRIBUTE_STRING = "Nexus.Core.ExplicitPropertiesAttribute";
   public const string IGNORE_MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.IgnoreModifiableAttribute";
   public const string MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.AddModifiableAttribute";

   public static List<ISymbol> FindModifiableMembers(INamedTypeSymbol classSymbol,
                                                     SourceProductionContext context)
   {
      // --- Use the SHARED helper to get the list of properties ---
      var inclusive = classSymbol.GetAttributes()
                                 .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                       EXPLICIT_PROPERTIES_ATTRIBUTE_STRING) ==
                      null;

      var finalMembers = new List<ISymbol>();
      var potentialMembers = new Dictionary<string, ISymbol>();

      // --- Gather all potential members from the entire inheritance hierarchy ---
      var currentType = classSymbol;
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         foreach (var member in currentType.GetMembers())
         {
            // Prioritize members from the most derived class.
            // If a member with the same name exists (due to 'new'), it's already been added.
            if (potentialMembers.ContainsKey(member.Name))
               continue;

            potentialMembers.Add(member.Name, member);
         }

         currentType = currentType.BaseType;
      }

      // --- Iterate through the potential members and apply the rules ---
      foreach (var member in potentialMembers.Values)
      {
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

         // --- Check for Ignore attribute anywhere in the hierarchy ---
         // Find the member in the most-derived class that might be hiding the base member.
         var mostDerivedMember = classSymbol.FindImplementationForInterfaceMember(member) ??
                                 classSymbol.GetMembers(member.Name).FirstOrDefault() ?? member;

         var ignoreAttr = mostDerivedMember.GetAttributes()
                                           .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                                 IGNORE_MODIFIABLE_ATTRIBUTE_STRING);

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

         // --- Check for Add attribute on the most relevant member ---
         var addAttr = mostDerivedMember.GetAttributes()
                                        .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                              MODIFIABLE_ATTRIBUTE_STRING);

         if (!inclusive) // Explicit Mode
         {
            // Must have [AddModifiable] to be included.
            if (addAttr == null)
               continue;
         }
         else // Implicit Mode
         {
            if (addAttr != null)
            {
               // [AddModifiable] is redundant in implicit mode unless it's on an inherited member
               // that is NOT from the class itself.
               if (SymbolEqualityComparer.Default.Equals(mostDerivedMember.ContainingType, classSymbol))
               {
                  var location = addAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                  if (location != null)
                  {
                     var diagnostic = Diagnostic.Create(Diagnostics.RedundantAddAttributeWarning, location);
                     context.ReportDiagnostic(diagnostic);
                  }
               }
            }
            else if (!SymbolEqualityComparer.Default.Equals(member.ContainingType, classSymbol))
            {
               // In implicit mode, inherited members MUST be opted-in with [AddModifiable].
               // Since addAttr is null, we skip this inherited member.
               continue;
            }
         }

         finalMembers.Add(member);
      }

      return finalMembers;
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
}