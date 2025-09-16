using System.Collections;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class PropertySavingMetadata
{
   public required string Keyword { get; init; }
   public TokenType Separator { get; init; } = TokenType.Equals;
   public required Enum NxProp { get; init; }
   public SavingValueType ValueType { get; set; }
   public required AgsDelegates.AgsCommentProvider? CommentProvider { get; set; }
   public required AgsDelegates.AgsSavingAction? SavingMethod { get; set; }
   public required AgsDelegates.GetCollectionItemKey? CollectionItemKeyProvider { get; set; }
   public required bool IsCollection { get; set; }

   public void Format(IAgs ags, IndentedStringBuilder sb, string commentChar, SavingFormat format)
   {
      if (CommentProvider != null)
         CommentProvider(ags, commentChar, sb);

      if (ValueType == SavingValueType.Auto)
         ValueType = SavingUtil.GetSavingValueType(ags[NxProp]);

      if (SavingMethod == null)
      {
         if (ValueType == SavingValueType.IAgs)
         {
            HandleIAgsProperty((IAgs)ags[NxProp], sb, commentChar);
            return;
         }

         if (IsCollection)
            HandleCollection(ags, sb, commentChar, format);
         else
            HandlePrimitiveProperty(ags, sb);
      }
      else
         SavingMethod(ags, this, sb);
   }

   private static void HandleIAgsProperty(IAgs ags, IndentedStringBuilder sb, string commentChar)
      => ags.ToAgsContext(commentChar).BuildContext(sb);

   private void HandlePrimitiveProperty(IAgs ags, IndentedStringBuilder sb)
   {
      sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatObjectValue(ValueType, ags, NxProp)}");
   }

   private void HandleCollection(IAgs ags, IndentedStringBuilder sb, string commentChar, SavingFormat format)
   {
      using (sb.BlockWithName(Keyword))
      {
         // ValueType is only auto if the property is null or an empty collection
         if (ags.Settings.WriteEmptyCollectionHeader && ValueType == SavingValueType.Auto)
            sb.AppendComment(commentChar, "Empty Collection");
         else
         {
            if (format == SavingFormat.Spacious)
               sb.AppendLine();
            List<string> keys = [];
            if (CollectionItemKeyProvider != null)
               keys.AddRange(from object item in (IEnumerable)ags[NxProp]
                             select CollectionItemKeyProvider(item));
            else
               keys.AddRange(from object item in (IEnumerable)ags[NxProp]
                             select SavingUtil.FormatValue(ValueType, item));
            sb.AppendList(keys);
            if (format == SavingFormat.Spacious)
               sb.AppendLine();
         }
      }
   }
}