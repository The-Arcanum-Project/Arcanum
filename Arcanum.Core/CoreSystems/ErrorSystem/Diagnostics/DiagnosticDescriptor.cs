using System.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// Represents a descriptor for a diagnosticException, which provides identifying information, category, severity, message format, and description of a diagnosticException entry.
/// </summary>
/// <remarks>
/// This class defines metadata about a diagnosticException, including its unique ID, category, severity, description, and how it should be reported.
/// It also includes behavior to manage the diagnosticException's reporting state and severity.
/// The message as well as the description can be formatted with arguments, allowing for dynamic content in diagnostics.
/// </remarks>
/// <param name="id">A unique identifier for the diagnosticException descriptor.</param>
/// <param name="category">The category to which the diagnosticException belongs, typically associated with a functional area.</param>
/// <param name="severity">The default diagnosticException severity, indicating the level of impact of the diagnosticException.</param>
/// <param name="message">The message template associated with the diagnosticException. This provides information about the diagnosticException instance.</param>
/// <param name="description">An optional description template that provides additional details about the diagnosticException instance.</param>
/// <param name="reportSeverity">Defines how the diagnosticException should be displayed or reported in the system.</param>
[DebuggerDisplay("{Category}-{Id:D4} {Message}")]
public class DiagnosticDescriptor(
   int id,
   DiagnosticCategory category,
   DiagnosticSeverity severity,
   string message,
   string description,
   DiagnosticReportSeverity reportSeverity)
{
   public void ResetToDefault()
   {
      ReportSeverity = _reportSeverity;
      Severity = _severity;
   }

   public bool IsEnabled => ReportSeverity != DiagnosticReportSeverity.Suppressed;

   public readonly DiagnosticCategory Category = category;
   private readonly DiagnosticReportSeverity _reportSeverity = reportSeverity;
   public DiagnosticReportSeverity ReportSeverity = reportSeverity;
   private readonly DiagnosticSeverity _severity = severity;
   public DiagnosticSeverity Severity = severity;
   public readonly int Id = id;
   public readonly string Message = message;
   public readonly string Description = description;

   public override string ToString() => $"{Category.GetPrefix()}-{Id:D4}";

   public override int GetHashCode() => HashCode.Combine(Category, Id);

   public override bool Equals(object? obj)
   {
      if (obj is not DiagnosticDescriptor other)
         return false;

      return Category == other.Category && Id == other.Id;
   }
}