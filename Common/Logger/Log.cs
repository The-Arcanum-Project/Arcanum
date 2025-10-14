namespace Common.Logger;

public enum LogLevel
{
   INF, // Information
   WRN, // Warning
   ERR, // Error
   DBG, // Debug
   CRT // Critical
}

public static class ArcLog
{
   // We want to log messages but in this format: [<Source>] [<Level>] <Message> 
   // The Source should be either a 3-letter or a word which will be converted to 3-letter
   // The Level should be one of the following: INF, WRN, ERR, DBG, CRT
   // The Message is the actual log message

   public static void Write(string source, LogLevel level, string message)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {message}");

   public static void Write(string source, LogLevel level, string message, Exception ex)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {message} | Exception: {ex}");

   public static void Write(string source, LogLevel level, string message, params object[] args)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {string.Format(message, args)}");

   public static void WriteLine(string source, LogLevel level, string message)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {message}");

   public static void WriteLine(string source, LogLevel level, string message, Exception ex)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {message} | Exception: {ex}");

   public static void WriteLine(string source, LogLevel level, string message, params object[] args)
      => Log($"[{GetSourceString(source)}] [{level.ToString()}] {string.Format(message, args)}");

   private static void Log(string str)
   {
      Console.WriteLine(str);
   }

   private static string GetSourceString(string source)
   {
      // if it is 3 chars we just return it uppercased
      // else we take the first 3 uppercased chars
      // if it has less than 3 uppercased chars we take all uppercased and concat 1-2 chars after the first uppercased char

      switch (source.Length)
      {
         case 3:
            return source.ToUpper();
         case < 3:
            return source.ToUpper().PadRight(3);
      }

      var result = "";
      var upperCount = 0;
      foreach (var c in source.Where(char.IsUpper))
      {
         result += c;
         upperCount++;
         if (upperCount == 3)
            break;
      }

      if (upperCount >= 3)
         return result.PadRight(3).ToUpper();

      foreach (var c in source.Where(c => !char.IsUpper(c)))
      {
         result += c;
         upperCount++;
         if (upperCount == 3)
            break;
      }

      return result.PadRight(3).ToUpper();
   }
}