using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

public class IOError
{
   private static readonly Lazy<IOError> LazyInstance = new(() => new());

   public static IOError Instance => LazyInstance.Value;

   private IOError()
   {
   }

   /// <param name="0">File Path</param>
   public DiagnosticDescriptor FileReadingError { get; } = new(DiagnosticCategory.Miscellaneous,
                                                               0,
                                                               "FileReadingError",
                                                               DiagnosticSeverity.Error,
                                                               "The given file does not exist or could not be read:\n {0}",
                                                               "Tried to read the file '{0}' but it does not exist or could not be read.\nPlease check the file path and permissions.\n",
                                                               DiagnosticReportSeverity.PopupError);
   
   
}