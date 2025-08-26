using Microsoft.CodeAnalysis;

namespace DiagnosticArgsAnalyzer;

public static class Diagnostics
{
   public static readonly DiagnosticDescriptor IncorrectCollectionType = new(id: "PS004",
                                                                             title: "Incorrect collection item type",
                                                                             messageFormat:
                                                                             "The collection property '{0}' expects items of type '{1}', but an item of type '{2}' was provided",
                                                                             category: "TypeSafety",
                                                                             defaultSeverity: DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true,
                                                                             description:
                                                                             "An item added to a collection property does not match the expected item type defined in the enum's ExpectedType attribute.");
}