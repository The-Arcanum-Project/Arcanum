namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// How to handle a diagnostic after it has been reported to the user.
/// This is used to determine the next action after a diagnostic has been reported.
/// </summary>
public enum DiagnosticHandle
{
   /// <summary>
   /// Reload the file after the user has made changes to it.
   /// </summary>
   Retry,

   /// <summary>
   /// Ignore the error and continue parsing.
   /// </summary>
   Ignore,

   /// <summary>
   /// Close the application.
   /// </summary>
   Close,
}