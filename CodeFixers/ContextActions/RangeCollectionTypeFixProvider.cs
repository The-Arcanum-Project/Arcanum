using System.Collections.Immutable;
using System.Composition;
using DiagnosticArgsAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeFixers.ContextActions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RangeCollectionTypeFixProvider)), Shared]
public class RangeCollectionTypeFixProvider : CodeFixProvider
{
   
   public sealed override ImmutableArray<string> FixableDiagnosticIds => [Diagnostics.IncorrectCollectionType.Id];

   public sealed override FixAllProvider GetFixAllProvider()
   {
      return WellKnownFixAllProviders.BatchFixer;
   }

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
         return;

      var diagnostic = context.Diagnostics.First();
      var declaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
                            .Parent?.AncestorsAndSelf()
                            .OfType<TypeSyntax>()
                            .FirstOrDefault();
      if (declaration == null)
         return;

      context.RegisterCodeFix(CodeAction.Create(title: "Change to ObservableRangeCollection<T>",
                                                createChangedDocument: c
                                                   => ChangeToObservableRangeCollectionAsync(context.Document,
                                                       declaration,
                                                       c),
                                                equivalenceKey:
                                                "ChangeToObservableRangeCollection"), // A unique key for the fix
                              diagnostic);
   }

   private static async Task<Document> ChangeToObservableRangeCollectionAsync(Document document,
                                                                              TypeSyntax typeSyntax,
                                                                              CancellationToken cancellationToken)
   {
      // Get the semantic model to understand the types.
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
      if (semanticModel == null)
         return document;

      // Get the type symbol for the incorrect collection (e.g., List<string>)
      if (semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type is not INamedTypeSymbol incorrectTypeSymbol)
         return document;

      // Get the inner type argument (e.g., string from List<string>)
      var itemType = incorrectTypeSymbol.TypeArguments.FirstOrDefault();
      if (itemType == null)
         return document;

      // Create the new correct TypeSyntax node.
      // We need to add the correct 'using' statement if it's missing.
      var observableRangeCollectionName = SyntaxFactory.IdentifierName("ObservableRangeCollection");
      var itemTypeName =
         SyntaxFactory.ParseTypeName(itemType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

      var newTypeSyntax = SyntaxFactory.GenericName(observableRangeCollectionName.Identifier,
                                                    SyntaxFactory.TypeArgumentList(SyntaxFactory
                                                          .SingletonSeparatedList(itemTypeName)));

      // Replace the old node with the new one in the syntax tree.
      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(typeSyntax, newTypeSyntax.WithTriviaFrom(typeSyntax));

      // (Optional but Recommended) Add the 'using' statement if needed.
      const string requiredUsing = "Arcanum.Core.CoreSystems.NUI"; // Or your namespace for ObservableRangeCollection
      if (newRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         var compilationUnit = (CompilationUnitSyntax)newRoot;
         newRoot = compilationUnit.AddUsings(usingDirective);
      }

      // Return a new document with the transformed tree.
      return document.WithSyntaxRoot(newRoot);
   }
}