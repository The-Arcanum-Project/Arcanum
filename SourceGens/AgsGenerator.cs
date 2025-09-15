using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator.HelperClasses;

namespace ParserGenerator;

[Generator]
public class SavingGenerator : IIncrementalGenerator
{
   private const string IAGS_INTERFACE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.IAgs";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var nexusClassesProvider = context.SyntaxProvider.CreateSyntaxProvider(predicate: (node, _)
                                                                                => node is ClassDeclarationSyntax cds &&
                                                                                   cds.Modifiers
                                                                                     .Any(m => m.IsKind(SyntaxKind
                                                                                                .PartialKeyword)),
                                                                             transform: NexusHelpers
                                                                               .GetNexusClassSymbol)
                                        .Where(s => s is not null);

      context.RegisterSourceOutput(nexusClassesProvider.Collect(),
                                   (spc, nexusClasses) => Execute(nexusClasses, spc));
   }

   private static void Execute(ImmutableArray<INamedTypeSymbol?> nexusClasses, SourceProductionContext context)
   {
      if (nexusClasses.IsDefaultOrEmpty)
         return;

      foreach (var nexusClassSymbol in nexusClasses.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
         try
         {
            NexusHelpers.RunPropertyModifierGenerator(nexusClassSymbol, context);
            if (nexusClassSymbol.AllInterfaces.Any(i => i.ToDisplayString() == IAGS_INTERFACE))
               AgsHelper.RunSavingGenerator(nexusClassSymbol, context);
         }
         catch (Exception ex)
         {
            var descriptor = new DiagnosticDescriptor(id: "AGS001",
                                                      title: "Source Generation Error",
                                                      messageFormat: "An error occurred during source generation: {0}",
                                                      category: "SourceGeneration",
                                                      DiagnosticSeverity.Error,
                                                      isEnabledByDefault: true);

            var diagnostic = Diagnostic.Create(descriptor, Location.None, ex.Message);
            context.ReportDiagnostic(diagnostic);
         }
   }
}