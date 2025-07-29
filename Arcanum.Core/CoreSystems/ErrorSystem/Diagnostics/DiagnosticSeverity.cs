namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;


public enum DiagnosticSeverity
{
   Error,
   Warning,
   Information,
   Debug,
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
         DiagnosticSeverity.Debug => "DBG",
         _ => throw new ArgumentOutOfRangeException(nameof(severity)),
      };
   }
}