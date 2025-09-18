using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DiagnosticArgsAnalyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RangeCollectionAnalyzer : DiagnosticAnalyzer
{
   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Diagnostics.IncorrectCollectionType];

   public override void Initialize(AnalysisContext context)
   {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSymbolAction(AnalyzePropertySymbol, SymbolKind.Property);
   }

   private void AnalyzePropertySymbol(SymbolAnalysisContext context)
   {
      if (context.Symbol is not IPropertySymbol propertySymbol)
         return;

      if (IsNexusProperty(context, propertySymbol))
         return;

      var expectedValueType = propertySymbol.Type;

      var requiredCollectionSymbol =
         context.Compilation.GetTypeByMetadataName("Arcanum.Core.CoreSystems.NUI.ObservableRangeCollection`1");

      var iCollectionInterfaceSymbol =
         context.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");

      if (requiredCollectionSymbol != null &&
          iCollectionInterfaceSymbol != null &&
          expectedValueType is INamedTypeSymbol namedExpectedType)
      {
         // Check if the property's type is a generic collection.
         var isAnyCollection = namedExpectedType.OriginalDefinition.AllInterfaces.Any(i =>
                                      SymbolEqualityComparer.Default.Equals(i.OriginalDefinition,
                                                                            iCollectionInterfaceSymbol)) ||
                               SymbolEqualityComparer.Default.Equals(namedExpectedType.OriginalDefinition,
                                                                     iCollectionInterfaceSymbol);

         if (isAnyCollection)
            if (!SymbolEqualityComparer.Default.Equals(namedExpectedType.OriginalDefinition,
                                                       requiredCollectionSymbol))
            {
               var itemType = namedExpectedType.TypeArguments.FirstOrDefault();

               // --- THIS IS THE CHANGE ---
               // Find the syntax node for the property's type.
               var propertySyntax =
                  propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;
               var location = propertySyntax?.Type.GetLocation() ?? propertySymbol.Locations[0];

               context.ReportDiagnostic(Diagnostic.Create(Diagnostics.IncorrectCollectionType,
                                                          location, // Use the location of the TYPE SYNTAX
                                                          propertySymbol.Name,
                                                          requiredCollectionSymbol.ToDisplayString(SymbolDisplayFormat
                                                                .MinimallyQualifiedFormat),
                                                          itemType?.ToDisplayString(SymbolDisplayFormat
                                                                .MinimallyQualifiedFormat) ??
                                                          "T"));
            }
      }
   }

   private static bool IsNexusProperty(SymbolAnalysisContext context, IPropertySymbol propertySymbol)
   {
      var nexusInterfaceSymbol = context.Compilation.GetTypeByMetadataName("Nexus.Core.INexus");
      if (nexusInterfaceSymbol == null ||
          !propertySymbol.ContainingType.AllInterfaces.Contains(nexusInterfaceSymbol, SymbolEqualityComparer.Default))
         return true;

      var fieldEnumSymbol = propertySymbol.ContainingType.GetTypeMembers("Field")
                                          .FirstOrDefault(m => m.TypeKind == TypeKind.Enum);

      if (fieldEnumSymbol == null)
         // The INexus object doesn't have the generated 'Field' enum.
         // This could be a temporary state during typing, or the generator hasn't run.
         // It's safest to do nothing in this case.
         return true;

      var hash = new HashSet<string>(fieldEnumSymbol.GetMembers().Select(m => m.Name));
      if (hash.Contains(propertySymbol.Name))
         return !true;

      return !false;
   }
}