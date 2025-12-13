using System.Collections;
using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils;

// ReSharper disable PossibleMultipleEnumeration

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
   public required bool IsCollection { get; init; }
   public required bool CollectionAsPureIdentifierList { get; init; }
   public required bool IsEmbeddedObject { get; init; }

   /// <summary>
   /// The separator to use between items in a collection when saving. <br/>
   /// Only relevant if IsCollection is true. Default is "".
   /// </summary>
   public required string CollectionSeparator { get; init; } = "";
   public required bool SaveEmbeddedAsIdentifier { get; init; } = true;
   /// <summary>
   /// If this list is shattered into multiple parts when saved. <br/>
   /// Only relevant if IsCollection is true. Default is false.
   /// </summary>
   public required bool IsShattered { get; init; }
   /// <summary>
   /// Number of decimal places to use when saving float or double values. <br/>
   /// Default is 2.
   /// </summary>
   public required int NumOfDecimalPlaces { get; init; }

   #region Equality operations

   public override string ToString() => $"{NxProp} as {Keyword} ({ValueType})";

   public override int GetHashCode()
   {
      return NxProp.GetHashCode();
   }

   protected bool Equals(PropertySavingMetadata other) => NxProp.Equals(other.NxProp);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((PropertySavingMetadata)obj);
   }

   #endregion

   /// <summary>
   /// Formats the property and appends it to the provided IndentedStringBuilder. <br/>
   /// Handles comments, collections, and custom saving methods as needed. <br/>
   /// Properties are only saved if they differ from their default values or are required.
   /// </summary>
   public void Format(IAgs ags,
                      IndentedStringBuilder sb,
                      bool asOneLine,
                      string commentChar,
                      AgsSettings settings,
                      bool alwaysSerializeAll = false)
   {
      if (CommentProvider != null)
         CommentProvider(ags, commentChar, sb);

      var value = ags._getValue(NxProp);

      if (ValueType == SavingValueType.Auto)
         ValueType = SavingUtil.GetSavingValueType(value);

      // Required fields must always be saved
      if (!alwaysSerializeAll && ShouldSkipValueProcessing(settings, value) && !ags.IsRequired(NxProp))
         return;

      if (SavingMethod == null)
      {
         if (ValueType == SavingValueType.IAgs && !IsCollection)
         {
            HandleIAgsProperty((IAgs)value, sb, commentChar, asOneLine);
            return;
         }

         if (IsCollection)
         {
            if (value is IEnumerable collection)
               if (IsShattered)
                  HandleShatteredCollection(sb, commentChar, settings.Format, CollectionSeparator, collection);
               else
                  HandleCollection(ags, sb, commentChar, settings.Format, CollectionSeparator, collection);
         }
         else if (ValueType is SavingValueType.FlagsEnum or SavingValueType.Enum)
         {
            HandleEnumProperty(sb, value, asOneLine);
         }
         else
         {
            if (asOneLine)
               sb.Append($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatValue(ValueType, value, this)} ");
            else
               sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatValue(ValueType, value, this)}");
         }
      }
      else
         SavingMethod(ags, this, sb, asOneLine);
   }

   private bool ShouldSkipValueProcessing(AgsSettings settings, object value)
   {
      if (Config.Settings.SavingConfig.WriteAllDefaultValues)
         return false;

      if (!settings.SkipDefaultValues)
         return false;

      switch (ValueType)
      {
         case SavingValueType.FlagsEnum or SavingValueType.Enum:
         {
            if (DefaultValue != null && Convert.ToInt64(DefaultValue).Equals(Convert.ToInt64(value)))
               return true;

            break;
         }
         case SavingValueType.Float when DefaultValue == null && value == null!:
         case SavingValueType.Float when DefaultValue != null &&
                                         Math.Abs(Convert.ToDouble(DefaultValue) - Convert.ToDouble(value)) < 0.0001:
            return true;
         case SavingValueType.Float:
            break;
         default:
         {
            if (DefaultValue == null && value == null!)
               return true;
            if ((DefaultValue != null && DefaultValue.Equals(value)) || (value is string str && string.IsNullOrEmpty(str)))
               return true;

            break;
         }
         case SavingValueType.IAgs:
         case SavingValueType.Identifier:
            if (value is IEu5Object eu5Obj)
            {
               var defaultValue = EmptyRegistry.Empties[eu5Obj.GetType()];
               if (eu5Obj.Equals(defaultValue))
                  return true;
            }
            else if (value is JominiDate date && date == JominiDate.Empty)
               return true;
            else if (value is string s && string.IsNullOrEmpty(s))
               return true;

            break;
      }

      return false;
   }

   private void HandleEnumProperty(IndentedStringBuilder sb, object value, bool isOneLine)
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

            var str = $"{Keyword} {SavingUtil.GetSeparator(Separator)} {stringRep}";
            if (isOneLine)
               sb.Append(str + " ");
            else
               sb.AppendLine(str);
         }
      }
      else
      {
         if (!data.Mapping.TryGetValue(value.ToString()!, out var stringRep))
            return;

         sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {stringRep}");
      }
   }

   private void HandleIAgsProperty(IAgs ags, IndentedStringBuilder sb, string commentChar, bool asOneLine)
   {
      var sm = ags.ClassMetadata.SavingMethod;
      if (sm != null)
      {
         sm.Invoke(ags, [this], sb, asOneLine);
         return;
      }

      if (SaveEmbeddedAsIdentifier)
      {
         var str = $"{Keyword} {SavingUtil.GetSeparator(Separator)} {ags.SavingKey}";
         if (asOneLine)
            sb.Append(str + " ");
         else
            sb.AppendLine(str);
      }
      else
         ags.ToAgsContext(commentChar).BuildContext(sb);
   }

   private void HandleShatteredCollection(IndentedStringBuilder sb,
                                          string commentChar,
                                          SavingFormat format,
                                          string collectionSeparator,
                                          IEnumerable collection)
   {
      if (!collection.HasItems())
         return;

      if (CollectionAsPureIdentifierList)
      {
         var maxItemsPerLine = sb.MaxItemsInCollectionLine;
         sb.MaxItemsInCollectionLine = 1;
         FormatAsIdentifierList(sb, collection, collectionSeparator);
         sb.MaxItemsInCollectionLine = maxItemsPerLine;
      }
      else if (IsEmbeddedObject)
      {
         foreach (var item in collection)
            if (item is IAgs ia)
            {
               if (SaveEmbeddedAsIdentifier)
                  sb.Append($"{Keyword}");
               ia.ToAgsContext(commentChar).BuildContext(sb);
            }
            else
               throw new
                  InvalidOperationException($"Collection property '{NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");
      }
      else
      {
         if (!collection.HasItems())
            return;

         if (format == SavingFormat.Spacious)
            sb.AppendLine();

         foreach (var item in collection)
         {
            var itemType = SavingUtil.GetSavingValueType(item);
            if (itemType == SavingValueType.IAgs && item is IAgs ia)
               sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {ia.SavingKey}");
            else
               sb.AppendLine($"{Keyword} {SavingUtil.GetSeparator(Separator)} {SavingUtil.FormatValue(itemType, item, this)}");
         }
      }
   }

   private void HandleCollection(IAgs ags,
                                 IndentedStringBuilder sb,
                                 string commentChar,
                                 SavingFormat format,
                                 string collectionSeparator,
                                 IEnumerable collection)
   {
      if (!collection.HasItems() && !ags.AgsSettings.WriteEmptyCollectionHeader)
         return;

      if (format == SavingFormat.Spacious)
         sb.AppendLine();

      if (CollectionAsPureIdentifierList)
         FormatAsIdentifierList(sb, collection, collectionSeparator);
      else if (IsEmbeddedObject)
         FormatAsEmbeddedObjectList(sb, collection, commentChar);
      else
         using (sb.BlockWithName(Keyword))
            if (ValueType == SavingValueType.IAgs)
               FormatAsEmbeddedObjectList(sb, collection, commentChar);
            else
               FormatAsValueList(sb, collection, collectionSeparator);

      if (format == SavingFormat.Spacious)
         sb.AppendLine();
   }

   private static void FormatAsIdentifierList(IndentedStringBuilder sb, IEnumerable collection, string separator)
   {
      var lineItemCount = 0;
      var currentLineStartPos = sb.InnerBuilder.Length;
      var isFirstItem = true;

      foreach (var item in collection)
      {
         if (item is not IAgs ia)
            continue;

         var savingKey = ia.SavingKey;

         var needsLineBreak = !isFirstItem &&
                              (sb.InnerBuilder.Length - currentLineStartPos + savingKey.Length + separator.Length >
                               sb.MaxCollectionLineLength ||
                               lineItemCount >= sb.MaxItemsInCollectionLine);

         if (needsLineBreak)
         {
            sb.AppendLine();
            currentLineStartPos = sb.InnerBuilder.Length;
            lineItemCount = 0;
         }

         if (!isFirstItem)
            sb.Append(separator);

         sb.Append(savingKey);
         lineItemCount++;
         isFirstItem = false;
      }

      sb.AppendLine();
   }

   private void FormatAsValueList(IndentedStringBuilder sb, IEnumerable collection, string separator)
   {
      var padding = 0;
      if (sb.PadCollectionItems)
      {
         if (sb.AutoCollectionPadding)
         {
            var maxLength = 0;
            foreach (var item in collection)
            {
               var value = CollectionItemKeyProvider != null
                              ? CollectionItemKeyProvider(item)
                              : SavingUtil.FormatValue(ValueType, item, this);
               if (value.Length > maxLength)
                  maxLength = value.Length;
            }

            padding = maxLength + 1;
         }
         else
         {
            padding = sb.CollectionItemPadding;
         }
      }

      var lineItemCount = 0;
      var currentLineStartPos = sb.InnerBuilder.Length;
      var isFirstItem = true;

      foreach (var item in collection)
      {
         var valueToAppend = CollectionItemKeyProvider != null
                                ? CollectionItemKeyProvider(item)
                                : SavingUtil.FormatValue(ValueType, item, this);

         var needsLineBreak = !isFirstItem &&
                              (sb.InnerBuilder.Length -
                               currentLineStartPos +
                               valueToAppend.Length +
                               separator.Length >
                               sb.MaxCollectionLineLength ||
                               lineItemCount >= sb.MaxItemsInCollectionLine);

         if (needsLineBreak)
         {
            sb.AppendLine();
            currentLineStartPos = sb.InnerBuilder.Length;
            lineItemCount = 0;
         }

         if (!isFirstItem)
            sb.Append(separator);

         sb.Append(valueToAppend);
         if (sb.PadCollectionItems)
         {
            var padCount = padding - valueToAppend.Length;
            if (padCount > 0)
               sb.InnerBuilder.Append(' ', padCount);
         }

         lineItemCount++;
         isFirstItem = false;
      }

      if (collection.Cast<object?>().Any())
         sb.AppendLine();
   }

   private void FormatAsEmbeddedObjectList(IndentedStringBuilder sb, IEnumerable collection, string commentChar)
   {
      foreach (var item in collection)
         if (item is IAgs ia)
            ia.ToAgsContext(commentChar).BuildContext(sb);
         else
            throw new
               InvalidOperationException($"Collection property '{NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");
   }
}