using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

/// <summary>
/// Describes how to save an object using AGS (Automatic Generative Saving).
/// </summary>
/// <param name="separator">The separator between keyword and opening token. By default '='</param>
/// <param name="openingToken">The token that opens the object. By default '{'</param>
/// <param name="closingToken">The token that closes the object. By default '}'</param>
/// <param name="savingMethod">What custom method to use to save. Must be located in <see cref="SavingActionProvider"/></param>
/// <param name="commentMethod">What method should be used to generate a saving comment from the value and name. Must be located in <see cref="SavingCommentProvider"/></param>
[AttributeUsage(AttributeTargets.Class)]
public class ObjectSaveAsAttribute(TokenType separator = TokenType.Equals,
                                   TokenType openingToken = TokenType.LeftBrace,
                                   TokenType closingToken = TokenType.RightBrace,
                                   string? savingMethod = null,
                                   string? commentMethod = null,
                                   bool asOneLine = false)
   : Attribute
{
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public string? SavingMethod { get; } = savingMethod;
   public string? CommentMethod { get; } = commentMethod;
   public bool AsOneLine { get; } = asOneLine;
}