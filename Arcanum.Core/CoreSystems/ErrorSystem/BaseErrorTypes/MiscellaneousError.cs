using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

public class MiscellaneousError
{
   private static readonly Lazy<MiscellaneousError> LazyInstance = new(() => new());

   public static MiscellaneousError Instance => LazyInstance.Value;
   
   private MiscellaneousError()
   {
   }

   /// <param name="0">Function name</param>
   /// <param name="1">Line number</param>
   /// <param name="2">File name</param>
   public DiagnosticDescriptor UnknownError { get; } = new(DiagnosticCategory.Miscellaneous,
                                                                    0,
                                                                    "UnknownError",
                                                                    DiagnosticSeverity.Error,
                                                                    "Unknown error occurred.",
                                                                    "The internal Arcanum Error handling has just failed! An internal function did not return the respective object!\nin File: {2}\n atLine: {0}, in Function: {1}",
                                                                    DiagnosticReportSeverity.PopupError);
}