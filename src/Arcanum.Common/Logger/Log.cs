using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Common.Logger;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LogLevel
{
   INF, // Information
   WRN, // Warning
   ERR, // Error
   DBG, // Debug
   CRT, // Critical
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum CommonLogSource
{
   PGM, // Plugin Manager
   PMS, // Parsing Master
   FSM, // File System Manager
   MPS, // Map Processing System
   SDK, // Software Development Kit
   SRG, // Source Generator
   NUI, // NUI
   AGP, // Generated Parsing system files
   AGS, // Automatic saving system
}

public static class ArcLog
{
   private static readonly DateTime StartTime = DateTime.Now;
   private static readonly BlockingCollection<string> _logQueue = new();

   // We want to log messages but in this format: [<Source>] [<Level>] <Message> 
   // The Source should be either a 3-letter or a word which will be converted to 3-letter
   // The Level should be one of the following: INF, WRN, ERR, DBG, CRT
   // The Message is the actual log message

   static ArcLog()
   {
      var loggingThread = new Thread(() =>
      {
         // This loop will run for the entire lifetime of the application.
         // GetConsumingEnumerable() will block until an item is available in the queue.
         // When .CompleteAdding() is called, the loop will finish.
         foreach (var message in _logQueue.GetConsumingEnumerable())
            Console.WriteLine(message);
      })
      {
         IsBackground = true, Name = "ArcLog Worker",
      };
      loggingThread.Start();
   }

   // TODO: Add to lifecycle shutdown
   public static void Shutdown()
   {
      _logQueue.CompleteAdding();
   }

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

   public static void WriteLine(CommonLogSource source, LogLevel level, string message)
      => Log($"[{GetSourceString(source.ToString())}] [{level.ToString()}] {message}");

   public static void WriteLine(CommonLogSource source, LogLevel level, string message, Exception ex)
      => Log($"[{GetSourceString(source.ToString())}] [{level.ToString()}] {message} | Exception: {ex}");

   private static void Log(string str)
   {
      _logQueue.Add($"{GetTimestamp()} - {str}");
   }

   private static string GetTimestamp()
   {
      return (DateTime.Now - StartTime).ToString(@"mm\:ss\.ff");
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