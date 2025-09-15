// using System.Collections.Immutable;
// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.CodeAnalysis.Text;
//
// namespace ParserGenerator;
//
// [Generator]
// public class PropertyModifierGenerator : IIncrementalGenerator
// {
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//       const string targetInterfaceName = "Nexus.Core.INexus";
//
//       var classDeclarations = context.SyntaxProvider
//                                      .CreateSyntaxProvider(predicate: Predicate,
//                                                            transform: Transform)
//                                      .Where(x => x is not null); // Filter out the nulls
//
//       // Combine them with the compilation
//       var compilationAndClasses
//          = context.CompilationProvider.Combine(classDeclarations.Collect());
//
//       // Generate the source
//       context.RegisterSourceOutput(compilationAndClasses,
//                                    (spc, source) => Execute(source.Left, source.Right!, spc));
//       return;
//
//       ClassDeclarationSyntax? Transform(GeneratorSyntaxContext genSyntaxCtx, CancellationToken cancellationToken)
//       {
//          var classDeclaration = (ClassDeclarationSyntax)genSyntaxCtx.Node;
//          var semanticModel = genSyntaxCtx.SemanticModel;
//
//          // Get the symbol for the interface we're looking for
//          var interfaceSymbol = semanticModel.Compilation.GetTypeByMetadataName(targetInterfaceName);
//          if (interfaceSymbol is null)
//             // The interface isn't defined in this compilation, so no classes can implement it.
//             return null;
//
//          // Get the symbol for the class we're inspecting. 
//          // GetDeclaredSymbol returns ISymbol, so we must safely cast it to INamedTypeSymbol.
//
//          // Check if the class implements the interface, using the SymbolEqualityComparer for a robust check
//          if (genSyntaxCtx.SemanticModel.GetDeclaredSymbol(classDeclaration,
//                                                           cancellationToken) is { } classSymbol &&
//              classSymbol.AllInterfaces.Contains(interfaceSymbol,
//                                                 SymbolEqualityComparer.Default))
//             return classDeclaration;
//
//          return null;
//       }
//    }
//
//    private bool Predicate(SyntaxNode node, CancellationToken _)
//    {
//       if (node is ClassDeclarationSyntax cds &&
//           cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
//          return true;
//
//       return false;
//    }
//
//    private void Execute(Compilation compilation,
//                         ImmutableArray<ClassDeclarationSyntax> classes,
//                         SourceProductionContext context)
//    {
//       if (classes.IsDefaultOrEmpty)
//          return;
//
//       var distinctClasses = classes.Distinct();
//
//       foreach (var classSyntax in distinctClasses)
//       {
//          context.CancellationToken.ThrowIfCancellationRequested();
//
//          var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
//
//          if (ModelExtensions.GetDeclaredSymbol(semanticModel, classSyntax) is not INamedTypeSymbol classSymbol)
//             continue;
//
//          // Find all eligible properties and fields
//          RunNexusGenerator(context, classSymbol);
//       }
//    }
//
//    public static void RunNexusGenerator(SourceProductionContext context, INamedTypeSymbol classSymbol)
//    {
//       var members = Helpers.FindModifiableMembers(classSymbol, context);
//
//       // Generate the code for this class
//       var sourceCode = GeneratePartialClass(classSymbol, members);
//
//       // Add the generated source file to the compilation
//       
//       context.AddSource(hintName, SourceText.From(sourceCode, Encoding.UTF8));
//    }
//
//    
//
//    
// }

