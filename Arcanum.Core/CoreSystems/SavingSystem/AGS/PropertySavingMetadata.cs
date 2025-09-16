using System.Collections;
using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public class PropertySavingMetadata
{
   /// <summary>
   /// The keyword to use when saving this property.
   /// </summary>
   public required string Keyword { get; init; }
   /// <summary>
   /// The separator to use between the keyword and the value.
   /// </summary>
   public TokenType Separator { get; init; } = TokenType.Equals;
   /// <summary>
   /// The Nexus property this metadata is for.
   /// </summary>
   public required Enum NxProp { get; init; }
   /// <summary>
   /// The type of value this property represents. <br/>
   /// If set to Auto, the type will be determined at runtime based on the actual value.
   /// </summary>
   public SavingValueType ValueType { get; set; }
   /// <summary>
   /// The default value for this property. <br/>
   /// Used to determine if the property should be saved or omitted (if it matches the default value).
   /// </summary>
   public required object? DefaultValue { get; set; }
   /// <summary>
   /// A delegate to provide a comment for this property when saving. <br/>
   /// If null, no comment will be added. 
   /// </summary>
   public required AgsDelegates.AgsCommentProvider? CommentProvider { get; set; }
   /// <summary>
   /// A delegate to provide a custom saving method for this property. <br/>
   /// If null, the property will be saved using the default method based on its type.
   /// </summary>
   public required AgsDelegates.AgsSavingAction? SavingMethod { get; set; }
   /// <summary>
   /// A delegate to provide a key for each item in a collection when saving. <br/>
   /// If null, the items will be saved using their value representation.
   /// </summary>
   public required AgsDelegates.GetCollectionItemKey? CollectionItemKeyProvider { get; set; }
   /// <summary>
   /// Indicates whether this property is a collection (e.g., List, Array). <br/>
   /// If true, the property will be handled as a collection during saving.
   /// </summary>
   public required bool IsCollection { get; set; }

   /// <summary>
   /// Formats the property and appends it to the provided IndentedStringBuilder. 
   /// </summary>
   /// <param name="ags"></param>
   /// <param name="sb"></param>
   /// <param name="commentChar"></param>
   /// <param name="settings"></param>
   public void Format(IAgs ags, IndentedStringBuilder sb, string commentChar, AgsSettings settings)
   {
      if (CommentProvider != null)
         CommentProvider(ags, commentChar, sb);

      if (ValueType == SavingValueType.Auto)
         ValueType = SavingUtil.GetSavingValueType(ags[NxProp]);

      var value = ags[NxProp];
      if (ShouldSkipValueProcessing(settings, value))
         return;

      if (SavingMethod == null)
      {
         if (ValueType == SavingValueType.IAgs)
         {
            HandleIAgsProperty((IAgs)value, sb, commentChar);
            return;
         }

         if (IsCollection)
            HandleCollection(ags, sb, commentChar, settings.Format);
         else if (ValueType is SavingValueType.FlagsEnum or SavingValueType.Enum)
            HandleEnumProperty(ags, sb, value);
         else
            HandlePrimitiveProperty(ags, sb);
      }
      else
         SavingMethod(ags, this, sb);
   }

   private bool ShouldSkipValueProcessing(AgsSettings settings, object value)
   {
      if (Config.Settings.AgsConfig.WriteAllDefaultValues)
         return false;

      if (!settings.SkipDefaultValues)
         return false;

      if (ValueType is SavingValueType.FlagsEnum or SavingValueType.Enum)
      {
         if (DefaultValue != null && Convert.ToInt64(DefaultValue).Equals(Convert.ToInt64(value)))
            return true;
      }
      else
      {
         if (DefaultValue == null && value == null!)
            return true;
         if (DefaultValue != null && DefaultValue.Equals(value))
            return true;
      }

      return false;
   }

   private void HandleEnumProperty(IAgs ags, IndentedStringBuilder sb, object value)
   {
      Debug.Assert(value is Enum, "Property is not an Enum");

      if (!EnumAgsRegistry.Registry.TryGetValue(value.GetType(), out var data))
         throw
            new InvalidOperationException($"Enum type '{value.GetType()}' is not registered in the EnumAgsRegistry.");

      if (data.IsFlags)
      {
         foreach (var sv in ((Enum)value).GetSetAtomicFlagNames())
         {
            if (!data.Mapping.TryGetValue(sv, out var stringRep))
               return;

            sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {stringRep}");
         }
      }
      else
      {
         // If it is not contained in the mapping it is meant to be ignored as it is a default or code-only value
         if (!data.Mapping.TryGetValue(value.ToString()!, out var stringRep))
            return;

         sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {stringRep}");
      }
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
         if (ags.AgsSettings.WriteEmptyCollectionHeader && ValueType == SavingValueType.Auto)
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