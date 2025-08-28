using Microsoft.CodeAnalysis;

namespace DiagnosticArgsAnalyzer;

public static class Diagnostics
{
   public static readonly DiagnosticDescriptor IncorrectCollectionType = new(id: "ARC001",
                                                                             title: "Incorrect collection item type",
                                                                             messageFormat:
                                                                             "The collection property '{0}' expects items of type '{1}', but an item of type '{2}' was provided",
                                                                             category: "TypeSafety",
                                                                             defaultSeverity: DiagnosticSeverity.Error,
                                                                             isEnabledByDefault: true,
                                                                             description:
                                                                             "An item added to a collection property does not match the expected item type defined in the enum's ExpectedType attribute.");

   public static readonly DiagnosticDescriptor MissingCollectionProviderInterface = new(id: "ARC002",
    title: "INUI implementer should implement ICollectionProvider<T>",
    messageFormat:
    "The class '{0}' implements INUI but does not implement ICollectionProvider<{0}>. This is required for collection editing in the UI.",
    category: "Design",
    defaultSeverity: DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description:
    "To support dynamic collection editing, any class that implements INUI should also implement ICollectionProvider<T> where T is the class itself.");

}