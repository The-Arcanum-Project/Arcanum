using Microsoft.CodeAnalysis;

namespace Nexus.SourceGen;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor RedundantIgnoreAttributeWarning = new(
                                                                                      id: "PM001",
                                                                                      title: "Redundant Ignore Attribute",
                                                                                      messageFormat: "The [IgnoreModifiable] attribute is redundant in 'Explicit' inclusion mode. Fields are ignored by default in this mode and only included if marked with [AddModifiable].",
                                                                                      category: "Usage",
                                                                                      defaultSeverity: DiagnosticSeverity.Warning,
                                                                                      isEnabledByDefault: true,
                                                                                      description: "The [IgnoreModifiable] attribute should only be used in 'Implicit' mode to exclude a field that would otherwise be included.");
    
    public static readonly DiagnosticDescriptor RedundantAddAttributeWarning = new(
                                                                                   id: "PM002",
                                                                                   title: "Redundant Add Attribute",
                                                                                   messageFormat: "The [AddModifiable] attribute is redundant in 'Implicit' inclusion mode. Fields are included by default in this mode and only excluded if marked with [IgnoreModifiable].",
                                                                                   category: "Usage",
                                                                                   defaultSeverity: DiagnosticSeverity.Warning,
                                                                                   isEnabledByDefault: true,
                                                                                   description: "The [AddModifiable] attribute should only be used in 'Explicit' mode to exclude a field that would otherwise be included.");
}