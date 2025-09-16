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
   // IMPORTANT: Replace "YOUR_DIAGNOSTIC_ID" with the actual ID string from DefinedDiagnostics.MissingSaveAsAttribute.Id
   public sealed override ImmutableArray<string> FixableDiagnosticIds
      => [DefinedDiagnostics.MissingSaveAsAttributeWarning.Id];
   // Example: public sealed override ImmutableArray<string> FixableDiagnosticIds => [DefinedDiagnostics.MissingSaveAsAttribute.Id];

   public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
         return;

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      // Find the PropertyDeclarationSyntax node that the diagnostic was reported on.
      var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
                                    .Parent?.AncestorsAndSelf()
                                    .OfType<PropertyDeclarationSyntax>()
                                    .FirstOrDefault();
      if (propertyDeclaration == null)
         return;

      // The diagnostic message contains the property name and type name.
      // We can extract them for a more descriptive title if needed.
      var propertyName = propertyDeclaration.Identifier.Text;

      // Register the code action.
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

      // 2. Create a new attribute list containing our new attribute.
      // This also preserves the leading indentation of the property.
      var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                          .WithLeadingTrivia(propertyDecl.GetLeadingTrivia());

      // 3. Create the new property declaration with the added attribute.
      // We strip the original trivia since we moved it to the attribute list.
      var newPropertyDeclaration = propertyDecl
                                  .WithLeadingTrivia() // Remove original leading trivia
                                  .AddAttributeLists(newAttributeList); // Add the new attribute list

      // 4. Replace the old property declaration with the new one in the syntax tree.
      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(propertyDecl, newPropertyDeclaration);

      // 5. (Optional but recommended) Add the required 'using' directive if it's missing.
      //    Replace "Your.Namespace.For.Attributes" with the actual namespace of SaveAsAttribute.
      const string requiredUsing = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes";
      if (newRoot is CompilationUnitSyntax compilationUnit &&
          compilationUnit.Usings.All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         newRoot =
            compilationUnit.AddUsings(usingDirective.WithTrailingTrivia(SyntaxFactory
                                                                          .CarriageReturnLineFeed)); // Add a new line after
      }

      return document.WithSyntaxRoot(newRoot);
   }
}