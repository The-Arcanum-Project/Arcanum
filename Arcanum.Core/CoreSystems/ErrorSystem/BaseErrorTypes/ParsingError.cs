using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;

public class ParsingError : ILazySingleton
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

   public DiagnosticDescriptor ForbiddenElement { get; } = new(DiagnosticCategory.Parsing,
                                                               6,
                                                               "Forbidden File Content",
                                                               DiagnosticSeverity.Error,
                                                               "File contains forbidden content elements.",
                                                               "A content element is defined in a file where no content is allowed.",
                                                               DiagnosticReportSeverity.Silent);

   public DiagnosticDescriptor ForbiddenBlock { get; } = new(DiagnosticCategory.Parsing,
                                                             7,
                                                             "Forbidden Block",
                                                             DiagnosticSeverity.Error,
                                                             "Block contains forbidden sub-blocks.",
                                                             "A block has sub-blocks defined where no sub-blocks are allowed or the number of allowed sub-blocks is exceeded.",
                                                             DiagnosticReportSeverity.Silent);

   public DiagnosticDescriptor InvalidContentElementCount { get; } = new(DiagnosticCategory.Parsing,
                                                                         8,
                                                                         "Invalid Content Element Count",
                                                                         DiagnosticSeverity.Error,
                                                                         "Block contains an invalid number of content elements.",
                                                                         "A block has an invalid number of content elements defined, which does not match the expected count.",
                                                                         DiagnosticReportSeverity.Silent);

   public DiagnosticDescriptor InvalidLocationKey { get; } = new(DiagnosticCategory.Parsing,
                                                                 9,
                                                                 "Invalid Location Name",
                                                                 DiagnosticSeverity.Error,
                                                                 "The location name '{0}' is invalid.",
                                                                 "This error indicates that the location name provided is not valid or does not conform to the expected format.",
                                                                 DiagnosticReportSeverity.Silent);

   public DiagnosticDescriptor DuplicateProvinceDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                          10,
                                                                          "Duplicate Province Definition",
                                                                          DiagnosticSeverity.Error,
                                                                          "Duplicate province definition found for '{0}'.",
                                                                          "Provinces must have unique names. This error indicates that a province with the same name already exists.",
                                                                          DiagnosticReportSeverity.PopupNotify);

   public DiagnosticDescriptor DuplicateAreaDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                      11,
                                                                      "Duplicate Area Definition",
                                                                      DiagnosticSeverity.Error,
                                                                      "Duplicate area definition found for '{0}'.",
                                                                      "Areas must have unique names. This error indicates that an area with the same name already exists.",
                                                                      DiagnosticReportSeverity.PopupNotify);

   public DiagnosticDescriptor DuplicateRegionDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                        12,
                                                                        "Duplicate Region Definition",
                                                                        DiagnosticSeverity.Error,
                                                                        "Duplicate region definition found for '{0}'.",
                                                                        "Regions must have unique names. This error indicates that a region with the same name already exists.",
                                                                        DiagnosticReportSeverity.PopupNotify);

   public DiagnosticDescriptor DuplicateSuperRegionDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                             13,
                                                                             "Duplicate Super Region Definition",
                                                                             DiagnosticSeverity.Error,
                                                                             "Duplicate super region definition found for '{0}'.",
                                                                             "Super regions must have unique names. This error indicates that a super region with the same name already exists.",
                                                                             DiagnosticReportSeverity.PopupNotify);

   public DiagnosticDescriptor DuplicateContinentDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                           14,
                                                                           "Duplicate Continent Definition",
                                                                           DiagnosticSeverity.Error,
                                                                           "Duplicate continent definition found for '{0}'.",
                                                                           "Continents must have unique names. This error indicates that a continent with the same name already exists.",
                                                                           DiagnosticReportSeverity.PopupNotify);
}