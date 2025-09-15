using Microsoft.CodeAnalysis;

namespace ParserGenerator;

public static class DefinedDiagnostics
{
   public static readonly DiagnosticDescriptor MissingSaveAsAttributeWarning = new(id: "AGS004",
       title: "Missing [SaveAs] attribute",
       messageFormat:
       "Property '{0}' will not be saved because it is missing a [SaveAs] attribute. Add the attribute to include it in serialization, or add [SuppressAgs] to explicitly ignore it.",
       category: "SavingGenerator",
       DiagnosticSeverity.Warning, // This is a warning, not an error.
       isEnabledByDefault: true);

   // An error for a missing keyword, which is unrecoverable.
   public static readonly DiagnosticDescriptor MissingParseAsKeywordError = new(id: "AGS002",
                                                                                title:
                                                                                "Invalid or Missing ParseAs Keyword",
                                                                                messageFormat:
                                                                                "Property '{0}' cannot be saved because it is missing a [ParseAs] attribute with a valid keyword",
                                                                                category: "SavingGenerator",
                                                                                DiagnosticSeverity.Error,
                                                                                isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor MissingNexusEnumPropertyKey = new(id: "AGS006",
       title:
       "Missing Nexus Enum Property Key",
       messageFormat:
       "The property '{0}' corresponding to enum field '{1}' was not found in class '{2}'. Ensure that the property exists and matches the enum field name.",
       category: "SavingGenerator",
       DiagnosticSeverity
         .Error, // This should be a build-breaking error.
       isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidNexusKeyPropType = new("AGS010",
                                                                             "Incorrect Key Property Type",
                                                                             "The key-defining property '{0}' specified in [ObjectSaveAs] must be of type 'string', but was found to be of type '{1}'",
                                                                             "SavingGenerator",
                                                                             DiagnosticSeverity.Error,
                                                                             true);

   public static readonly DiagnosticDescriptor MissingObjectSaveAsAttribute = new(id: "AGS001",
       title: "Missing ObjectSaveAs Attribute",
       messageFormat:
       "The class '{0}' must have an [ObjectSaveAs] attribute to be processed by the AGS saving generator",
       category: "AgsSavingGenerator",
       DiagnosticSeverity.Error,
       isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidDefineKeywordAttribute = new(id: "AGS002",
       title: "Invalid ParseAs Keyword",
       messageFormat:
       "Property '{0}' in class '{1}' has an invalid or empty keyword in its [ParseAs] attribute",
       category: "SavingGenerator",
       DiagnosticSeverity.Error,
       isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidObjectSaveAsAttribute = new(id: "AGS003",
       title: "Invalid ObjectSaveAs Attribute",
       messageFormat:
       "The [ObjectSaveAs] attribute on class '{0}' must specify a non-empty string key corresponding to a string property in the class",
       category: "SavingGenerator",
       DiagnosticSeverity.Error,
       isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidKeyTargetProperty = new(id: "AGS005",
                                                                              title: "Invalid Key Target Property",
                                                                              messageFormat:
                                                                              "The property '{0}' specified in [ObjectSaveAs] on class '{1}' must be an INexus property and be of type 'string'",
                                                                              category: "SavingGenerator",
                                                                              DiagnosticSeverity.Error,
                                                                              isEnabledByDefault: true);

   public static readonly DiagnosticDescriptor InvalidKeyTargetPropertyType = new(id: "AGS007",
       title: "Invalid Key Target Property Type",
       messageFormat:
       "The property '{0}' specified in [ObjectSaveAs] on class '{1}' must be of type 'string', but is of type '{2}'",
       category: "SavingGenerator",
       DiagnosticSeverity.Error,
       isEnabledByDefault: true);
}