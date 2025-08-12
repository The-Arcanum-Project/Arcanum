using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// Represents a diagnostic that has been saved to the ErrorManager.
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
   public string Action { get; } = action;
   public DiagnosticDescriptor Descriptor { get; }= descriptor;
   public LocationContext Context { get; } = context;
   public string Code { get; }= descriptor.ToString();
   public string Description { get; } = description;
   public string Message { get; } = message;
   public DiagnosticSeverity Severity { get; }= severity;

   public Diagnostic(DiagnosticException diagnosticException, LocationContext context, string action)
      : this(diagnosticException.Descriptor,
             context,
             diagnosticException.Severity,
             action,
             diagnosticException.Message,
             diagnosticException.Description)
   {
   }

   // Example:  PA-002 Duplicate Province: Validating Province ID failed in File \"./wrong.txt\" at Line 10:4: The Province ID '10' is duplicate and was previously defined
   
   
   public override string ToString()
   {
      var actionString = string.IsNullOrWhiteSpace(Action) ? string.Empty : $" {Action} failed";
      return $"{Code}: {actionString}: {Context.ToErrorString} := {Message}";
   }
}