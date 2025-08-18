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

   /// <param name="0">Expected content element count</param>
   /// <param name="1">Actual content element count</param>
   public DiagnosticDescriptor InvalidContentElementCount { get; } = new(DiagnosticCategory.Parsing,
                                                                         8,
                                                                         "Invalid Content Element Count",
                                                                         DiagnosticSeverity.Error,
                                                                         "Block contains an invalid number of content elements. Expected {0}, but found {1}.",
                                                                         "A block has an invalid number of content elements defined, which does not match the expected count.",
                                                                         DiagnosticReportSeverity.Silent);

   /// <param name="0">The location name that is invalid</param>
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

   /// <param name="0">The blocks parent</param>
   /// <param name="1">The expected block count</param>
   /// <param name="2">The actual block count</param>
   public DiagnosticDescriptor InvalidBlockCount { get; } = new(DiagnosticCategory.Parsing,
                                                                15,
                                                                "Invalid Block Count",
                                                                DiagnosticSeverity.Error,
                                                                "The block count of '{0}' is invalid. Expected {1}, but found {2}.",
                                                                "This error indicates that the number of blocks defined does not match the expected count.",
                                                                DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The string that could not be parsed</param>
   public DiagnosticDescriptor BoolParsingError { get; } = new(DiagnosticCategory.Parsing,
                                                               16,
                                                               "Boolean Parsing Error",
                                                               DiagnosticSeverity.Error,
                                                               "Failed to parse boolean value from '{0}'.",
                                                               "This error indicates that the parser could not convert the provided string to a boolean value. Please ensure it is 'yes' or 'no'.",
                                                               DiagnosticReportSeverity.PopupNotify);
   
    /// <param name="0">The string that could not be parsed</param>
   public DiagnosticDescriptor IntParsingError { get; } = new(DiagnosticCategory.Parsing,
                                                               17,
                                                               "Integer Parsing Error",
                                                               DiagnosticSeverity.Error,
                                                               "Failed to parse integer value from '{0}'.",
                                                               "This error indicates that the parser could not convert the provided string to an integer value. Please ensure it is a valid integer.",
                                                               DiagnosticReportSeverity.PopupNotify);
   
   /// <param name="0">The string that could not be parsed</param>
    public DiagnosticDescriptor FloatParsingError { get; } = new(DiagnosticCategory.Parsing,
                                                                    18,
                                                                    "Float Parsing Error",
                                                                    DiagnosticSeverity.Error,
                                                                    "Failed to parse float value from '{0}'.",
                                                                    "This error indicates that the parser could not convert the provided string to a float value. Please ensure it is a valid float.",
                                                                    DiagnosticReportSeverity.PopupNotify);
    
   /// <param name="0">The unknown key.</param>
    public DiagnosticDescriptor UnknownKeyInDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                    19,
                                                                    "Unknown Key in Definition",
                                                                    DiagnosticSeverity.Error,
                                                                    "The key '{0}' is not recognized in the current context.",
                                                                    "This error indicates that the parser encountered a key that is not defined or recognized in the current parsing context.",
                                                                    DiagnosticReportSeverity.PopupNotify);
   
   /// <param name="0">The default map collection name that is unknown.</param>
   public DiagnosticDescriptor UnknownDefaultMapCollectionName { get; } = new(DiagnosticCategory.Parsing,
                                                                    20,
                                                                    "Unknown Default Map Collection Name",
                                                                    DiagnosticSeverity.Error,
                                                                    "The default map collection name '{0}' is not recognized.",
                                                                    "The only valid location collection names are: sound_tolls, non_ownable, impassable_mountains, volcanoes, earthquakes, sea_zones, lakes.",
                                                                    DiagnosticReportSeverity.PopupNotify);
   
   public DiagnosticDescriptor InvalidDefaultMapDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                    21,
                                                                    "Invalid Default Map Definition",
                                                                    DiagnosticSeverity.Error,
                                                                    "The default map definition is invalid.",
                                                                    "The default.map is either missing a bock of location definitions or one of its defining attributes is invalid.",
                                                                    DiagnosticReportSeverity.PopupError);
   
    /// <param name="0">The line number where the error occurred</param>
    /// <param name="1">The line content that caused the error</param>
    public DiagnosticDescriptor InvalidLineFormat { get; } = new(DiagnosticCategory.Parsing,
                                                                    22,
                                                                    "Invalid Line Format",
                                                                    DiagnosticSeverity.Error,
                                                                    "Invalid line format at line {0}: '{1}'. Expected format is 'key=value'.",
                                                                    "This error indicates that the line does not conform to the expected key-value pair format.",
                                                                    DiagnosticReportSeverity.PopupNotify);
    
   
}