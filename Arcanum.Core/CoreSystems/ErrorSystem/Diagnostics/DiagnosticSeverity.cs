namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;


/// <summary>
/// This enum defines the severity of a Diagnostic.
/// </summary>
public enum DiagnosticSeverity
{
   /// <summary>
   /// Indicates a error while parsing a file, which cannot easily be resolved.
   /// </summary>
   Error,
   /// <summary>
   /// Indicates a error while parsing a file, which can be resolved automatically.
   /// They can also be bad practices but practically do not break the file.
   /// This includes such things as duplicate items in a list.
   /// </summary>
   Warning,
   /// <summary>
   /// A simple information message that does not indicate an error or warning.
   /// This might be used for example to inform the user that a file was reloaded.
   /// </summary>
   Information
}

public static class DiagnosticSeveretyExtensions
{
   /// <summary>
   /// Retrieves the prefix associated with the specified <see cref="DiagnosticSeverity"/> value.
   /// The prefix has at least one and at maximum three-letters that represent the diagnosticException severity.
   /// </summary>
   /// <param name="severity">The diagnosticException severity for which the prefix is needed.</param>
   /// <returns>A string representing the prefix for the given diagnosticException severity.</returns>
   public static string GetPrefix(this DiagnosticSeverity severity)
   {
      return severity switch
      {
         DiagnosticSeverity.Error => "ERR",
         DiagnosticSeverity.Warning => "WRN",
         DiagnosticSeverity.Information => "INF",
         _ => throw new ArgumentOutOfRangeException(nameof(severity)),
      };
   }
}