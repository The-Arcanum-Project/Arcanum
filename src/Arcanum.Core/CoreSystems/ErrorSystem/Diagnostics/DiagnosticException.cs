using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem;
using Common.UI;
using Common.UI.MBox;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// Represents an instance of a diagnosticException which encapsulates a specific diagnosticException message, description, and unique code generated from its associated descriptor.
/// </summary>
/// <remarks>
/// This class is designed to wrap a <see cref="DiagnosticDescriptor"/>, providing a formatted message, description, and code derived from the descriptor.
/// It serves as a concrete diagnosticException entry that has been initialized with specific context, such as message arguments, making it usable for reporting and logging purposes.
/// </remarks>
public sealed class DiagnosticException : Exception
{
   public static DiagnosticException Fallback([CallerMemberName] string functionName = "",
                                              [CallerLineNumber] int lineNumber = 0,
                                              [CallerFilePath] string filePath = "")
      => new(ErrorDescriptors.Instance.Misc.UnknownError, functionName, lineNumber.ToString(), filePath);

   public readonly DiagnosticDescriptor Descriptor;
   public readonly object[] Arguments;

   public DiagnosticReportSeverity ReportSeverity;
   public DiagnosticSeverity Severity;

   public static void CreateAndHandle(LocationContext context,
                                      DiagnosticDescriptor descriptor,
                                      string action,
                                      DiagnosticSeverity? severity = null,
                                      DiagnosticReportSeverity? reportSeverity = null,
                                      params object[] args)
   {
      new DiagnosticException(descriptor, args).HandleDiagnostic(context, action, severity, reportSeverity);
   }

   /// <summary>
   /// Formats a diagnosticException message by replacing placeholders with the provided arguments, using an invariant culture. <br/>
   /// Any provided argument of type <see cref="IEnumerable"/> (excluding strings) will be converted to a comma-separated string representation. <br/>
   /// If no arguments are provided, the original message is returned unchanged.
   /// </summary>
   /// <param name="message">The message string containing placeholders to be replaced.</param>
   /// <param name="args">An array of arguments to replace the placeholders in the message string.</param>
   /// <returns>A formatted string with the placeholders replaced by the corresponding arguments or the original message if no arguments are provided.</returns>
   private static string FormatMessage(string message, params object[] args)
   {
      if (args.Length == 0)
         return message;

      var processedArgs = new object[args.Length];
      for (var i = 0; i < args.Length; i++)
      {
         var arg = args[i];
         if (arg is IEnumerable collection and not string)
            processedArgs[i] = string.Join(", ", collection.Cast<object>());
         else
            processedArgs[i] = arg;
      }

      return string.Format(CultureInfo.InvariantCulture, message, processedArgs);
   }

   // public DiagnosticException(DiagnosticException diagnosticException) : base(diagnosticException.Message)
   // {
   //    Descriptor = diagnosticException.Descriptor;
   //    Description = diagnosticException.Description;
   //    Code = diagnosticException.Code;
   // }

   public DiagnosticException(DiagnosticDescriptor descriptor, params object[] args) :
      base(FormatMessage(descriptor.Message, args))
   {
      Descriptor = descriptor;
      Description = FormatMessage(descriptor.Description, args);
      Code = Descriptor
        .ToString(); // For now take the Descriptor ToString since it is equal / $"{Descriptor.Category.GetPrefix()}-{Descriptor.Id:D4}";
      Arguments = args;
   }

