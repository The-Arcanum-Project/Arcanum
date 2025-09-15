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

   public void Format(IAgs ags, IndentedStringBuilder sb, string commentChar)
   {
      if (CommentProvider != null)
         CommentProvider(ags, commentChar, sb);

      if (SavingMethod == null)
      {
         if (IsCollection)
         {
            if (ValueType == SavingValueType.Auto)
            {
               if (ags[NxProp] is IEnumerable enumerable)
                  ValueType = SavingUtil.GetSavingValueTypeForCollection(enumerable);
               else
                  throw new
                     InvalidOperationException($"Property {NxProp} is marked as a collection but does not implement IEnumerable.");
            }

            using (sb.BlockWithName(Keyword))
               // ValueType is only auto if the property is null or an empty collection
               if (ags.Settings.WriteEmptyCollectionHeader && ValueType == SavingValueType.Auto)
                  sb.AppendComment(commentChar, "Empty Collection");
               else
               {
                  List<string> keys = [];
                  if (CollectionItemKeyProvider != null)
                     keys.AddRange(from object item in (IEnumerable)ags[NxProp]
                                   select CollectionItemKeyProvider(item));
                  else
                     keys.AddRange(from object item in (IEnumerable)ags[NxProp]
                                   select SavingUtil.FormatValue(ValueType, item));
                  sb.AppendList(keys);
               }
         }
         else
         {
            sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatObjectValue(ValueType, ags, NxProp)}");
         }
      }
      else
         SavingMethod(ags, this, sb);
   }
}