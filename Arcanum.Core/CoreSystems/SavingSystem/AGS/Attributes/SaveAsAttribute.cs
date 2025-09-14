using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

/// <summary>
/// What type of value is being saved. Mostly used to mark identifiers instead of strings.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SaveAsAttribute(SavingValueType valueType,
                             TokenType separator = TokenType.Equals,
                             string? savingMethod = null,
                             string? commentMethod = null) : Attribute
{
   public SavingValueType ValueType { get; } = valueType;
   public TokenType Separator { get; } = separator;
   public string? SavingMethod { get; } = savingMethod;
   public string? CommentMethod { get; } = commentMethod;
}