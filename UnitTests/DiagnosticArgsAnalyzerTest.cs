using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Arcanum.Core;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace UnitTests;

[TestFixture]
[TestOf(typeof(DiagnosticArgsAnalyzer.DiagnosticArgsAnalyzer))]
public class DiagnosticArgsAnalyzerTest
{
   [Test]
   public async Task TooFewArguments_ShouldTriggerDiagnostic()
   {
      var testCode = @"
class C
{
    void M()
    {
        var descriptor = new Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticDescriptor(
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticCategory.Miscellaneous,
            1,
            ""DA001"",
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticSeverity.Error,
            ""Message {0} {1}"",
            ""Parsing some shit"",
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticReportSeverity.Silent);

        // Call CreateAndHandle with too few arguments
        CreateAndHandle(null, descriptor, () => {}, 42);
    }

    void CreateAndHandle(object context, 
                         Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticDescriptor descriptor,
                         System.Action action,
                         params object[] args) {}
}";

      var expected = new DiagnosticResult("DA001", DiagnosticSeverity.Error)
                    .WithSpan(16, 9, 16, 56)  // line/column of the CreateAndHandle call
                    .WithArguments("2", "1");

      var test = new CSharpAnalyzerTest<DiagnosticArgsAnalyzer.DiagnosticArgsAnalyzer, NUnitVerifier>
      {
         TestCode = testCode,
         ExpectedDiagnostics = { expected },
         ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
      };

      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticDescriptor).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticException).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(
                                                    Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticSeverity)
                                                .Assembly);
      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticReportSeverity).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(
                                                    Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticCategory)
                                                .Assembly);
      test.TestState.AdditionalReferences.Add(typeof(DiagnosticException).Assembly);

      await test.RunAsync();
   }

   [Test]
   public async Task CorrectNumberOfArguments_ShouldNotTriggerDiagnostic()
   {
      var testCode = @"
class C
{
    void M()
    {
        var descriptor = new Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticDescriptor(
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticCategory.Miscellaneous,
            1,
            ""DA001"",
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticSeverity.Error,
            ""Message {0} {1}"",
            ""Parsing some shit"",
            Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticReportSeverity.Silent);

        // Correct number of arguments
        var exception = new Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticException(descriptor, 42, 99);
    }
}";

      var test = new CSharpAnalyzerTest<DiagnosticArgsAnalyzer.DiagnosticArgsAnalyzer, NUnitVerifier>
      {
         TestCode = testCode, ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
      };

      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticDescriptor).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticException).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(
                                                    Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.DiagnosticSeverity)
                                                .Assembly);
      test.TestState.AdditionalReferences.Add(typeof(Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.
                                                 DiagnosticReportSeverity).Assembly);
      test.TestState.AdditionalReferences.Add(typeof(DiagnosticException).Assembly);

      await test.RunAsync(); // should pass with zero diagnostics
   }
}