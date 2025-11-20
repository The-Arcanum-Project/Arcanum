using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator;
using ParserGenerator.HelperClasses;

namespace CodeFixers.ContextActions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddEnumAgsDataAttributeCodeFixProvider)), Shared]
public class AddEnumAgsDataAttributeCodeFixProvider : CodeFixProvider
{
   public sealed override ImmutableArray<string> FixableDiagnosticIds
      => [DefinedDiagnostics.MissingEnumAgsDataAttribute.Id];

   public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

   public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
   {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
         return;

      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;

      var enumMemberDeclaration = root.FindToken(diagnosticSpan.Start)
                                      .Parent?.AncestorsAndSelf()
                                      .OfType<EnumMemberDeclarationSyntax>()
                                      .FirstOrDefault();
      if (enumMemberDeclaration == null)
         return;

      var enumMemberName = enumMemberDeclaration.Identifier.Text;

      context.RegisterCodeFix(CodeAction.Create(title: $"Add [EnumAgsData] attribute to '{enumMemberName}'",
                                                createChangedDocument: c
                                                   => AddEnumAgsDataAttributeAsync(context.Document,
                                                    enumMemberDeclaration,
                                                    c),
                                                equivalenceKey: "AddEnumAgsDataAttribute"),
                              diagnostic);
   }

   private static async Task<Document> AddEnumAgsDataAttributeAsync(Document document,
                                                                    EnumMemberDeclarationSyntax enumMemberDecl,
                                                                    CancellationToken cancellationToken)
   {
      var enumMemberName = enumMemberDecl.Identifier.Text;

      var attributeArgument =
         SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                                         SyntaxFactory.Literal(enumMemberName
                                                                           .ToSnakeCase())));
      var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("EnumAgsData"))
                                   .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory
                                                       .SingletonSeparatedList(attributeArgument)));

      var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                          .WithLeadingTrivia(enumMemberDecl.GetLeadingTrivia());

      var newEnumMemberDeclaration = enumMemberDecl
                                    .WithLeadingTrivia()
                                    .AddAttributeLists(newAttributeList);

      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(enumMemberDecl, newEnumMemberDeclaration);

      const string requiredUsing = "Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes";
      if (newRoot is CompilationUnitSyntax compilationUnit &&
          compilationUnit.Usings.All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         newRoot = compilationUnit.AddUsings(usingDirective.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
      }

      return document.WithSyntaxRoot(newRoot);
   }
}