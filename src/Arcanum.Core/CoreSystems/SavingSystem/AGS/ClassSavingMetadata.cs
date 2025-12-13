using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class ClassSavingMetadata(TokenType separator,
                                 TokenType openingToken,
                                 TokenType closingToken,
                                 Func<IAgs, string, IndentedStringBuilder, string>? commentProvider = null,
                                 Action<IAgs, HashSet<PropertySavingMetadata>, IndentedStringBuilder, bool>? savingMethod = null,
                                 bool asOneLine = false)
{
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public Func<IAgs, string, IndentedStringBuilder, string>? CommentProvider { get; set; } = commentProvider;
   public Action<IAgs, HashSet<PropertySavingMetadata>, IndentedStringBuilder, bool>? SavingMethod { get; set; } =
      savingMethod;
   public bool AsOneLine { get; } = asOneLine;

   public override string ToString()
   {
      if (SavingMethod == null)
         return "Cm with no saving method";

      return $"Cm with {SavingMethod?.Method.Name ?? "no"}";
   }
}