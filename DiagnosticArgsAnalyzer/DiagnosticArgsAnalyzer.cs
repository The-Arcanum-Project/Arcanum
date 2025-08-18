using System.Collections.Immutable;
using System.Linq;
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
                                                           "Descriptor expects {0} arguments but call provides {1}",
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
   private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
   {
      var invocation = (InvocationExpressionSyntax)context.Node;

      // Only analyze CreateAndHandle calls
      var symbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;
      if (symbol is not { Name: "CreateAndHandle" })
         return;

      var args = invocation.ArgumentList.Arguments;
      if (args.Count < 2)
         return;

      var descriptorExpr = args[1].Expression;

      ObjectCreationExpressionSyntax creation = null;

      // Handle local variable descriptor
      var descriptorSymbol = context.SemanticModel.GetSymbolInfo(descriptorExpr).Symbol;
      if (descriptorSymbol is ILocalSymbol local)
      {
         if (local.DeclaringSyntaxReferences.Length > 0)
         {
            var decl =
               local.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) as VariableDeclaratorSyntax;
            creation = (decl?.Initializer?.Value as ObjectCreationExpressionSyntax)!;
         }
      }
      // Handle property descriptor
      else if (descriptorSymbol is IPropertySymbol prop)
      {
         creation =
            ((prop.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken) as
                 PropertyDeclarationSyntax)
           ?.Initializer?.Value as ObjectCreationExpressionSyntax)!;
      }

      if (creation == null)
         return;

      // Extract the message string (5th argument)
      if (creation.ArgumentList?.Arguments.Count >= 5 &&
          creation.ArgumentList.Arguments[4].Expression is LiteralExpressionSyntax lit &&
          lit.IsKind(SyntaxKind.StringLiteralExpression))
      {
         var message = lit.Token.ValueText;

         var matches = Regex.Matches(message, @"\{(\d+)\}");
         int max = -1;
         foreach (Match m in matches)
         {
            if (int.TryParse(m.Groups[1].Value, out var n) && n > max)
               max = n;
         }

         var requiredArgs = max + 1;
         var providedArgs = args.Count - 3; // after context, descriptor, action

         if (requiredArgs != providedArgs)
         {
            context.ReportDiagnostic(Diagnostic.Create(Rule,
                                                       invocation.GetLocation(),
                                                       requiredArgs,
                                                       providedArgs));
         }
      }
   }
}