   public void HandleDiagnostic(LocationContext context,
                                string action,
                                DiagnosticSeverity? severity = null,
                                DiagnosticReportSeverity? reportSeverity = null)
   {
      Severity = severity ?? Descriptor.Severity;
      ReportSeverity = reportSeverity ?? Descriptor.ReportSeverity;
      var ohNoWhatShouldWeDoNow = DiagnosticHandle.Ignore;
#if DEBUG
      if (!DebugConfig.Settings.SuppressAllErrors)
      {
         if (DebugConfig.Settings.OnlyHandleSpecifiedErrors)
            if (!DebugConfig.Settings.ErrorsToHandle.Any(action.Contains))
               return;
#endif
         if (!Config.Settings.ErrorLogOptions.SuppressAllErrors &&
             (Config.Settings.ErrorLogOptions.VanillaErrorsCausePopups || !LocationContext.IsVanillaContext(context)))
            switch (ReportSeverity)
            {
               case DiagnosticReportSeverity.Silent:
                  break;
               case DiagnosticReportSeverity.PopupNotify:
                  ohNoWhatShouldWeDoNow =
                     MBoxResultToDiagnosticHandle(UIHandle.Instance.PopUpHandle
                                                          .ShowMBox($"At ({context.LineNumber}:{context.ColumnNumber}) in File: {FileManager.SanitizePath(context.FilePath)}\n\n{ToString()}\n\n{Description}\n\nAction: {action}",
                                                                    "Error Encountered",
                                                                    icon: GetMessageBoxIconForSeverity(Severity)));
                  break;
               case DiagnosticReportSeverity.PopupWarning:
                  ohNoWhatShouldWeDoNow =
                     MBoxResultToDiagnosticHandle(UIHandle.Instance.PopUpHandle
                                                          .ShowMBox($"At ({context.LineNumber}:{context.ColumnNumber}) in File: {FileManager.SanitizePath(context.FilePath)}\n\n{ToString()}",
                                                                    "Error Encountered",
                                                                    MBoxButton.OKRetryCancel,
                                                                    GetMessageBoxIconForSeverity(Severity)));
                  break;
               case DiagnosticReportSeverity.PopupError:
                  ohNoWhatShouldWeDoNow =
                     MBoxResultToDiagnosticHandle(UIHandle.Instance.PopUpHandle
                                                          .ShowMBox($"At ({context.LineNumber}:{context.ColumnNumber}) in File: {FileManager.SanitizePath(context.FilePath)}\n\n{ToString()}",
                                                                    "Error Encountered",
                                                                    MBoxButton.RetryCancel,
                                                                    GetMessageBoxIconForSeverity(Severity)));
                  break;
               case DiagnosticReportSeverity.Suppressed:
                  // TODO @Minnator: Write to the Debug log of Arcanum
                  return;
               default:
                  throw new ArgumentOutOfRangeException(nameof(ReportSeverity), ReportSeverity, null);
            }

#if DEBUG
      }
#endif

      var diagnostic = new Diagnostic(this, context, action, Arguments);
      ErrorManager.AddToLog(diagnostic);

      if (ohNoWhatShouldWeDoNow == DiagnosticHandle.Retry)
         throw new ReloadFileException();

      if (ohNoWhatShouldWeDoNow == DiagnosticHandle.Close)
         throw new ReloadFileException(true);
   }

   private static MessageBoxImage GetMessageBoxIconForSeverity(DiagnosticSeverity severity)
   {
      return severity switch
      {
         DiagnosticSeverity.Error => MessageBoxImage.Error,
         DiagnosticSeverity.Warning => MessageBoxImage.Warning,
         DiagnosticSeverity.Information => MessageBoxImage.Information,
         _ => MessageBoxImage.Question,
      };
   }

   public static void LogWarning(ref ParsingContext pc,
                                 DiagnosticDescriptor descriptor,
                                 params object[] args) => LogWarning(pc.Context.GetInstance(), descriptor, pc.BuildStackTrace(), args);

   public static void LogWarning(LocationContext ctx,
                                 DiagnosticDescriptor descriptor,
                                 string action,
                                 params object[] args)
   {
      DiagnosticException diagnosticException = new(descriptor, args);
      diagnosticException.HandleDiagnostic(ctx, action);
   }

   private static DiagnosticHandle MBoxResultToDiagnosticHandle(MBoxResult result)
   {
      return result switch
      {
         MBoxResult.Retry => DiagnosticHandle.Retry,
         MBoxResult.Cancel => DiagnosticHandle.Close,
         MBoxResult.OK => DiagnosticHandle.Ignore,
         _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
      };
   }

   public readonly string Description;
   public readonly string Code;

   public override string ToString()
   {
      return $"{Code} ({Severity.GetPrefix()}): {Message}";
   }
}