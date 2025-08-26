using Microsoft.CodeAnalysis;

namespace Nexus.Analyzer;

internal static class Diagnostics
{
   public static readonly DiagnosticDescriptor SetValueTypeMismatch = new(id: "PS001",
                                                                          title: "Incorrect property value type",
                                                                          messageFormat:
                                                                          "The value for '{0}' must be convertible to type '{1}', but was given a '{2}'",
                                                                          category: "TypeSafety",
                                                                          defaultSeverity: DiagnosticSeverity.Error,
                                                                          isEnabledByDefault: true,
                                                                          description:
                                                                          "The value passed to SetValue does not match the type defined in the enum's ExpectedType attribute.");

   public static readonly DiagnosticDescriptor GetValueTypeMismatch = new(id: "PS002",
                                                                          title: "Incorrect property cast type",
                                                                          messageFormat:
                                                                          "The property '{0}' has an expected type of '{1}' but is being cast to '{2}'",
                                                                          category: "TypeSafety",
                                                                          defaultSeverity: DiagnosticSeverity.Error,
                                                                          isEnabledByDefault: true,
                                                                          description:
                                                                          "The result of GetValue is being cast to a type that does not match the ExpectedType attribute.");

   public static readonly DiagnosticDescriptor EnumMismatch = new(id: "PS003",
                                                                  title: "Mismatched property enum type",
                                                                  messageFormat:
                                                                  "The enum member '{0}' does not belong to the required enum '{1}' for the target of type '{2}'",
                                                                  category: "TypeSafety",
                                                                  defaultSeverity: DiagnosticSeverity.Error,
                                                                  isEnabledByDefault: true,
                                                                  description:
                                                                  "The enum provided does not match the 'Field' enum defined in the target object's class.");

}