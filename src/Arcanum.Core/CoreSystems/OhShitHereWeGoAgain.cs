namespace Arcanum.Core.CoreSystems;

public class OhShitHereWeGoAgainException : Exception
{
   public OhShitHereWeGoAgainException(string message)
      : base(message)
   {
   }

   public OhShitHereWeGoAgainException(string message, Exception innerException)
      : base(message, innerException)
   {
   }
}