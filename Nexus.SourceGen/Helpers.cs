// --- START OF FILE Helpers.cs ---

using Microsoft.CodeAnalysis;

namespace Nexus.SourceGen;

public static class Helpers
{
   public const string EXPLICIT_PROPERTIES_ATTRIBUTE_STRING = "Nexus.Core.ExplicitPropertiesAttribute";
   public const string IGNORE_MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.IgnoreModifiableAttribute";
   public const string MODIFIABLE_ATTRIBUTE_STRING = "Nexus.Core.AddModifiableAttribute";

   public static List<ISymbol> FindModifiableMembers(INamedTypeSymbol classSymbol,
                                                     bool inclusive,
                                                     SourceProductionContext context)
   {
      var members = new List<ISymbol>();
      // Use a HashSet to track member names we've already processed.
      // This correctly handles member hiding (the 'new' keyword) by prioritizing the member from the most derived class.
      var processedMemberNames = new HashSet<string>();

      var currentType = classSymbol;

      // Walk up the inheritance chain until we hit 'object' or null.
      while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
      {
         foreach (var member in currentType.GetMembers())
         {
            // If we've already added a member with this name from a more derived class, skip.
            if (processedMemberNames.Contains(member.Name))
               continue;

            // Must not be static
            if (member.IsStatic)
               continue;

            // Must be a field or a property
            if (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field)
               continue;

            // For inherited members, they must be accessible from the derived class (public, protected, etc.).
            // For the class's own members, they must be public.
            if (currentType.Equals(classSymbol, SymbolEqualityComparer.Default))
            {
               if (member.DeclaredAccessibility != Accessibility.Public)
                  continue;
            }
            else
            {
               // This is the corrected accessibility check for inherited members.
               if (!IsAccessibleFrom(member, classSymbol))
                  continue;
            }

            // Must not have the [IgnoreModifiable] attribute
            var ignoreAttr = member.GetAttributes()
                                   .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                         IGNORE_MODIFIABLE_ATTRIBUTE_STRING);

            if (ignoreAttr is not null)
            {
               if (inclusive) // not Explicit
                  continue;

               var location = ignoreAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
               if (location != null)
               {
                  // Create and report the diagnostic.
                  var diagnostic = Diagnostic.Create(Diagnostics.RedundantIgnoreAttributeWarning, location);
                  context.ReportDiagnostic(diagnostic);
               }

               continue;
            }

            var addAttr = member.GetAttributes()
                                .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() ==
                                                      MODIFIABLE_ATTRIBUTE_STRING);

            // Must have addition in Explicit mode
            if (addAttr is null)
            {
               if (!inclusive)
                  // In explicit mode, we only include members with [AddModifiable].
                  // This applies to both the class's own members and inherited members.
                  continue;

               // In implicit mode, we have a special case for inherited members:
               // only include them if they are explicitly marked with [AddModifiable].
               // This prevents polluting the derived class with all public members from its base.
               // We only apply this check to members from a base type.
               if (!currentType.Equals(classSymbol, SymbolEqualityComparer.Default))
                  continue;
            }
            else if (inclusive)
            {
               // Only report redundant [AddModifiable] for members of the current class in implicit mode.
               // It's not redundant for an inherited member, as it's required for inclusion.
               if (currentType.Equals(classSymbol, SymbolEqualityComparer.Default))
               {
                  var location = addAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                  if (location != null)
                  {
                     var diagnostic = Diagnostic.Create(Diagnostics.RedundantAddAttributeWarning, location);
                     context.ReportDiagnostic(diagnostic);
                  }
               }
            }

            // For properties, must have an accessible setter
            if (member is IPropertySymbol property)
               // The setter must also be accessible from the derived class.
               if (property.SetMethod == null || !IsAccessibleFrom(property.SetMethod, classSymbol))
                  continue;

            members.Add(member);
            processedMemberNames.Add(member.Name); // Mark this name as processed
         }

         // Move to the next class in the inheritance chain.
         currentType = currentType.BaseType;
      }

      return members;
   }

   /// <summary>
   /// Checks if a member symbol is accessible from a specific derived type symbol.
   /// </summary>
   private static bool IsAccessibleFrom(ISymbol member, INamedTypeSymbol accessingType)
   {
      switch (member.DeclaredAccessibility)
      {
         // Always accessible
         case Accessibility.Public:
         case Accessibility.Protected: // We are checking from a derived type
         case Accessibility.ProtectedOrInternal: // We are checking from a derived type
            return true;

         // Accessible only if in the same assembly
         case Accessibility.Internal:
         case Accessibility.ProtectedAndInternal: // C#'s "private protected"
            return SymbolEqualityComparer.Default.Equals(member.ContainingAssembly, accessingType.ContainingAssembly);

         // Never accessible from a derived type
         case Accessibility.Private:
         default:
            return false;
      }
   }
}