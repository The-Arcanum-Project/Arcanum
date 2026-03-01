using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class ClassSavingMetadata(TokenType separator,
                                 TokenType openingToken,
                                 TokenType closingToken,
                                 Func<IEu5Object, string, IndentedStringBuilder, string>? commentProvider = null,
                                 Action<IEu5Object, HashSet<PropertySavingMetadata>, IndentedStringBuilder, bool, bool>? savingMethod = null)
{
   public TokenType Separator { get; } = separator;
   public TokenType OpeningToken { get; } = openingToken;
   public TokenType ClosingToken { get; } = closingToken;
   public Func<IEu5Object, string, IndentedStringBuilder, string>? CommentProvider { get; set; } = commentProvider;
   public Action<IEu5Object, HashSet<PropertySavingMetadata>, IndentedStringBuilder, bool, bool>? SavingMethod { get; set; } =
      savingMethod;

   public override string ToString()
   {
      if (SavingMethod == null)
         return "Cm with no saving method";

      return $"Cm with {SavingMethod?.Method.Name ?? "no"}";
   }
}