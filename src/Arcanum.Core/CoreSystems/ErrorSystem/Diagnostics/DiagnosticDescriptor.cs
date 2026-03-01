using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Arcanum.API.Attributes;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// Represents the metadata for a diagnosticException that can be reported in the system.
/// </summary>
/// <remarks>
/// This class defines metadata about a diagnosticException, including its category, severity, message, and reporting behavior.
/// It is used to create instances of diagnosticException that can be reported to the user or logged.
/// The message as well as the description can be formatted with arguments, allowing for dynamic content in diagnostics.
/// </remarks>
/// <param name="category">The category to which the diagnosticException belongs, typically associated with a functional area.</param>
/// <param name="id">A unique identifier for the diagnosticException descriptor.</param>
/// <param name="name"> A name for the diagnosticException descriptor, which is used to identify the diagnosticException in logs or reports. </param>
/// <param name="severity">The default diagnosticException severity, indicating the level of impact of the diagnosticException.</param>
/// <param name="message">The message template associated with the diagnosticException. This provides information about the diagnosticException instance.</param>
/// <param name="description">An optional description template that provides additional details about the diagnosticException instance.</param>
/// <param name="reportSeverity">Defines how the diagnosticException should be displayed or reported in the system.</param>
/// <param name="resolution">An optional resolution string that provides guidance on how to resolve the diagnosticException.</param>
[DebuggerDisplay("{Category}-{Id:D4} {Message}")]
public class DiagnosticDescriptor(DiagnosticCategory category,
                                  int id,
                                  string name,
                                  DiagnosticSeverity severity,
                                  string message,
                                  string description,
                                  DiagnosticReportSeverity reportSeverity,
                                  Func<object[], string>? resolution = null)
{
   public object ResetSettings(PropertyInfo propertyInfo)
   {
      return propertyInfo.Name switch
      {
         nameof(ReportSeverity) => _reportSeverity,
         nameof(Severity) => _severity,
         _ => throw new InvalidEnumArgumentException($"Invalid property name: {propertyInfo.Name}")
      };
   }

   [IgnoreInPropertyGrid]
   public bool IsEnabled => ReportSeverity != DiagnosticReportSeverity.Suppressed;

   public readonly DiagnosticCategory Category = category;
   private readonly DiagnosticReportSeverity _reportSeverity = reportSeverity;
   [CustomResetMethod(nameof(ResetSettings))]
   [Description("How a diagnostic should be reported in the system. This can be either Silent, or include various levels of user interaction.")]
   public DiagnosticReportSeverity ReportSeverity { get; set; } = reportSeverity;
   private readonly DiagnosticSeverity _severity = severity;
   [CustomResetMethod(nameof(ResetSettings))]
   [Description("The severity of the Diagnostic.")]
   public DiagnosticSeverity Severity { get; set; } = severity;
   public readonly int Id = id;
   public string Name { get; } = name;
   public string Message { get; } = message;
   public string Description { get; } = description;
   public Func<object[], string>? Resolution { get; } = resolution;

   public override string ToString() => $"{Category.GetPrefix()}-{Id:D4} {Name}";

   public override int GetHashCode() => HashCode.Combine(Category, Id);

   public override bool Equals(object? obj)
   {
      if (obj is not DiagnosticDescriptor other)
         return false;

      return Category == other.Category && Id == other.Id;
   }
}