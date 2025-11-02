using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace UnitTests.CoreSystems.ErrorHandling;

[TestFixture]
public class ErrorSystemTestsDiagnostics
{
   [Test]
   public void Diagnostic_MessageAndDescription_ShouldBeFormatted()
   {
      var descriptor = new DiagnosticDescriptor(DiagnosticCategory.Rendering,
                                                1,
                                                "Test Name",
                                                DiagnosticSeverity.Error,
                                                "Test message with {0} and {1}",
                                                "Test description with {0} and {1}",
                                                DiagnosticReportSeverity.PopupError);

      var diagnostic = new DiagnosticException(descriptor, "File.cs", 42);

      Assert.That(diagnostic.Message, Is.EqualTo("Error in File.cs at 42"));
      Assert.That(diagnostic.Description, Is.EqualTo("Detailed error in File.cs at 42"));
      Assert.That(diagnostic.Code, Is.EqualTo("LOD-0001")); // Assuming GetPrefix returns "GEN"
   }

   [Test]
   public void DiagnosticDescriptor_ToString_ShouldFormatCorrectly()
   {
      var descriptor = new DiagnosticDescriptor(DiagnosticCategory.Rendering,
                                                42,
                                                "Test Name",
                                                DiagnosticSeverity.Error,
                                                "Test message with {0} and {1}",
                                                "Test description with {0} and {1}",
                                                DiagnosticReportSeverity.PopupError);

      Assert.That(descriptor.ToString(), Is.EqualTo("PAR-0042")); // Assuming GetPrefix returns "NET"
   }

   [Test]
   public void DiagnosticDescriptor_EqualityAndHashCode_WorkCorrectly()
   {
      var d2 = new DiagnosticDescriptor(DiagnosticCategory.Rendering,
                                        1,
                                        "Test Name",
                                        DiagnosticSeverity.Error,
                                        "Test message with {0} and {1}",
                                        "Test description with {0} and {1}",
                                        DiagnosticReportSeverity.PopupError);

      var d1 = new DiagnosticDescriptor(DiagnosticCategory.Rendering,
                                        1,
                                        "Test Name",
                                        DiagnosticSeverity.Error,
                                        "Test message with {0} and {1}",
                                        "Test description with {0} and {1}",
                                        DiagnosticReportSeverity.PopupError);

      Assert.That(d2, Is.EqualTo(d1));
      Assert.That(d2.GetHashCode(), Is.EqualTo(d1.GetHashCode()));
   }

   [Test]
   public void DiagnosticDescriptor_IsEnabled_BehavesCorrectly()
   {
      var descriptor = new DiagnosticDescriptor(DiagnosticCategory.Rendering,
                                                1,
                                                "Test Name",
                                                DiagnosticSeverity.Error,
                                                "Test message with {0} and {1}",
                                                "Test description with {0} and {1}",
                                                DiagnosticReportSeverity.Silent);
      Assert.That(descriptor.IsEnabled);

      descriptor.ReportSeverity = DiagnosticReportSeverity.Suppressed;
      Assert.That(descriptor.IsEnabled, Is.False);
   }
}