using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace UnitTests.CoreSystems.ErrorHandling;

[TestFixture]
public class ErrorSystemTestsDiagnostics
{
   [Test]
        public void Diagnostic_MessageAndDescription_ShouldBeFormatted()
        {
            var descriptor = new DiagnosticDescriptor(
                id: 1,
                category: DiagnosticCategory.Loading,
                severity: DiagnosticSeverity.Warning,
                message: "Error in {0} at {1}",
                description: "Detailed error in {0} at {1}",
                reportSeverity: DiagnosticReportSeverity.Suppressed);

            var diagnostic = new DiagnosticException(descriptor, "File.cs", 42);

            Assert.That(diagnostic.Message, Is.EqualTo("Error in File.cs at 42"));
            Assert.That(diagnostic.Description, Is.EqualTo("Detailed error in File.cs at 42"));
            Assert.That(diagnostic.Code, Is.EqualTo("LOD-0001")); // Assuming GetPrefix returns "GEN"
        }

        [Test]
        public void DiagnosticDescriptor_ToString_ShouldFormatCorrectly()
        {
            var descriptor = new DiagnosticDescriptor(
                id: 42,
                category: DiagnosticCategory.Parsing,
                severity: DiagnosticSeverity.Error,
                message: "Failure",
                description: "Network failure",
                reportSeverity: DiagnosticReportSeverity.Silent);

            Assert.That(descriptor.ToString(), Is.EqualTo("PAR-0042")); // Assuming GetPrefix returns "NET"
        }

        [Test]
        public void DiagnosticDescriptor_EqualityAndHashCode_WorkCorrectly()
        {
            var d1 = new DiagnosticDescriptor(5, DiagnosticCategory.Loading, DiagnosticSeverity.Information, "msg", "desc", DiagnosticReportSeverity.Silent);
            var d2 = new DiagnosticDescriptor(5, DiagnosticCategory.Loading, DiagnosticSeverity.Warning, "other", "otherDesc", DiagnosticReportSeverity.Suppressed);

            Assert.That(d2, Is.EqualTo(d1));
            Assert.That(d2.GetHashCode(), Is.EqualTo(d1.GetHashCode()));
        }

        [Test]
        public void DiagnosticDescriptor_ResetToDefault_Works()
        {
            var descriptor = new DiagnosticDescriptor(
                id: 7,
                category: DiagnosticCategory.Loading,
                severity: DiagnosticSeverity.Error,
                message: "DB issue",
                description: "Details",
                reportSeverity: DiagnosticReportSeverity.Suppressed)
            {
                ReportSeverity = DiagnosticReportSeverity.PopupError, Severity = DiagnosticSeverity.Warning,
            };

            descriptor.ResetToDefault();

            Assert.That(descriptor.ReportSeverity, Is.EqualTo(DiagnosticReportSeverity.Suppressed));
            Assert.That(descriptor.Severity, Is.EqualTo(DiagnosticSeverity.Error));
        }

        [Test]
        public void DiagnosticDescriptor_IsEnabled_BehavesCorrectly()
        {
            var descriptor = new DiagnosticDescriptor(10, DiagnosticCategory.Loading, DiagnosticSeverity.Warning, "msg", "desc", DiagnosticReportSeverity.Silent);
            Assert.That(descriptor.IsEnabled);

            descriptor.ReportSeverity = DiagnosticReportSeverity.Suppressed;
            Assert.That(descriptor.IsEnabled, Is.False);
        }
}