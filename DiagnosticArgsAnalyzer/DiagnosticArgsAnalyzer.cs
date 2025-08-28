using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DiagnosticArgsAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DiagnosticArgsAnalyzer : DiagnosticAnalyzer
{
   private static readonly DiagnosticDescriptor Rule = new("DA001",
                                                           "Incorrect diagnostic arguments",
                                                           "The call provides {0} argument(s), but the descriptor expects {1}: {2}", // New format
                                                           "Usage",
                                                           DiagnosticSeverity.Error,
                                                           isEnabledByDefault: true);

   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

   public override void Initialize(AnalysisContext context)
   {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
   }

   private static readonly ImmutableHashSet<string> TargetMethodNames =
      ImmutableHashSet.Create("LogWarning", "CreateAndHandle");

   private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
   {
      var invocation = (InvocationExpressionSyntax)context.Node;

      if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
         return;

      // --- Step 2: Check if the method is one we care about ---
      if (!TargetMethodNames.Contains(methodSymbol.Name))
         return;

      // --- Step 3: Find the parameters we need from the method's signature ---
      var descriptorParameter =
         methodSymbol.Parameters.FirstOrDefault(p => p.Type.Name == nameof(DiagnosticDescriptor));
      var paramsParameter =
         methodSymbol.Parameters.FirstOrDefault(p => p.IsParams &&
                                                     p.Type is IArrayTypeSymbol
                                                     {
                                                        ElementType.SpecialType: SpecialType.System_Object
                                                     });

      // If the method doesn't have the required parameters, it's not the one we're looking for.
      if (descriptorParameter == null || paramsParameter == null)
         return;

      var args = invocation.ArgumentList.Arguments;
      if (args.Count <= descriptorParameter.Ordinal)
         return;

      var descriptorExpr = args[descriptorParameter.Ordinal].Expression;

      // Handle local variable descriptor
      var descriptorSymbol = context.SemanticModel.GetSymbolInfo(descriptorExpr).Symbol;
      if (descriptorSymbol == null)
         return;

      BaseObjectCreationExpressionSyntax? creation = null;

      var syntax = descriptorSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

      switch (syntax)
      {
         // Case 1: field declaration (static/instance field)
         case VariableDeclaratorSyntax variableDecl:
            if (variableDecl.Initializer?.Value is BaseObjectCreationExpressionSyntax boces)
               creation = boces;

            break;

         // Case 2: property declaration
         case PropertyDeclarationSyntax propertyDecl:
            // Initializer
            if (propertyDecl.Initializer?.Value is BaseObjectCreationExpressionSyntax boces2)
               creation = boces2;

            // Expression-bodied
            if (propertyDecl.ExpressionBody?.Expression is BaseObjectCreationExpressionSyntax boces3)
               creation = boces3;

            break;
      }

      if (creation == null)
         return;

      // Extract the message string and description string from the descriptor creation.
      if (creation.ArgumentList?.Arguments.Count >= 5)
      {
         var message = creation.ArgumentList.Arguments[4].ToString().Trim('"');
         var description = creation.ArgumentList.Arguments[5].ToString().Trim('"');

         var max = -1;
         MaxArgumentsInString(message, ref max);
         MaxArgumentsInString(description, ref max);

         var requiredArgs = max + 1;
         var providedArgs = 0;

         // Check if the 'params' argument was passed by name (e.g., args: ...).
         var namedParamsArgument = args.FirstOrDefault(a => a.NameColon?.Name.Identifier.Text == paramsParameter.Name);

         if (namedParamsArgument != null)
         {
            // Path A: The argument was named.

            providedArgs = namedParamsArgument.Expression switch
            {
               // Passed as an explicit array: args: new object[] { "a", "b" }
               ArrayCreationExpressionSyntax arrayCreation => arrayCreation.Initializer?.Expressions.Count ?? 0,
               // Passed as an implicit array: args: new[] { "a", "b" }
               ImplicitArrayCreationExpressionSyntax implicitArray => implicitArray.Initializer?.Expressions.Count ?? 0,
               // A collection expression's children are its elements.
               CollectionExpressionSyntax collectionExpression => collectionExpression.Elements.Count,
               _ => 1,
            };
         }
         else
         {
            // Path B: The arguments are passed positionally (the original logic).
            var paramsIndex = paramsParameter.Ordinal;
            if (args.Count > paramsIndex)
               providedArgs = args.Count - paramsIndex;
         }

         if (requiredArgs != providedArgs)
         {
            var expectedParamsString = GetExpectedParamsFromDocs(descriptorSymbol!, requiredArgs);
            context.ReportDiagnostic(Diagnostic.Create(Rule,
                                                       invocation.GetLocation(),
                                                       providedArgs,
                                                       requiredArgs,
                                                       expectedParamsString));
         }
      }
   }

   /// <summary>
   /// NEW HELPER: Parses the XML doc comments of a symbol to extract parameter descriptions.
   /// </summary>
   private string GetExpectedParamsFromDocs(ISymbol symbol, int requiredCount)
   {
      var xmlDocs = symbol.GetDocumentationCommentXml();
      if (string.IsNullOrEmpty(xmlDocs))
         return $"<{requiredCount} unnamed arguments>";

      try
      {
         var xml = XDocument.Parse(xmlDocs);
         var paramElements = xml.Root?.Elements("param")
                                .Where(p => int.TryParse(p.Attribute("name")?.Value, out _))
                                .OrderBy(p => int.Parse(p.Attribute("name")!.Value))
                                .Select(p => $"'{p.Value.Trim()}'")
                                .ToList();

         if (paramElements != null && paramElements.Any())
         {
            var joinedParams = string.Empty;
            for (var i = 0; i < paramElements.Count; i++)
               joinedParams += $"\n{i} : {paramElements[i]}";

            return joinedParams;
         }
      }
      catch
      {
         /* XML parsing can fail, fall back gracefully */
      }

      return $"<{requiredCount} arguments>";
   }

   private static void MaxArgumentsInString(string message, ref int max)
   {
      var matches = Regex.Matches(message, @"\{(\d+)\}");
      foreach (Match m in matches)
         if (int.TryParse(m.Groups[1].Value, out var n) && n > max)
            max = n;
   }
}