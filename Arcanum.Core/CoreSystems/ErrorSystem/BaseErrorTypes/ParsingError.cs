using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

public static class ErrorProvider
{
   /// <param name="0">Wrong type</param>
   /// <param name="1">To type</param>
   public static readonly DiagnosticDescriptor ConversionED = new(1,
                                                                  DiagnosticCategory.Parsing,
                                                                  DiagnosticSeverity.Error,
                                                                  "Cannot convert the value of {0} to {1}",
                                                                  "This error indicates that the parser could not convert the value of {0} to the expected type {1}.",
                                                                  DiagnosticReportSeverity.PopupNotify);
}