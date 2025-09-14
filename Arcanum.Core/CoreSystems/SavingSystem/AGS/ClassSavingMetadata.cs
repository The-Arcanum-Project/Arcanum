using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class ClassSavingMetadata(Enum keyword,
                                 TokenType separator,
                                 TokenType openingToken,
                                 TokenType closingToken,
                                 Func<IAgs, Enum, string>? commentProvider = null,
                                 Action<IAgs, PropertySavingMetaData, IndentedStringBuilder>? savingMethod = null)
{
   public Enum Keyword { get; } = keyword;
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public Func<IAgs, Enum, string>? CommentProvider { get; set; } = commentProvider;
   public Action<IAgs, PropertySavingMetaData, IndentedStringBuilder>? SavingMethod { get; set; } =
      savingMethod;
}