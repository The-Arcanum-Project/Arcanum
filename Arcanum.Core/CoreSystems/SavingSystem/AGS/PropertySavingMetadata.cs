using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class PropertySavingMetadata
{
   public required string Keyword { get; init; }
   public TokenType Separator { get; init; } = TokenType.Equals;
   public required Enum NxProp { get; init; }
   public SavingValueType ValueType { get; init; }
   public required AgsDelegates.AgsCommentProvider? CommentProvider { get; set; }
   public required AgsDelegates.AgsSavingAction? SavingMethod { get; set; }
   public required AgsDelegates.GetCollectionItemKey? CollectionItemKeyProvider { get; set; }
   public required bool IsCollection { get; set; }

   public void Format(IAgs ags, IndentedStringBuilder sb, string commentChar)
   {
      if (CommentProvider != null)
         CommentProvider(ags, commentChar, sb);

      if (SavingMethod == null)
         sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatObjectValue(ValueType, ags, NxProp)}");
      else
         SavingMethod(ags, this, sb);
   }
}