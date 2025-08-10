using Arcanum.API.Attributes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;


public class ParsingError
{
   private static readonly Lazy<ParsingError> LazyInstance = new(() => new());

   public static ParsingError Instance => LazyInstance.Value;
   
   private ParsingError()
   {
   }

   /// <param name="0">Wrong type</param>
   /// <param name="1">To type</param>
   public DiagnosticDescriptor ConversionError { get; } = new(DiagnosticCategory.Parsing,
                                                                       1,
                                                                       "Conversion Error",
                                                                       DiagnosticSeverity.Error,
                                                                       "Cannot convert the value of {0} to {1}",
                                                                       "This error indicates that the parser could not convert the value of {0} to the expected type {1}.",
                                                                       DiagnosticReportSeverity.PopupNotify);
   
    /// <param name="0">Parsing step name</param>
    /// <param name="1">Error message or inner exception</param>
   public DiagnosticDescriptor ParsingBaseStepFailure { get; } = new(DiagnosticCategory.Parsing,
                                                                       2,
                                                                       "Parsing Step Failure",
                                                                       DiagnosticSeverity.Error,
                                                                       "Parsing step {0} failed with error: {1}",
                                                                       "This error indicates that a parsing step failed with more than a simple error during execution.",
                                                                       DiagnosticReportSeverity.PopupNotify);
   
}