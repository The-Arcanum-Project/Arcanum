using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ParserGenerator;

namespace CodeFixers.ContextActions;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddDefaultValueAttributeCodeFixProvider)), Shared]
public class AddDefaultValueAttributeCodeFixProvider : CodeFixProvider
{
   public sealed override ImmutableArray<string> FixableDiagnosticIds
      => [DefinedDiagnostics.MissingDefaultValueAttributeWarning.Id];

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

      context.RegisterCodeFix(CodeAction.Create(title: $"Add [DefaultValue] attribute to '{propertyName}'",
                                                createChangedDocument: c
                                                   => AddDefaultValueAttributeAsync(context.Document,
                                                    propertyDeclaration,
                                                    c),
                                                equivalenceKey: "AddDefaultValueAttribute"),
                              diagnostic);
   }

   private static async Task<Document> AddDefaultValueAttributeAsync(Document document,
                                                                     PropertyDeclarationSyntax propertyDecl,
                                                                     CancellationToken cancellationToken)
   {
      // The core new logic: determine the property type and create the default value syntax
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
      if (semanticModel == null ||
          ModelExtensions.GetDeclaredSymbol(semanticModel, propertyDecl) is not IPropertySymbol propertySymbol)
         return document;

      var defaultValueExpression = CreateDefaultValueExpression(propertySymbol.Type);

      // Create the attribute syntax: [DefaultValue(...)]
      var attributeArgument = SyntaxFactory.AttributeArgument(defaultValueExpression);
      var attribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DefaultValue"))
                                   .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory
                                                       .SingletonSeparatedList(attributeArgument)));

      // This logic for handling trivia and adding the attribute is reused from your example
      var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                                          .WithLeadingTrivia(propertyDecl.GetLeadingTrivia());

      var newPropertyDeclaration = propertyDecl
                                  .WithLeadingTrivia()
                                  .AddAttributeLists(newAttributeList);

      var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      var newRoot = oldRoot.ReplaceNode(propertyDecl, newPropertyDeclaration);

      // DefaultValueAttribute is in System.ComponentModel
      const string requiredUsing = "System.ComponentModel";
      if (newRoot is CompilationUnitSyntax compilationUnit &&
          compilationUnit.Usings.All(u => u.Name?.ToString() != requiredUsing))
      {
         var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(requiredUsing));
         newRoot = compilationUnit.AddUsings(usingDirective.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
      }

      return document.WithSyntaxRoot(newRoot);
   }

   /// <summary>
   /// Creates the syntax for the default value of a given type.
   /// </summary>
   private static ExpressionSyntax CreateDefaultValueExpression(ITypeSymbol? typeSymbol)
   {
      if (typeSymbol == null)
         return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

      // Handle common special types
      switch (typeSymbol.SpecialType)
      {
         case SpecialType.System_Boolean:
            return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
         case SpecialType.System_String:
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
         case SpecialType.System_Char:
         case SpecialType.System_SByte:
         case SpecialType.System_Byte:
         case SpecialType.System_Int16:
         case SpecialType.System_UInt16:
         case SpecialType.System_Int32:
         case SpecialType.System_UInt32:
         case SpecialType.System_Int64:
         case SpecialType.System_UInt64:
         case SpecialType.System_Decimal:
         case SpecialType.System_Single:
         case SpecialType.System_Double:
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
      }

      // Handle enums by casting 0 to the enum type, e.g., (MyEnum)0
      if (typeSymbol.TypeKind == TypeKind.Enum)
         return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString()),
                                             SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                                                             SyntaxFactory.Literal(0)));

      // For any other reference type (class, interface, delegate, etc.)
      if (typeSymbol.IsReferenceType)
         return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

      // For other value types (structs), 'null' is often the only valid constant
      // that can be supplied to the DefaultValue attribute if no specific constructor is used.
      // The compiler will handle if this is appropriate.
      return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
   }
}