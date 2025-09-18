using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiagnosticArgsAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class INUIAnalyzer : DiagnosticAnalyzer
{
   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
      => [Diagnostics.MissingCollectionProviderInterface];

   public override void Initialize(AnalysisContext context)
   {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
   }

   private static void AnalyzeNamedType(SymbolAnalysisContext context)
   {
      // We only care about classes that are not abstract.
      if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } classSymbol ||
          classSymbol.IsAbstract)
         return;

      var isPartial = classSymbol.DeclaringSyntaxReferences
                                 .Select(r => r.GetSyntax())
                                 .OfType<TypeDeclarationSyntax>()
                                 .Any(t => t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

      if (!isPartial)
         return;

      // Get the symbols for the interfaces we care about.
      var inuiInterfaceSymbol = context.Compilation.GetTypeByMetadataName("Arcanum.Core.CoreSystems.NUI.INUI");
      var collectionProviderInterfaceSymbol =
         context.Compilation.GetTypeByMetadataName("Arcanum.Core.CoreSystems.NUI.IEu5ObjectProvider`1");

      if (inuiInterfaceSymbol == null || collectionProviderInterfaceSymbol == null)
         return;

      // Check if the class implements INUI.
      var implementsINUI = classSymbol.AllInterfaces.Contains(inuiInterfaceSymbol, SymbolEqualityComparer.Default);
      if (!implementsINUI)
         return;

      // Construct the specific interface we expect: ICollectionProvider<ThisClassType>
      var requiredProviderInterface = collectionProviderInterfaceSymbol.Construct(classSymbol);

      // Check if the class implements this required interface.
      var implementsRequiredProvider =
         classSymbol.AllInterfaces.Contains(requiredProviderInterface, SymbolEqualityComparer.Default);

      // If it doesn't, report the diagnostic on the class name.
      if (!implementsRequiredProvider)
         context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingCollectionProviderInterface,
                                                    classSymbol.Locations[0], // Location of the class declaration
                                                    classSymbol.Name));
   }
}