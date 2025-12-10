using Microsoft.CodeAnalysis;

namespace ParserGenerator;

public static class DefinedDiagnostics
{
   private const string CATEGORY = "UniGen";

   public static readonly DiagnosticDescriptor MissingObjectSaveAsAttribute = new (id: "AGS001",
                                                                                   title: "Missing ObjectSaveAs Attribute",
                                                                                   messageFormat:
                                                                                   "The class '{0}' must have an [ObjectSaveAs] attribute to be processed by the AGS saving generator",
                                                                                   category: "AgsSavingGenerator",
                                                                                   DiagnosticSeverity.Error,
                                                                                   isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidDefineKeywordAttribute = new (id: "AGS002",
                                                                                    title: "Invalid ParseAs Keyword",
                                                                                    messageFormat:
                                                                                    "Property '{0}' in class '{1}' has an invalid or empty keyword in its [ParseAs] attribute",
                                                                                    category: CATEGORY,
                                                                                    DiagnosticSeverity.Error,
                                                                                    isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingSaveAsAttribute = new (id: "AGS003",
                                                                             title: "Missing SaveAs Attribute",
                                                                             messageFormat:
                                                                             "Property '{0}' in class '{1}' is missing the required [SaveAs] attribute",
                                                                             category: CATEGORY,
                                                                             DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingSaveAsAttributeWarning = new (id: "AGS004",
                                                                                    title: "Missing [SaveAs] attribute",
                                                                                    messageFormat:
                                                                                    "Property '{0}' will not be saved because it is missing a [SaveAs] attribute. Add the attribute to include it in serialization, or add [SuppressAgs] to explicitly ignore it.",
                                                                                    category: CATEGORY,
                                                                                    DiagnosticSeverity.Warning, // This is a warning, not an error.
                                                                                    isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidKeyTargetProperty = new (id: "AGS005",
                                                                               title: "Invalid Key Target Property",
                                                                               messageFormat:
                                                                               "The property '{0}' specified in [ObjectSaveAs] on class '{1}' must be an INexus property and be of type 'string'",
                                                                               category: CATEGORY,
                                                                               DiagnosticSeverity.Error,
                                                                               isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingNexusEnumPropertyKey = new (id: "AGS006",
                                                                                  title:
                                                                                  "Missing Nexus Enum Property Key",
                                                                                  messageFormat:
                                                                                  "The property '{0}' corresponding to enum field '{1}' was not found in class '{2}'. Ensure that the property exists and matches the enum field name.",
                                                                                  category: CATEGORY,
                                                                                  DiagnosticSeverity
                                                                                    .Error, // This should be a build-breaking error.
                                                                                  isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidKeyTargetPropertyType = new (id: "AGS007",
                                                                                   title: "Invalid Key Target Property Type",
                                                                                   messageFormat:
                                                                                   "The property '{0}' specified in [ObjectSaveAs] on class '{1}' must be of type 'string', but is of type '{2}'",
                                                                                   category: CATEGORY,
                                                                                   DiagnosticSeverity.Error,
                                                                                   isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingParseAsAttribute = new (id: "AGS008",
                                                                              title: "Missing ParseAs Attribute",
                                                                              messageFormat:
                                                                              "Property '{0}' in class '{1}' is missing a [ParseAs] attribute, which is required for AGS serialization",
                                                                              category: CATEGORY,
                                                                              DiagnosticSeverity.Error,
                                                                              isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingDefaultValueAttributeWarning = new (id: "AGS009",
                                                                                          title: "Missing DefaultValue Attribute",
                                                                                          messageFormat:
                                                                                          "Property '{0}' in class '{1}' is missing a [DefaultValue] attribute. If the attribtue is not defined, omitting default values during saving is not possible.",
                                                                                          category: CATEGORY,
                                                                                          DiagnosticSeverity.Warning, // This is a warning, not an error.
                                                                                          isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingEnumAgsDataAttribute = new (id: "AGS010",
                                                                                  title: "Missing EnumAgsData Attribute",
                                                                                  messageFormat:
                                                                                  "The enum '{0}' must have an [EnumAgsData] attribute to be processed by the AGS saving generator",
                                                                                  category: CATEGORY,
                                                                                  DiagnosticSeverity.Error,
                                                                                  isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidSaveAsAttribute = new (id: "AGS011",
                                                                             title: "Invalid SaveAs Attribute",
                                                                             messageFormat:
                                                                             "The [SaveAs] attribute on property '{0}' in class '{1}' has an invalid argument: {2}",
                                                                             category: CATEGORY,
                                                                             DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true);
}