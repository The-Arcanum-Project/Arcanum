using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.GameObjects;

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
   /// <param name="2">Block name</param>
   public DiagnosticDescriptor InvalidContentElementCount { get; } = new(DiagnosticCategory.Parsing,
                                                                         8,
                                                                         "Invalid Content Element Count",
                                                                         DiagnosticSeverity.Error,
                                                                         "Block contains an invalid number of content elements. Expected {0}, but found {1}.",
                                                                         "{0} content elements are expected in the block '{2}' but {1} were found.",
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

   /// <param name="0">The blocks Name</param>
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
   public DiagnosticDescriptor InvalidIntMarkup { get; } = new(DiagnosticCategory.Parsing,
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

   /// <param name="0">The line number where the error occurred</param>
   /// <param name="1">The line content that caused the error</param>
   /// <param name="2">The number of columns found in the line</param>
   public DiagnosticDescriptor InvalidAdjacencyLine { get; } = new(DiagnosticCategory.Parsing,
                                                                   23,
                                                                   "Invalid Adjacency Line",
                                                                   DiagnosticSeverity.Error,
                                                                   "Invalid adjacency line format at line {0}: '{1}'. Expected 9 columns but found {2}.",
                                                                   "There is a mismatch in the expected number of columns for an adjacency line. Expected 9 in format 'From;To;Type;Through;start_x;start_y;stop_x;stop_y;Comment'",
                                                                   DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The adjacency type that is invalid</param>
   public DiagnosticDescriptor InvalidAdjacencyType { get; } = new(DiagnosticCategory.Parsing,
                                                                   24,
                                                                   "Invalid Adjacency Type",
                                                                   DiagnosticSeverity.Error,
                                                                   "Invalid adjacency type '{0}' Expected.",
                                                                   $"The adjacency type specified in the adjacency line is not recognized. Valid types are {string.Join(", ", Enum.GetNames<AdjacencyType>().Select(x => $"'{x}'"))}.",
                                                                   DiagnosticReportSeverity.PopupNotify);
   /// <param name="0">The block name that is invalid</param>
   /// <param name="1">The expected block name</param>
   public DiagnosticDescriptor InvalidBlockName { get; } = new(DiagnosticCategory.Parsing,
                                                               25,
                                                               "Invalid Block Name",
                                                               DiagnosticSeverity.Error,
                                                               "The block name '{0}' is invalid in the current context.",
                                                               "A block with the name '{1}' was expected but the parser encountered a block with the name '{0}' instead.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The unexpected key in the key-value pair</param>
   /// <param name="1">The expected key in the key-value pair</param>
   public DiagnosticDescriptor UnexpectedKeyInKeyValuePair { get; } = new(DiagnosticCategory.Parsing,
                                                                          26,
                                                                          "Unexpected Key in Key-Value Pair",
                                                                          DiagnosticSeverity.Error,
                                                                          "The key '{0}' is unexpected in the key-value pair.",
                                                                          "A key-value pair was expected to have the key '{1}', but the parser encountered the key '{0}' instead.",
                                                                          DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The integer value that is out of range</param>
   /// <param name="1">The minimum allowed value</param>
   /// <param name="2">The maximum allowed value</param>
   public DiagnosticDescriptor IntOutOfRange { get; } = new(DiagnosticCategory.Parsing,
                                                            27,
                                                            "Integer Out of Range",
                                                            DiagnosticSeverity.Error,
                                                            "The integer value '{0}' is out of the allowed range ({1} to {2}).",
                                                            "The given integer '{0}' is outside the allowed range of {1} to {2}. Please ensure the value is within this range.",
                                                            DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The float value that is out of range</param>
   public DiagnosticDescriptor InvalidFloatMarkup { get; } = new(DiagnosticCategory.Parsing,
                                                                 28,
                                                                 "Invalid Float Markup",
                                                                 DiagnosticSeverity.Error,
                                                                 "Failed to parse float value from '{0}'.",
                                                                 "The provided string '{0}' could not be parsed as a valid float value. Please ensure it is a valid float in the format '0.00'.",
                                                                 DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The float value that is out of range</param>
   /// <param name="1">The minimum allowed value</param>
   /// <param name="2">The maximum allowed value</param>
   public DiagnosticDescriptor FloatOutOfRange { get; } = new(DiagnosticCategory.Parsing,
                                                              29,
                                                              "Float Out of Range",
                                                              DiagnosticSeverity.Error,
                                                              "The float value '{0}' is out of the allowed range ({1} to {2}).",
                                                              "The given float '{0}' is outside the allowed range of {1} to {2}. Please ensure the value is within this range.",
                                                              DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The string that could not be parsed</param>
   public DiagnosticDescriptor InvalidBoolMarkup { get; } = new(DiagnosticCategory.Parsing,
                                                                30,
                                                                "Invalid Boolean Markup",
                                                                DiagnosticSeverity.Error,
                                                                "Failed to parse boolean value from '{0}'. Expected 'yes' or 'no'.",
                                                                "The provided string '{0}' could not be parsed as a valid boolean value. Please ensure it is either 'yes' or 'no'.",
                                                                DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The key that is missing in the key-value pair</param>
   public DiagnosticDescriptor MissingKeyValue { get; } = new(DiagnosticCategory.Parsing,
                                                              31,
                                                              "Missing Key Value",
                                                              DiagnosticSeverity.Warning,
                                                              "The key '{0}' is missing in the key-value pair.",
                                                              "The key '{0}' was expected in the current content but was not found.",
                                                              DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The pop type key that is invalid</param>
   public DiagnosticDescriptor InvalidPopTypeKey { get; } = new(DiagnosticCategory.Parsing,
                                                                32,
                                                                "Invalid Pop Type Key",
                                                                DiagnosticSeverity.Error,
                                                                "The pop type key '{0}' is invalid.",
                                                                "The provided pop type key does not match any known pop types. Please ensure it is a valid pop type.",
                                                                DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The unknown key</param>
   /// <param name="1">The value associated with the unknown key</param>
   public DiagnosticDescriptor UnknownKey { get; } = new(DiagnosticCategory.Parsing,
                                                         33,
                                                         "Unknown Key",
                                                         DiagnosticSeverity.Error,
                                                         "The key '{0}' is unknown in the current context.",
                                                         "The key '{0}' with the value '{1}' is not recognized in the current parsing context.",
                                                         DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The object value that is duplicated</param>
   /// <param name="1">The object type that is duplicated</param>
   /// <param name="2">The property that uniquely identifies the object type</param>
   public DiagnosticDescriptor DuplicateObjectDefinition { get; } = new(DiagnosticCategory.Parsing,
                                                                        34,
                                                                        "Duplicate Object Definition",
                                                                        DiagnosticSeverity.Error,
                                                                        "Duplicate object definition found for '{0}' of type '{1}'.",
                                                                        "Objects of type '{1}' are uniquely identified by their '{2}' property.\n'{0}' Is defined multiple times which is not allowed.",
                                                                        DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The location name that is used for both start and end of the road</param>
   public DiagnosticDescriptor InvalidRoadSameLocation { get; } = new(DiagnosticCategory.Parsing,
                                                                      35,
                                                                      "Invalid Road Definition - Same Location",
                                                                      DiagnosticSeverity.Error,
                                                                      "Invalid road definition: start and end locations are the same ('{0}').",
                                                                      "A road cannot connect a location to itself. Please ensure that the start and end locations are different.",
                                                                      DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The line number where the syntax error occurred</param>
   /// <param name="1">The column number where the syntax error occurred</param>
   /// <param name="2">The unexpected token that caused the syntax error</param>
   /// <param name="3">The expected token or tokens</param>
   public DiagnosticDescriptor SyntaxError { get; } = new(DiagnosticCategory.Parsing,
                                                          36,
                                                          "Syntax Error",
                                                          DiagnosticSeverity.Error,
                                                          "Syntax Error on line {0}:{1}: Unexpected token '{2}'.",
                                                          "Expected {3}.",
                                                          DiagnosticReportSeverity.PopupNotify);

   public DiagnosticDescriptor UnexpectedToken { get; } = new(DiagnosticCategory.Parsing,
                                                              37,
                                                              "Unexpected Token",
                                                              DiagnosticSeverity.Error,
                                                              "Unexpected token '{0}' in line {1}:{2}.",
                                                              "The parser encountered a token that was not expected in the current context. Please check the syntax and structure of the input.",
                                                              DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The line number where the invalid block type was found</param>
   /// <param name="1">The column number where the invalid block type was found</param>
   /// <param name="2">The invalid block type that was found</param>
   /// <param name="3">The expected block type or types</param>
   public DiagnosticDescriptor InvalidBlockType { get; } = new(DiagnosticCategory.Parsing,
                                                               38,
                                                               "Invalid Block Type",
                                                               DiagnosticSeverity.Error,
                                                               "Invalid block type at line {0}:{1}: '{2}'.",
                                                               "In this context only blocks of the type(s) '{3}' are allowed.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The invalid content key or type that was found</param>
   /// <param name="1">The expected content key or type or types</param>
   public DiagnosticDescriptor InvalidContentKeyOrType { get; } = new(DiagnosticCategory.Parsing,
                                                                      39,
                                                                      "Invalid Content Key or Type",
                                                                      DiagnosticSeverity.Error,
                                                                      "Invalid content key or type '{0}'.",
                                                                      "In the current context only '{1}' is expected",
                                                                      DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The block name that is invalid</param>
   /// <param name="1">The expected block names</param>
   public DiagnosticDescriptor InvalidBlockNames { get; } = new(DiagnosticCategory.Parsing,
                                                                40,
                                                                "Invalid Block Name",
                                                                DiagnosticSeverity.Error,
                                                                "The block name '{0}' is invalid in the current context.",
                                                                "A block with (one of) the name(s) '{1}' was expected but the parser encountered a block with the name '{0}' instead.",
                                                                DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The node type that is invalid</param>
   /// <param name="1">The expected node type or types</param>
   /// <param name="2">The actual node or it's name that was found</param>
   public DiagnosticDescriptor InvalidNodeType { get; } = new(DiagnosticCategory.Parsing,
                                                              41,
                                                              "Invalid Node Type",
                                                              DiagnosticSeverity.Error,
                                                              "The node type '{0}' is invalid in the current context.",
                                                              "The node ('{2}') of the type '{1}' was expected but the parser encountered a node of the type '{0}' instead.",
                                                              DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The node type that is invalid</param>
   /// <param name="1">The expected node count</param>
   /// <param name="2">The actual node count</param>
   public DiagnosticDescriptor InvalidNodeCountOfType { get; } = new(DiagnosticCategory.Parsing,
                                                                     42,
                                                                     "Invalid Node Count of Type",
                                                                     DiagnosticSeverity.Error,
                                                                     "The node count of type '{0}' is invalid. Expected {1}, but found {2}.",
                                                                     "This error indicates that the number of nodes of type '{0}' does not match the expected count.",
                                                                     DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The tag that is invalid</param>
   public DiagnosticDescriptor InvalidTagFormat { get; } = new(DiagnosticCategory.Parsing,
                                                               43,
                                                               "Invalid Tag Format",
                                                               DiagnosticSeverity.Error,
                                                               "The tag format is invalid: '{0}'.",
                                                               "Tags must be made of 3 alphanumeric characters. The provided tag '{0}' does not conform to this format.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The expected separator type</param>
   /// <param name="1">The actual separator found</param>
   public DiagnosticDescriptor InvalidSeparator { get; } = new(DiagnosticCategory.Parsing,
                                                               44,
                                                               "Invalid Separator",
                                                               DiagnosticSeverity.Error,
                                                               "The separator in the key-value pair is invalid",
                                                               "Expected a separator of type {0} but found '{1}' instead.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The integer value that is invalid</param>
   public DiagnosticDescriptor InvalidIntegerValue { get; } = new(DiagnosticCategory.Parsing,
                                                                  45,
                                                                  "Invalid Integer Value",
                                                                  DiagnosticSeverity.Error,
                                                                  "The integer value is invalid: '{0}'.",
                                                                  "The provided integer value '{0}' is not a valid integer. Please ensure it is a valid integer format.",
                                                                  DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The country rank key that is invalid</param>
   /// <param name="1">A list of valid country rank keys</param>
   public DiagnosticDescriptor InvalidCountryRankKey { get; } = new(DiagnosticCategory.Parsing,
                                                                    46,
                                                                    "Invalid Country Rank Key",
                                                                    DiagnosticSeverity.Error,
                                                                    "The country rank key '{0}' is invalid.",
                                                                    "The provided country rank key does not match any known country ranks: '{1}'.",
                                                                    DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The integer value that is forbidden</param>
   /// <param name="1">The context / field in which the integer value is forbidden</param>
   public DiagnosticDescriptor ForbiddenIntegerValue { get; } = new(DiagnosticCategory.Parsing,
                                                                    47,
                                                                    "Forbidden Integer Value",
                                                                    DiagnosticSeverity.Error,
                                                                    "The integer value is forbidden: '{0}'.",
                                                                    "The provided integer value '{0}' is not allowed as a value for {1}.",
                                                                    DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The enum value that is invalid</param>
   /// <param name="1">The expected enum type</param>
   /// <param name="2">A list of valid enum values for the expected enum type</param>
   public DiagnosticDescriptor InvalidEnumValue { get; } = new(DiagnosticCategory.Parsing,
                                                               48,
                                                               "Invalid Enum Value",
                                                               DiagnosticSeverity.Error,
                                                               "The enum value is invalid: '{0}'.",
                                                               "The provided enum value '{0}' does not match any known values for the expected enum type {1}:'{2}'.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The file path that was parsed</param>
   public DiagnosticDescriptor EmptyRootNode { get; } = new(DiagnosticCategory.Parsing,
                                                            49,
                                                            "Empty Root Node",
                                                            DiagnosticSeverity.Warning,
                                                            "The root node is empty.",
                                                            "The root node of the parsed content from file {0} is empty. It is either an empty file and can be discarded or the file is corrupted and needs to be fixed.",
                                                            DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The expected statement count</param>
   /// <param name="1">The actual statement count</param>
   public DiagnosticDescriptor InvalidStatementCount { get; } = new(DiagnosticCategory.Parsing,
                                                                    50,
                                                                    "Invalid Statement Count",
                                                                    DiagnosticSeverity.Error,
                                                                    "The statement count is invalid. Expected {0}, but found {1}.",
                                                                    "The root node must contain exactly {0} statements. The current count is {1}.",
                                                                    DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The unexpected token that was found instead of an identifier</param>
   /// <param name="1">The type of the unexpected token</param>
   public DiagnosticDescriptor ExpectedIdentifier { get; } = new(DiagnosticCategory.Parsing,
                                                                 51,
                                                                 "Expected Identifier",
                                                                 DiagnosticSeverity.Error,
                                                                 "Expected an identifier on the left side but found '{0}'.",
                                                                 "An identifier is required in this context, but the parser encountered '{0}' of type '{1}' instead.",
                                                                 DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The file path that could not be parsed</param>
   public DiagnosticDescriptor UnsuccessfulFileParse { get; } = new(DiagnosticCategory.Parsing,
                                                                    52,
                                                                    "Unsuccessful File Parse",
                                                                    DiagnosticSeverity.Error,
                                                                    "The file {0} could not be parsed successfully.",
                                                                    "The parser encountered errors while processing the file. Please review the diagnostics for details.",
                                                                    DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The unknown object key.</param>
   /// <param name="1">The expected object type for which the key is unknown.</param>
   public DiagnosticDescriptor UnknownObjectKey { get; } = new(DiagnosticCategory.Parsing,
                                                               53,
                                                               "Unknown Object Key",
                                                               DiagnosticSeverity.Error,
                                                               "The object key '{0}' is unknown in the current context.",
                                                               "Expected a key for an object of type '{1}', but found the unknown key '{0}' instead.",
                                                               DiagnosticReportSeverity.PopupNotify);

   /// <param name="0">The string that could not be parsed</param>
   /// <param name="1">The expected enum type</param>
   public DiagnosticDescriptor EnumParseError { get; } = new(DiagnosticCategory.Parsing,
                                                             54,
                                                             "Enum Parse Error",
                                                             DiagnosticSeverity.Error,
                                                             "Failed to parse enum of type '{1}' value from '{0}'.",
                                                             "The provided string '{0}' could not be parsed to the target enum type '{1}'. Please ensure it is a valid enum value.",
                                                             DiagnosticReportSeverity.PopupNotify);
}