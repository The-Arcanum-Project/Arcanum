using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
   FUM, // File Update Manager
   PRT, // Programm Root
   PMT, // ParsingMasTer
}

public static class ArcLog
{
   private static readonly DateTime StartTime = DateTime.Now;

   private static readonly BlockingCollection<LogEntry> LogQueue = new();
   private static readonly ConcurrentDictionary<string, string> SourceCache = new();

   static ArcLog()
   {
      var loggingThread = new Thread(ProcessLogQueue)
      {
         IsBackground = true,
         Name = "ArcLog Worker",
         Priority = ThreadPriority.BelowNormal,
      };
      loggingThread.Start();
   }

   public static LogLevel LogLevel { get; set; } = LogLevel.INF;

   public static bool IsLevelEnabled(LogLevel level) => level >= LogLevel;

   public static void Shutdown()
   {
      LogQueue.CompleteAdding();
   }

   public static void Error(string source, string message) => Enqueue(source, LogLevel.ERR, message);

   public static void Error(string source, string message, Exception ex) => Enqueue(source, LogLevel.ERR, message, ex);

   public static void Warning(string source, string message) => Enqueue(source, LogLevel.WRN, message);

   public static void Warning(string source, string message, Exception ex) => Enqueue(source, LogLevel.WRN, message, ex);

   public static void Write(string source, LogLevel level, string message) => Enqueue(source, level, message);

   public static void Write(string source, LogLevel level, string message, Exception ex) => Enqueue(source, level, message, ex);

   public static void Write(string source, LogLevel level, string message, params object[] args)
   {
      if (IsLevelEnabled(level))
         Enqueue(source, level, message, null, args);
   }

   public static void WriteLine(string source, LogLevel level, string message) => Enqueue(source, level, message);

   public static void WriteLine(string source, LogLevel level, string message, Exception ex) => Enqueue(source, level, message, ex);

   public static void WriteLine(string source, LogLevel level, string message, params object[] args)
   {
      if (IsLevelEnabled(level))
         Enqueue(source, level, message, null, args);
   }

   public static void WriteLine(CommonLogSource source, LogLevel level, string message) => Enqueue(source.ToString(), level, message);

   public static void WriteLine(CommonLogSource source, LogLevel level, string message, Exception ex) => Enqueue(source.ToString(), level, message, ex);

   public static void WritePure(string message)
   {
      LogQueue.Add(new(string.Empty, LogLevel.INF, message, null, null, DateTime.Now));
   }

   private static void Enqueue(string source, LogLevel level, string message, Exception? ex = null, object[]? args = null)
   {
      if (!IsLevelEnabled(level))
         return;

      LogQueue.Add(new(source, level, message, ex, args, DateTime.Now));
   }

   private static void ProcessLogQueue()
   {
      var batchBuffer = new StringBuilder(4096);

      foreach (var entry in LogQueue.GetConsumingEnumerable())
         try
         {
            FormatEntryToBuffer(batchBuffer, entry);

            while (LogQueue.TryTake(out var nextEntry))
            {
               FormatEntryToBuffer(batchBuffer, nextEntry);

               if (batchBuffer.Length > 8000)
                  break;
            }

            Console.Write(batchBuffer.ToString());
            batchBuffer.Clear();
         }
         catch
         {
            batchBuffer.Clear();
         }
   }

   private static void FormatEntryToBuffer(StringBuilder sb, LogEntry entry)
   {
      if (entry.SourceRaw == string.Empty && entry.Timestamp != default)
      {
         sb.AppendLine(entry.Message);
         return;
      }

      var timeSpan = entry.Timestamp - StartTime;

      sb.Append(timeSpan.ToString(@"mm\:ss\.ff"));
      sb.Append(" - ");

      var processedSource = SourceCache.GetOrAdd(entry.SourceRaw, GenerateSourceString);
      sb.Append('[').Append(processedSource).Append("] ");

      sb.Append('[').Append(entry.Level).Append("] ");

      if (entry.Args != null && entry.Args.Length > 0)
         sb.AppendFormat(entry.Message, entry.Args);
      else
         sb.Append(entry.Message);

      if (entry.Exception != null)

         sb.Append(" | Exception: ").Append(entry.Exception);

      sb.AppendLine();
   }

   private static string GenerateSourceString(string source)
   {
      if (string.IsNullOrEmpty(source))
         return "UNK";

      if (source.Length == 3)
         return source.ToUpper();

      if (source.Length < 3)
         return source.ToUpper().PadRight(3);

      var sb = new StringBuilder();
      var upperCount = 0;

      foreach (var c in source)
      {
         if (!char.IsUpper(c))
            continue;

         sb.Append(c);
         upperCount++;
         if (upperCount == 3)
            break;
      }

      if (upperCount >= 3)
         return sb.ToString().PadRight(3).ToUpper();

      foreach (var c in source)
      {
         if (char.IsUpper(c))
            continue;

         sb.Append(c);
         upperCount++;
         if (upperCount == 3)
            break;
      }

      return sb.ToString().PadRight(3).ToUpper();
   }

   private readonly record struct LogEntry(
      string SourceRaw,
      LogLevel Level,
      string Message,
      Exception? Exception,
      object[]? Args,
      DateTime Timestamp);
}