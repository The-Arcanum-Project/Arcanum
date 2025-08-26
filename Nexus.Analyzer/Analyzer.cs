namespace Nexus.Analyzer;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NexusAnalyzer : DiagnosticAnalyzer
{
   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
   [
      Diagnostics.SetValueTypeMismatch, Diagnostics.GetValueTypeMismatch, Diagnostics.EnumMismatch,
   ];

   public override void Initialize(AnalysisContext context)
   {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
   }
   private void AnalyzeInvocation(OperationAnalysisContext context)
   {
      if (!(context.Operation is IInvocationOperation invocation))
         return;

      var method = invocation.TargetMethod;

      // Path 1: Is this a "setter" method?
      var valueParamInfo = Helpers.FindParameterWithAttribute(method, "PropertyValueAttribute");
      if (valueParamInfo.parameter != null)
      {
         AnalyzeSetter(context, invocation, valueParamInfo.index);
         return;
      }

      // Path 2: Is this a "getter" method?
      var isGetter = method.GetAttributes().Any(a => a.AttributeClass?.Name == "PropertyGetterAttribute");
      if (!isGetter)
         return;

      AnalyzeGetter(context, invocation);
   }

   private void AnalyzeSetter(OperationAnalysisContext context,
                              IInvocationOperation invocation,
                              int valueParameterIndex)
   {
      if (!Helpers.TryGetTargetType(invocation, out var targetType, out var enumParamInfo))
         return;

      var enumArgument = invocation.Arguments[enumParamInfo.index];
      var valueArgument = invocation.Arguments[valueParameterIndex];

      //var (isEnumValid, enumMemberSymbol, requiredEnumSymbol) = Helpers.IsEnumValidForTarget(targetType, enumArgument);
      //if (enumMemberSymbol == null) return;

      if (!Helpers.IsEnumValidForTarget(targetType!,
                                        enumArgument,
                                        out var enumMemberSymbol,
                                        out var requiredEnumSymbol))
      {
         context.ReportDiagnostic(Diagnostic.Create(Diagnostics.EnumMismatch,
                                                    enumArgument.Syntax.GetLocation(),
                                                    enumMemberSymbol?.ToDisplayString() ?? "N/A",
                                                    requiredEnumSymbol?.ToDisplayString(SymbolDisplayFormat
                                                      .MinimallyQualifiedFormat) ??
                                                    "N/A",
                                                    targetType!.Name));
         return;
      }

      if (!Helpers.GetExpectedTypeFromEnumMember(enumMemberSymbol!, out var expectedValueType))
         return;

      var actualValueType = Helpers.GetActualTypeOfValue(valueArgument.Value);
      if (actualValueType == null || actualValueType.SpecialType == SpecialType.System_Object)
         return;

      var conversionInfo = context.Compilation.ClassifyConversion(actualValueType, expectedValueType!);
      if (conversionInfo is { IsIdentity: false, IsImplicit: false })
      {
         context.ReportDiagnostic(Diagnostic.Create(Diagnostics.SetValueTypeMismatch,
                                                    valueArgument.Syntax.GetLocation(),
                                                    enumMemberSymbol!.Name,
                                                    expectedValueType!.ToDisplayString(SymbolDisplayFormat
                                                      .MinimallyQualifiedFormat),
                                                    actualValueType.ToDisplayString(SymbolDisplayFormat
                                                      .MinimallyQualifiedFormat)));
      }
   }

   private void AnalyzeGetter(OperationAnalysisContext context, IInvocationOperation invocation)
   {
      var method = invocation.TargetMethod;

      // --- Path 1: It's a "Generic Getter" like T GetValue<T>(...) ---
      // We can identify this because its return type is a generic type parameter.
      if (method.OriginalDefinition.IsGenericMethod)
      {
         // For a generic getter, the type to check is the method's own instantiated return type.
         // For a call like DoSmthGetter<int>(...), invocation.Type is 'System.Int32'.
         var castToTypeSymbol = invocation.TargetMethod.TypeArguments[0];

         // The rest of the logic is the same: find target, check enum, then check type.
         if (!Helpers.TryGetTargetType(invocation, out var targetType, out var enumParamInfo))
            return;

         var enumArgument = invocation.Arguments[enumParamInfo.index];

         //var (isEnumValid, enumMemberSymbol, requiredEnumSymbol) = Helpers.IsEnumValidForTarget(targetType, enumArgument);
         //if (enumMemberSymbol == null) return;

         if (!Helpers.IsEnumValidForTarget(targetType!,
                                           enumArgument,
                                           out var enumMemberSymbol,
                                           out var requiredEnumSymbol))
         {
            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.EnumMismatch,
                                                       enumArgument.Syntax.GetLocation(),
                                                       enumMemberSymbol?.ToDisplayString() ?? "N/A",
                                                       requiredEnumSymbol?.ToDisplayString(SymbolDisplayFormat
                                                         .MinimallyQualifiedFormat) ??
                                                       "N/A",
                                                       targetType!.Name));
            return;
         }

         if (!Helpers.GetExpectedTypeFromEnumMember(enumMemberSymbol!, out var expectedValueType))
            return;

         if (!SymbolEqualityComparer.Default.Equals(expectedValueType, castToTypeSymbol))
         {
            var location = invocation.Syntax.GetLocation();

            // --- THIS IS THE FIX for finding the <T> location ---
            // The syntax for the method name in a generic call is often a GenericName.
            if (invocation.Syntax is InvocationExpressionSyntax
                {
                   Expression: MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName }
                })
            {
               // This gives us the location of the < ... > part.
               location = genericName.TypeArgumentList.GetLocation();
            }
            else
            {
               var parameter = invocation.TargetMethod.OriginalDefinition.Parameters;
               var typeArgument = invocation.TargetMethod.OriginalDefinition.TypeArguments[0];
               for (var index = 0; index < parameter.Length; index++)
               {
                  var par = parameter[index];

                  if (SymbolEqualityComparer.Default.Equals(typeArgument, par.Type))
                     location = invocation.Arguments[index].Syntax.GetLocation();
               }
            }

            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GetValueTypeMismatch,
                                                       location,
                                                       enumMemberSymbol!.Name,
                                                       expectedValueType!.ToDisplayString(SymbolDisplayFormat
                                                         .MinimallyQualifiedFormat),
                                                       castToTypeSymbol.ToDisplayString(SymbolDisplayFormat
                                                         .MinimallyQualifiedFormat)));
         }
      }
   }
}