namespace Arcanum.Core.CoreSystems.ErrorSystem.Exceptions;

public class ParsingCanceledException : Exception
{
   private readonly string _message;

   public ParsingCanceledException(string message)
   {
      _message = message;
   }

   public ParsingCanceledException(string message, Exception innerException)
   {
      _message = message + Environment.NewLine + innerException.Message;
   }

   public override string ToString() => _message + Environment.NewLine + StackTrace;
}