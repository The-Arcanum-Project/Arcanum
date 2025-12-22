namespace Arcanum.Core.CoreSystems.ErrorSystem.Exceptions;

public class ParsingCanceledException(string message) : Exception
{
   public override string ToString() => message + Environment.NewLine + StackTrace;
}