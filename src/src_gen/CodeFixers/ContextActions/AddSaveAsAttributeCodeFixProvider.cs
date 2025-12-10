using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator;

namespace CodeFixers.ContextActions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddSaveAsAttributeCodeFixProvider)), Shared]
public class AddSaveAsAttributeCodeFixProvider : CodeFixProvider
{
   public sealed override ImmutableArray<string> FixableDiagnosticIds => [DefinedDiagnostics.MissingSaveAsAttributeWarning.Id];

   public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
         return;

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
                                    .Parent?.AncestorsAndSelf()
                                    .OfType<PropertyDeclarationSyntax>()
                                    .FirstOrDefault();
      if (propertyDeclaration == null)
         return;

      var propertyName = propertyDeclaration.Identifier.Text;

      context.RegisterCodeFix(CodeAction.Create(title: $"Add [SaveAs] attribute to '{propertyName}'",
                                                createChangedDocument: c
                                                   => AddSaveAsAttributeAsync(context.Document,
                                                                              propertyDeclaration,
                                                                              c),
                                                equivalenceKey: "AddSaveAsAttribute"),
                              diagnostic);
   }

   private static async Task<Document> AddSaveAsAttributeAsync(Document document,
                                                               PropertyDeclarationSyntax propertyDecl,
                                                               CancellationToken cancellationToken)
   {
      var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("SaveAs"));

      var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                          .WithLeadingTrivia(propertyDecl.GetLeadingTrivia());

      var newPropertyDeclaration = propertyDecl
                                  .WithLeadingTrivia()
                                  .AddAttributeLists(newAttributeList);

      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot?.ReplaceNode(propertyDecl, newPropertyDeclaration);

      const string requiredUsing = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes";
      if (newRoot is CompilationUnitSyntax compilationUnit &&
          compilationUnit.Usings.All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         newRoot =
            compilationUnit.AddUsings(usingDirective.WithTrailingTrivia(SyntaxFactory
                                                                          .CarriageReturnLineFeed)); // Add a new line after
      }

      return document.WithSyntaxRoot(newRoot!);
   }
}