namespace Arcanum.API.Console;

public interface IOutputReceiver
{
   void WriteLine(string message, bool scrollToCaret = true, bool prefix = false);
   void WriteLines(List<string> messages);
   void WriteError(string message);
   void Clear();
}