using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CodeFixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IEmptyCodeFixProvider)), Shared]
public class IEmptyCodeFixProvider : CodeFixProvider
{
   public sealed override ImmutableArray<string> FixableDiagnosticIds => ["ARC003"];

   public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      // Find the class declaration that triggered the diagnostic.
      var classDeclaration = root.FindToken(diagnosticSpan.Start)
                                 .Parent?.AncestorsAndSelf()
                                 .OfType<ClassDeclarationSyntax>()
                                 .First();

      // Register a code action that will invoke our fix.
      context.RegisterCodeFix(CodeAction.Create(title: "Implement IEmpty<T>",
                                                createChangedSolution: c
                                                   => AddIEmptyInterfaceAsync(context.Document, classDeclaration!, c),
                                                equivalenceKey: "Implement IEmpty<T>"),
                              diagnostic);
   }

   private static async Task<Solution> AddIEmptyInterfaceAsync(Document document,
                                                               ClassDeclarationSyntax classDecl,
                                                               CancellationToken cancellationToken)
   {
      // Get the name of the class itself
      var className = classDecl.Identifier.ValueText;

      // --- Create the new interface syntax ---
      // This creates the syntax for: IEmpty<ClassName>
      var iemptyInterface = SyntaxFactory.GenericName(SyntaxFactory.Identifier("IEmpty"))
                                         .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory
                                                                     .SingletonSeparatedList<TypeSyntax>(SyntaxFactory
                                                                            .IdentifierName(className))));

      // Use DocumentEditor, a high-level API for syntax tree modifications
      var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

      // Add the new interface to the class's list of base types
      editor.AddInterfaceType(classDecl, iemptyInterface);

      // Get the new document with the changes applied
      var newDocument = editor.GetChangedDocument();

      return newDocument.Project.Solution;
   }
}