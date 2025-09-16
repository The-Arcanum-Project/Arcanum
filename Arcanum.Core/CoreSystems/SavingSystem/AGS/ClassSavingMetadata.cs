using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class ClassSavingMetadata(TokenType separator,
                                 TokenType openingToken,
                                 TokenType closingToken,
                                 Func<IAgs, string, IndentedStringBuilder, string>? commentProvider = null,
                                 Action<IAgs, PropertySavingMetadata, IndentedStringBuilder>? savingMethod = null)
{
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public Func<IAgs, string, IndentedStringBuilder, string>? CommentProvider { get; set; } = commentProvider;
   public Action<IAgs, PropertySavingMetadata, IndentedStringBuilder>? SavingMethod { get; set; } =
      savingMethod;

   public override string ToString()
   {
      return $"Cm with {SavingMethod?.Method.Name ?? "no"}";
   }
}