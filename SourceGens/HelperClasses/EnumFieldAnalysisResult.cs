using Microsoft.CodeAnalysis;

namespace ParserGenerator.HelperClasses;

public sealed class EnumFieldAnalysisResult(IFieldSymbol fieldSymbol, bool isValid, string key, bool isIgnored)
{
   public IFieldSymbol FieldSymbol { get; } = fieldSymbol;
   public bool IsValid { get; } = isValid;
   public string Key { get; } = key;
   public bool IsIgnored { get; } = isIgnored;
}

/// <summary>
/// Holds the complete analysis result for an entire enum type.
/// </summary>
public sealed class EnumAnalysisResult(INamedTypeSymbol enumSymbol,
                                       bool isValid,
                                       IReadOnlyList<EnumFieldAnalysisResult> fieldResults)
{
   public INamedTypeSymbol EnumSymbol { get; } = enumSymbol;
   public bool IsValid { get; } = isValid;
   public IReadOnlyList<EnumFieldAnalysisResult> FieldResults { get; } = fieldResults;
}