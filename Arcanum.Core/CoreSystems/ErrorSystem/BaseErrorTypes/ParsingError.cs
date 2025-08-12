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
   /// <param name="0">Key-Value pair</param>
   public DiagnosticDescriptor InvalidKeyValuePair { get; } = new(DiagnosticCategory.Parsing,
                                                                  3,
                                                                  "Invalid Key-Value Pair",
                                                                  DiagnosticSeverity.Error,
                                                                  "The key-value pair '{0}' is invalid.",
                                                                  "This error indicates that the key-value pair provided in the parsing step is not valid or does not conform to the expected format.",
                                                                  DiagnosticReportSeverity.Silent);

    /// <param name="0">The hex value which could not be converted to an int</param>
   public DiagnosticDescriptor HexToIntConversionError { get; } = new(DiagnosticCategory.Parsing,
                                                                      4,
                                                                      "Hex to Int Conversion Error",
                                                                      DiagnosticSeverity.Error,
                                                                      "Failed to convert hex value '{0}' to an integer.",
                                                                      "The given hex value could not be converted to an integer. Please ensure it is a valid hexadecimal number.",
                                                                      DiagnosticReportSeverity.PopupNotify);
    
    public DiagnosticDescriptor DuplicateLocationDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                      5,
                                                                      "Duplicate Location Definition",
                                                                      DiagnosticSeverity.Error,
                                                                      "Duplicate location definition found for '{0}'.",
                                                                      "The given location name has been used multiple times which is not allowed. They need to be unique.",
                                                                      DiagnosticReportSeverity.PopupNotify);
}