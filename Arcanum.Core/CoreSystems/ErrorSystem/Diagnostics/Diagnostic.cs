using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// Is being displayed in the ErrorLog
/// </summary>
/// <param name="descriptor"></param>
/// <param name="context"></param>
/// <param name="severity"></param>
/// <param name="action"></param>
/// <param name="message"></param>
/// <param name="description"></param>
public sealed class Diagnostic(DiagnosticDescriptor descriptor,
                               LocationContext context,
                               DiagnosticSeverity severity,
                               string action,
                               string message,
                               string description)
{
   private readonly DiagnosticDescriptor _descriptor = descriptor;
   public readonly LocationContext Context = context;
   private readonly string _code = descriptor.ToString();
   private readonly string _description = description;
   public DiagnosticSeverity Severity = severity;

   public Diagnostic(DiagnosticException diagnosticException, LocationContext context, string action)
      : this(diagnosticException.Descriptor,
             context,
             diagnosticException.Severity,
             action,
             diagnosticException.Message,
             diagnosticException.Description)
   {
   }

   // Example:  PA-002 Validating Province ID failed in File \"./wrong.txt\" at Line 10:4: The Province ID '10' is duplicate and was previously defined

   
   public override string ToString()
   {
      var actionString = string.IsNullOrWhiteSpace(action) ? string.Empty : $" {action} failed";
      return $"{_code}{actionString} {Context.ToErrorString()} := {message}";
   }
}