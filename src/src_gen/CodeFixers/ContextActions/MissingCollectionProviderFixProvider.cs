using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeFixers.ContextActions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingCollectionProviderFixProvider)), Shared]
public class MissingCollectionProviderFixProvider : CodeFixProvider
{
   public sealed override ImmutableArray<string> FixableDiagnosticIds => ["ARC002"];

   public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
         return;

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var declaration = root.FindToken(diagnosticSpan.Start)
                            .Parent?.AncestorsAndSelf()
                            .OfType<ClassDeclarationSyntax>()
                            .FirstOrDefault();
      if (declaration == null)
         return;

      // Register the code action.
      context.RegisterCodeFix(CodeAction.Create(title: $"Implement ICollectionProvider<{declaration.Identifier.Text}>",
                                                createChangedDocument: c
                                                   => AddCollectionProviderInterfaceAsync(context.Document,
                                                                                          declaration,
                                                                                          c),
                                                equivalenceKey: "AddICollectionProvider"),
                              diagnostic);
   }

   private static async Task<Document> AddCollectionProviderInterfaceAsync(Document document,
                                                                           ClassDeclarationSyntax classDecl,
                                                                           CancellationToken cancellationToken)
   {
      // Get the syntax for the new interface we want to add.
      var newInterfaceName = SyntaxFactory.GenericName(SyntaxFactory.Identifier("ICollectionProvider"),
                                                       SyntaxFactory.TypeArgumentList(SyntaxFactory
                                                                                        .SingletonSeparatedList<TypeSyntax>(SyntaxFactory
                                                                                                  .IdentifierName(classDecl.Identifier.Text))));

      // Create a new declaration with the added interface.
      var newDeclaration = classDecl.AddBaseListTypes(SyntaxFactory.SimpleBaseType(newInterfaceName));

      // Replace the old class declaration with the new one.
      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot?.ReplaceNode(classDecl, newDeclaration);

      const string requiredUsing = "Arcanum.Core.CoreSystems.NUI";
      if (newRoot != null && newRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         var compilationUnit = (CompilationUnitSyntax)newRoot;
         newRoot = compilationUnit.AddUsings(usingDirective);
      }

      return newRoot != null ? document.WithSyntaxRoot(newRoot) : document;
   }
}