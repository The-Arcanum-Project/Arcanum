namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// An exception that is thrown when a file needs to be reloaded.
/// Is just a way to escalate the reload instruction from the stack trace to the original file parsing method.
/// In theory this exception should always be caught by the file parsing method and handled accordingly.
/// </summary>
public class ReloadFileException
   : Exception;