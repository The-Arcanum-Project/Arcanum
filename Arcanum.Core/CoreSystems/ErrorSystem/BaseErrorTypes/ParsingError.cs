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
}