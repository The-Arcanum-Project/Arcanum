using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

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
      => new(MiscellaneousError.UnknownError, functionName, lineNumber.ToString(), filePath);

   public readonly DiagnosticDescriptor Descriptor;

   public DiagnosticReportSeverity ReportSeverity;
   public DiagnosticSeverity Severity;

   /// <summary>
   /// Formats a diagnosticException message by replacing placeholders with the provided arguments, using an invariant culture.
   /// </summary>
   /// <param name="message">The message string containing placeholders to be replaced.</param>
   /// <param name="args">An array of arguments to replace the placeholders in the message string.</param>
   /// <returns>A formatted string with the placeholders replaced by the corresponding arguments or the original message if no arguments are provided.</returns>
   private static string FormatMessage(string message, params object[] args)
   {
      return args.Length == 0 ? message : string.Format(CultureInfo.InvariantCulture, message, args);
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
   }

   public void HandleDiagnostic(LocationContext context,
                                string action = "",
                                DiagnosticSeverity? severity = null,
                                DiagnosticReportSeverity? reportSeverity = null)
   {
      Severity = severity ?? Descriptor.Severity;
      ReportSeverity = reportSeverity ?? Descriptor.ReportSeverity;
      var ohNoWhatShouldWeDoNow = DiagnosticHandle.Ignore;
      switch (ReportSeverity)
      {
         case DiagnosticReportSeverity.Silent:
            break;
         case DiagnosticReportSeverity.PopupNotify:
            Debug.WriteLine("Notification: " + this);
            break;
         case DiagnosticReportSeverity.PopupWarning:
            Debug.WriteLine("Warning: " + this);
            ohNoWhatShouldWeDoNow = DiagnosticHandle.Close;
            break;
         case DiagnosticReportSeverity.PopupError:
            Debug.WriteLine("Error: " + this);
            ohNoWhatShouldWeDoNow = DiagnosticHandle.Retry;
            break;
         case DiagnosticReportSeverity.Suppressed:
            // TODO @Minnator: Write to the Debug log of Arcanum
            return;
         default:
            throw new ArgumentOutOfRangeException(nameof(ReportSeverity), ReportSeverity, null);
      }
      // TODO @Minnator: Call the close handler if close is selected create fancy popup for other options

      var diagnostic = new Diagnostic(this, context, action);
      ErrorManager.Diagnostics.Add(diagnostic);

      if (ohNoWhatShouldWeDoNow == DiagnosticHandle.Retry)
         throw new ReloadFileException();
   }

   public readonly string Description;
   public readonly string Code;

   public override string ToString()
   {
      return $"{Code} ({Severity.GetPrefix()}): {Message}";
   }
}