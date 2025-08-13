using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

public class MiscellaneousError : ILazySingleton
{
    private static readonly Lazy<MiscellaneousError> LazyInstance = new(() => new MiscellaneousError());
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
   public DiagnosticDescriptor DebugError1 { get; } = new(DiagnosticCategory.Miscellaneous,
                                                          1,
                                                          "DebugError1",
                                                          DiagnosticSeverity.Error,
                                                          "Debug Error 1",
                                                          "This is a debug error for testing purposes.",
                                                          DiagnosticReportSeverity.PopupError);
   public DiagnosticDescriptor DebugError2 { get; } = new(DiagnosticCategory.Miscellaneous,
                                                          2,
                                                          "DebugError2",
                                                          DiagnosticSeverity.Error,
                                                          "Debug Error 2",
                                                          "This is another debug error for testing purposes.",
                                                          DiagnosticReportSeverity.PopupError);
}