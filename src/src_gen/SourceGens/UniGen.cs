using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator.HelperClasses;
using ParserGenerator.IAgsGen;
using ParserGenerator.NexusGeneration;

namespace ParserGenerator;

[Generator]
public class UniGen : IIncrementalGenerator
{
   public const string IAGS_INTERFACE = "Arcanum.Core.CoreSystems.SavingSystem.AGS.IAgs";

   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
      var nexusClassesProvider = context.SyntaxProvider
                                        .CreateSyntaxProvider(predicate: (node, _)
                                                                 => node is ClassDeclarationSyntax cds &&
                                                                    cds.Modifiers.Any(m => m.IsKind(SyntaxKind
                                                                                                      .PartialKeyword)),
                                                              transform: DataGatherer.GetNexusClassSymbol)
                                        .Where(s => s is not null);

      var combined = nexusClassesProvider.Collect()
                                         .Combine(context.CompilationProvider);

      context.RegisterSourceOutput(combined,
                                   (spc, tuple) =>
                                   {
                                      var (nexusClasses, compilation) = tuple;

                                      Execute(nexusClasses, spc, compilation);
                                   });
   }

   private static void Execute(ImmutableArray<INamedTypeSymbol?> nexusClasses,
                               SourceProductionContext context,
                               Compilation compilation)
   {
      if (nexusClasses.IsDefaultOrEmpty)
         return;

      var enumerableSymbol = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
      var iListSymbol = compilation.GetTypeByMetadataName("System.Collections.IList");
      var ieu5ObjectSymbol = compilation.GetTypeByMetadataName("Arcanum.Core.GameObjects.BaseTypes.IEu5Object");
      var iAgsSymbol = compilation.GetTypeByMetadataName("Arcanum.Core.CoreSystems.SavingSystem.AGS.IAgs");
      if (enumerableSymbol is null || ieu5ObjectSymbol is null || iListSymbol is null || iAgsSymbol is null)
         return;

      AgsHelper.EnumAnalysisCache = [];

      var nxClasses = nexusClasses.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToList();
      List<INamedTypeSymbol> iagsClasses = [];
      foreach (var nexusClassSymbol in nxClasses)
         try
         {
            // NexusHelpers.RunPropertyModifierGenerator(nexusClassSymbol,
            //                                           context,
            //                                           enumerableSymbol,
            //                                           ieu5ObjectSymbol,
            //                                           iListSymbol);

            var implementsIAgs = nexusClassSymbol.AllInterfaces.Any(i =>
                                                                       SymbolEqualityComparer.Default.Equals(i, iAgsSymbol) ||
                                                                       SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, iAgsSymbol));

            if (implementsIAgs)
            {
               AgsHelper.RunSavingGenerator(nexusClassSymbol, context, compilation);
               iagsClasses.Add(nexusClassSymbol);
            }
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

      AgsSettingsHolderGen.GenerateSettingsHolder(context, iagsClasses);
      Generator.RunNexusGenerator(nexusClasses.Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToArray(), context, compilation);

      EnumIndexGenerator.RunEnumIndexGenerator(context, AgsHelper.EnumAnalysisCache);
   }
}