using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiagnosticArgsAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class IEmptyAnalyzer : DiagnosticAnalyzer
{
   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
      => [Diagnostics.ExpectedIEmptyImplementationForINUIObject];

   public override void Initialize(AnalysisContext context)
   {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
   }

   private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
   {
      var classDeclaration = (ClassDeclarationSyntax)context.Node;
      var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

      if (classSymbol == null)
         return;

      var allInterfaces = classSymbol.AllInterfaces;
      var implementsINUI = allInterfaces.Any(i => string.Equals(i.Name, "INUI", StringComparison.Ordinal));

      if (!implementsINUI)
         return;

      var implementsIEmptyCorrectly = false;
      foreach (var implementedInterface in allInterfaces)
         if (implementedInterface.IsGenericType &&
             string.Equals(implementedInterface.Name, "IEmpty", StringComparison.Ordinal))
            if (implementedInterface.TypeArguments.Length == 1)
            {
               var typeArgument = implementedInterface.TypeArguments[0];
               if (!SymbolEqualityComparer.Default.Equals(typeArgument, classSymbol))
                  continue;

               implementsIEmptyCorrectly = true;
               break;
            }

      if (!implementsIEmptyCorrectly)
      {
         var diagnostic = Diagnostic.Create(Diagnostics.ExpectedIEmptyImplementationForINUIObject,
                                            classDeclaration.Identifier.GetLocation(),
                                            classDeclaration.Identifier.ValueText);
         context.ReportDiagnostic(diagnostic);
      }
   }
}