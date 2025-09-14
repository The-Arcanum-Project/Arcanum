using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class ClassSavingMetadata(string keyword,
                                 TokenType separator,
                                 TokenType openingToken,
                                 TokenType closingToken,
                                 Func<IAgs, Enum, string>? commentProvider,
                                 Action<IAgs, PropertySavingMetaData, IndentedStringBuilder>? savingMethod)
{
   public string Keyword { get; } = keyword;
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public required Func<IAgs, Enum, string>? CommentProvider { get; set; } = commentProvider;
   public required Action<IAgs, PropertySavingMetaData, IndentedStringBuilder>? SavingMethod { get; set; } =
      savingMethod;
}