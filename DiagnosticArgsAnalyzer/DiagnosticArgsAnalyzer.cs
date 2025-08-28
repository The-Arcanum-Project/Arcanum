using System.Collections.Immutable;
using System.Text.RegularExpressions;
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
                                                           "Descriptor message format expects '{0}' arguments but call provides '{1}'",
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

      var descriptorExpr = args[1].Expression;

      BaseObjectCreationExpressionSyntax? creation = null;

      // Handle local variable descriptor
      var descriptorSymbol = context.SemanticModel.GetSymbolInfo(descriptorExpr).Symbol;

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

      // Extract the message string (5th argument)
      if (creation.ArgumentList?.Arguments.Count >= 5)
      {
         var message = creation.ArgumentList.Arguments[4].ToString().Trim('"');
         var description = creation.ArgumentList.Arguments[5].ToString().Trim('"');

         var max = -1;
         MaxArgumentsInString(message, ref max);
         MaxArgumentsInString(description, ref max);

         var requiredArgs = max + 1;
         var providedArgs = 0;
         var paramsIndex = paramsParameter.Ordinal;
         if (args.Count > paramsIndex)
            providedArgs = args.Count - paramsIndex;

         if (requiredArgs != providedArgs)
            context.ReportDiagnostic(Diagnostic.Create(Rule,
                                                       invocation.GetLocation(),
                                                       requiredArgs,
                                                       providedArgs));
      }
   }

   private static void MaxArgumentsInString(string message, ref int max)
   {
      var matches = Regex.Matches(message, @"\{(\d+)\}");
      foreach (Match m in matches)
         if (int.TryParse(m.Groups[1].Value, out var n) && n > max)
            max = n;
   }
}