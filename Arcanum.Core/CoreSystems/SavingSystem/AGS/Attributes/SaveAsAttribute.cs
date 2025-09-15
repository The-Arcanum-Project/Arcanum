using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

/// <summary>
/// Attribute to define how a property should be saved using AGS (Automatic Generative Saving).
/// </summary>
/// <param name="valueType">What kind of data is represented</param>
/// <param name="separator">The separator between token and value</param>
/// <param name="savingMethod">What custom method to use to save. Must be located in <see cref="SavingActionProvider"/></param>
/// <param name="commentMethod">What method should be used to generate a saving comment from the value and name. Must be located in <see cref="SavingCommentProvider"/></param>
[AttributeUsage(AttributeTargets.Property)]
public class SaveAsAttribute(SavingValueType valueType = SavingValueType.Auto,
                             TokenType separator = TokenType.Equals,
                             string? savingMethod = null,
                             string? commentMethod = null,
                             string? collectionKeyMethod = null,
                             bool isCollection = false
) : Attribute
{
   public SavingValueType ValueType { get; } = valueType;
   public TokenType Separator { get; } = separator;
   public string? SavingMethod { get; } = savingMethod;
   public string? CommentMethod { get; } = commentMethod;
   public string? CollectionKeyMethod { get; } = collectionKeyMethod;
   public bool IsCollection { get; } = isCollection;
}