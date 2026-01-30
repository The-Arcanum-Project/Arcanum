#region

using System.Collections;
using System.Diagnostics;
using Arcanum.Core.AgsRegistry;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils;

#endregion

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization;

public static class FormattingService
{
   public static string? FormatBlockNameWithInjection(IEu5Object target, bool isArray)
   {
      if (isArray)
         return null;

      var key = target.SavingKey;
      if (target.InjRepType != InjRepType.None)
         key = $"{FormatInjectionType(target.InjRepType)}:{key}";
      return key;
   }

   public static string FormatInjectionType(InjRepType type)
   {
      return type switch
      {
         InjRepType.INJECT => "INJECT",
         InjRepType.TRY_INJECT => "TRY_INJECT",
         InjRepType.INJECT_OR_CREATE => "INJECT_OR_CREATE",
         InjRepType.REPLACE => "REPLACE",
         InjRepType.TRY_REPLACE => "TRY_REPLACE",
         InjRepType.REPLACE_OR_CREATE => "REPLACE_OR_CREATE",
         _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
      };
   }

   /// <summary>
   ///    Formats the property and appends it to the provided IndentedStringBuilder. <br />
   ///    Handles comments, collections, and custom saving methods as needed. <br />
   ///    Properties are only saved if they differ from their default values or are required.
   /// </summary>
   public static void Format(PropertySavingMetadata meta,
                             IndentedStringBuilder sb,
                             IEu5Object ags,
                             string commentChar,
                             bool asOneLine,
                             object value,
                             bool alwaysSerializeAll = false)
   {
#if DEBUG
      if (ShouldSkipCheck(meta, ags, value, alwaysSerializeAll))
      {
         ArcLog.WriteLine("FMS", LogLevel.ERR, "Skipping property '{0}' during serialization should have been handled beforehand!", meta.NxProp);
         return;
      }

      if (meta.ValueType == SavingValueType.Auto)
      {
         AssignValueType(meta, value);
         ArcLog.WriteLine("FMS", LogLevel.ERR, "Assigned ValueType '{0}' to {1} when it should already be assigned!", meta.ValueType, meta.NxProp);
      }
#endif

      if (meta.SavingMethod == null)
      {
         // Nested, single objects
         if (meta is { ValueType: SavingValueType.IAgs, IsCollection: false } && value is IEu5Object agsValue)
         {
            // SavingUtil.HandleIAgsProperty((IAgs)value, sb, commentChar, asOneLine, meta);
            var sm = agsValue.ClassMetadata.SavingMethod;
            if (sm != null)
            {
               sm.Invoke(agsValue, [meta], sb, asOneLine);
               return;
            }

            if (meta.SaveEmbeddedAsIdentifier)
            {
               sb.Append(meta.Keyword)
                 .AppendSeparator(SavingUtil.GetSeparator(meta.Separator))
                 .Append(agsValue.SavingKey);
               if (asOneLine)
                  sb.AppendSpacer();
            }
            else
            {
               var node = TreeBuilder.Construct(agsValue, meta.IsArray, meta);
               node.Write(sb, ref commentChar, asOneLine);
            }

            return;
         }

         // Collections
         if (meta.IsCollection)
            HandleCollectionSerialization(meta, sb, ags, commentChar, value);
         // Enums
         else if (meta.ValueType is SavingValueType.FlagsEnum or SavingValueType.Enum)
            HandleEnumProperty(meta, sb, value, asOneLine);
         // Simple values (int, float, string, bool, color, date, identifier, etc.)
         else
         {
            sb.Append(meta.Keyword)
              .AppendSpacer()
              .Append(SavingUtil.GetSeparator(meta.Separator))
              .AppendSpacer()
              .Append(SavingUtil.FormatValue(meta.ValueType, value, meta));
         }
      }
      // Custom saving
      else
         meta.SavingMethod(ags, meta, sb, asOneLine);
   }

   internal static void AssignValueType(PropertySavingMetadata meta, object value)
   {
      if (meta.ValueType == SavingValueType.Auto)
         meta.ValueType = SavingUtil.GetSavingValueType(value);
   }

   internal static void HandleCollectionSerialization(PropertySavingMetadata meta, IndentedStringBuilder sb, IEu5Object ags, string commentChar, object value)
   {
      if (value is IEnumerable collection)
         if (meta.IsShattered)
            HandleShatteredCollection(meta, sb, commentChar, ags, meta.CollectionSeparator, meta.ValueType, collection);
         else
            HandleCollection(ags, meta, sb, commentChar, meta.CollectionSeparator, collection);
      else
         throw new
            InvalidOperationException($"Property '{meta.NxProp}' is marked as a collection but the value is not IEnumerable (actual type: '{value.GetType().Name}').");
   }

   internal static bool ShouldSkipCheck(PropertySavingMetadata meta, IEu5Object ags, object value, bool alwaysSerializeAll)
   {
      // Required fields must always be saved
      if (!meta.AlwaysWrite && !Config.Settings.SavingConfig.WriteAllDefaultValues)
      {
         if ((meta.MustNotBeWritten != null && meta.MustNotBeWritten(ags)) ||
             (!alwaysSerializeAll && ShouldSkipValueProcessing(meta, ags.AgsSettings, value) && !ags.IsRequired(meta.NxProp)))
            return true;
      }
      else
         return !IsWriteableDefaultValue(value);

      return false;
   }

   // We can only serialize certain default values directly, others need special handling and thus can not be written by default
   private static bool IsWriteableDefaultValue(object value) => value is float or int or double or bool or Enum;

   public static bool ShouldSkipValueProcessing(PropertySavingMetadata meta, AgsSettings settings, object value)
   {
      if (!settings.SkipDefaultValues)
         return false;

      switch (meta.ValueType)
      {
         case SavingValueType.FlagsEnum or SavingValueType.Enum:
         {
            if (meta.DefaultValue != null && Convert.ToInt64(meta.DefaultValue).Equals(Convert.ToInt64(value)))
               return true;

            break;
         }
         case SavingValueType.Float when meta.DefaultValue == null && value == null!:
         case SavingValueType.Float when meta.DefaultValue != null &&
                                         Math.Abs(Convert.ToDouble(meta.DefaultValue) - Convert.ToDouble(value)) < 0.0001:
            return true;
         case SavingValueType.Float:
            break;
         default:
         {
            if (meta.DefaultValue == null && value == null!)
               return true;
            if ((meta.DefaultValue != null && meta.DefaultValue.Equals(value)) || (value is string str && string.IsNullOrEmpty(str)))
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

   public static void HandleEnumProperty(PropertySavingMetadata meta, IndentedStringBuilder sb, object value, bool isOneLine)
   {
      Debug.Assert(value is Enum, "Property is not an Enum");

      if (!EnumAgsRegistry.Registry.TryGetValue(value.GetType(), out var data))
         throw
            new InvalidOperationException($"Enum type '{value.GetType()}' is not registered in the EnumAgsRegistry.");

      if (data.IsFlags)
         foreach (var sv in ((Enum)value).GetSetAtomicFlagNames())
         {
            if (!data.Mapping.TryGetValue(sv, out var stringRep))
               return;

            sb.Append(meta.Keyword)
              .AppendSeparator(SavingUtil.GetSeparator(meta.Separator))
              .Append(stringRep);
            if (isOneLine)
               sb.AppendSpacer();
         }
      else
      {
         if (!data.Mapping.TryGetValue(value.ToString()!, out var stringRep))
            return;

         sb.Append(meta.Keyword)
           .AppendSeparator(SavingUtil.GetSeparator(meta.Separator))
           .Append(stringRep);
      }
   }

   public static void HandleShatteredCollection(PropertySavingMetadata meta,
                                                IndentedStringBuilder sb,
                                                string commentChar,
                                                IAgs ags,
                                                string collectionSeparator,
                                                SavingValueType svt,
                                                IEnumerable collection)
   {
      if (!collection.HasItems())
         return;

      if (meta.CollectionAsPureIdentifierList)
      {
         var maxItemsPerLine = sb.MaxItemsInCollectionLine;
         sb.MaxItemsInCollectionLine = 1;
         FormatAsIdentifierList(sb, collection, collectionSeparator, ags.AgsSettings.GetCollectionProfile(meta.NxProp));
         sb.MaxItemsInCollectionLine = maxItemsPerLine;
      }
      else if (meta.IsEmbeddedObject)
         foreach (var item in collection)
            if (item is IEu5Object ia)
            {
               if (meta.SaveEmbeddedAsIdentifier)
                  sb.Append(meta.Keyword);
               var node = TreeBuilder.Construct(ia, meta.IsArray, meta);
               node.Write(sb, ref commentChar, ia.AgsSettings.AsOneLine);
               sb.AppendLine();
            }
            else
               throw new
                  InvalidOperationException($"Collection property '{meta.NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");
      else
      {
         if (ags.AgsSettings.Format == SavingFormat.Spacious)
            sb.AppendLine();

         var startLength = sb.InnerBuilder.Length;
         foreach (var item in collection)
         {
            if (startLength != sb.InnerBuilder.Length)
               sb.AppendLine();
            if (svt == SavingValueType.Auto)
               svt = SavingUtil.GetSavingValueType(item);

            sb.Append(meta.Keyword)
              .AppendSpacer()
              .Append(SavingUtil.GetSeparator(meta.Separator))
              .AppendSpacer();

            if (svt == SavingValueType.IAgs && item is IAgs ia)
               sb.AppendLine(ia.SavingKey);
            else
               sb.Append(SavingUtil.FormatValue(svt, item, meta));
         }
      }
   }

   public static void HandleCollection(IAgs ags,
                                       PropertySavingMetadata meta,
                                       IndentedStringBuilder sb,
                                       string commentChar,
                                       string collectionSeparator,
                                       IEnumerable collection)
   {
      var internalCollection = collection.Cast<object>().ToList();
      if (internalCollection.Count == 0 && !ags.AgsSettings.WriteEmptyCollectionHeader)
         return;

      if (ags.AgsSettings.Format == SavingFormat.Spacious)
         sb.AppendLine();

      var collectionProfile = ags.AgsSettings.GetCollectionProfile(meta.NxProp);

      if (meta.CollectionAsPureIdentifierList)
         FormatAsIdentifierList(sb, internalCollection, collectionSeparator, collectionProfile);
      else if (meta.IsEmbeddedObject || meta.ValueType == SavingValueType.IAgs)
         FormatAsEmbeddedObjectList(meta, sb, internalCollection, commentChar);
      else
         FormatAsValueList(meta, sb, internalCollection, collectionSeparator, collectionProfile);

      if (ags.AgsSettings.Format == SavingFormat.Spacious)
         sb.AppendLine();
   }

   public static void FormatAsIdentifierList(IndentedStringBuilder sb,
                                             IEnumerable collection,
                                             string separator,
                                             CollectionFormatProfile profile)
   {
      var query = collection.OfType<IAgs>().Select(x => x.SavingKey);

      if (profile.SortMode == CollectionSortMode.Alphabetical)
         query = query.OrderBy(x => x);
      else if (profile.SortMode == CollectionSortMode.Numeric)
         // Simple length-then-value sort is decent
         query = query.OrderBy(x => x.Length).ThenBy(x => x);

      var itemList = query.ToList();
      if (itemList.Count == 0)
         return;

      // Constraints based on LayoutMode
      var isGrid = profile.LayoutMode == CollectionLayoutMode.Grid;
      var isVertical = profile.LayoutMode == CollectionLayoutMode.Vertical;
      var isCompact = profile.LayoutMode == CollectionLayoutMode.Compact;

      // Vertical = 1 per row. Compact = Infinite. Grid/Flow = User Setting.
      var maxItemsPerLine = isVertical
                               ? 1
                               : isCompact
                                  ? int.MaxValue
                                  : profile.ItemsPerRow;

      // Compact = Infinite length. Others = SB Setting.
      var maxLineLength = isCompact ? int.MaxValue : sb.MaxCollectionLineLength;

      // Setup Padding
      var paddingWidth = 0;
      if (profile.AlignColumns && !isVertical && !isCompact)
      {
         // Find longest item to ensure strict columns
         var maxLen = itemList.Max(s => s.Length);
         paddingWidth = Math.Max(maxLen, profile.ColumnWidth) + Config.Settings.SavingConfig.SpacesPerSpacing;
      }

      var lineItemCount = 0;
      var currentLineStartPos = sb.InnerBuilder.Length;
      var isFirstItem = true;

      foreach (var item in itemList)
      {
         // Determine if we need to break to a new line
         var lengthExceeded = sb.InnerBuilder.Length - currentLineStartPos + item.Length + separator.Length > maxLineLength;
         var countExceeded = lineItemCount >= maxItemsPerLine;

         // Logic:
         // - Vertical: Always break (handled by countExceeded = 1)
         // - Grid: Strictly break on count. Ignore length
         // - Flow: Break on Count OR Length.
         // - Compact: Never break (handled by MaxValue settings)

         var needsLineBreak = !isFirstItem && (isGrid ? countExceeded : lengthExceeded || countExceeded);

         if (needsLineBreak)
         {
            sb.AppendLine();
            currentLineStartPos = sb.InnerBuilder.Length;
            lineItemCount = 0;
         }

         // Append Separator (only if we didn't just newline, OR if separator is not a space)
         // Typically identifiers are space separated. If we just did a newline, we don't need a space.
         if (!isFirstItem && lineItemCount > 0)
            sb.Append(separator);

         sb.Append(item);

         // Apply Column Alignment Padding
         if (paddingWidth > 0)
         {
            var spaces = paddingWidth - item.Length;
            if (spaces > 0)
               sb.InnerBuilder.Append(' ', spaces);
         }

         lineItemCount++;
         isFirstItem = false;
      }

      sb.AppendLine();
   }

   public static void FormatAsValueList(PropertySavingMetadata meta,
                                        IndentedStringBuilder sb,
                                        IEnumerable collection,
                                        string separator,
                                        CollectionFormatProfile profile)
   {
      var stringValues = new List<string>();
      foreach (var item in collection)
      {
         var val = meta.CollectionItemKeyProvider != null
                      ? meta.CollectionItemKeyProvider(item)
                      : SavingUtil.FormatValue(meta.ValueType, item, meta);
         stringValues.Add(val);
      }

      if (stringValues.Count == 0)
         return;

      // Sorting
      if (profile.SortMode == CollectionSortMode.Alphabetical)
         stringValues.Sort(StringComparer.OrdinalIgnoreCase);
      else if (profile.SortMode == CollectionSortMode.Numeric)
         stringValues.Sort((a, b) =>
         {
            var lenCmp = a.Length.CompareTo(b.Length);
            return lenCmp != 0 ? lenCmp : string.Compare(a, b, StringComparison.Ordinal);
         });

      // Setup Constraints
      var isGrid = profile.LayoutMode == CollectionLayoutMode.Grid;
      var isVertical = profile.LayoutMode == CollectionLayoutMode.Vertical;
      var isCompact = profile.LayoutMode == CollectionLayoutMode.Compact;

      var maxItemsPerLine = isVertical
                               ? 1
                               : isCompact
                                  ? int.MaxValue
                                  : profile.ItemsPerRow;

      var maxLineLength = isCompact ? int.MaxValue : sb.MaxCollectionLineLength;

      // Setup Padding
      var paddingWidth = 0;
      if (profile.AlignColumns && !isVertical && !isCompact)
      {
         var maxLen = 0;
         foreach (var s in stringValues)
            if (s.Length > maxLen)
               maxLen = s.Length;

         // Use user's ColumnWidth as minimum
         paddingWidth = Math.Max(maxLen, profile.ColumnWidth) + Config.Settings.SavingConfig.SpacesPerSpacing;
      }

      // Execution Loop
      var lineItemCount = 0;
      var currentLineStartPos = sb.InnerBuilder.Length;
      var isFirstItem = true;

      foreach (var valueToAppend in stringValues)
      {
         // Calculate break condition
         var lengthExceeded = sb.InnerBuilder.Length - currentLineStartPos + valueToAppend.Length + separator.Length > maxLineLength;
         var countExceeded = lineItemCount >= maxItemsPerLine;

         // Grid mode ignores length constraints to enforce visual grid
         var needsLineBreak = !isFirstItem && (isGrid ? countExceeded : lengthExceeded || countExceeded);

         if (needsLineBreak)
         {
            sb.AppendLine();
            currentLineStartPos = sb.InnerBuilder.Length;
            lineItemCount = 0;
         }

         if (!isFirstItem && lineItemCount > 0)
            sb.Append(separator);

         sb.Append(valueToAppend);

         if (paddingWidth > 0)
         {
            var padCount = paddingWidth - valueToAppend.Length;
            if (padCount > 0)
               sb.InnerBuilder.Append(' ', padCount);
         }

         lineItemCount++;
         isFirstItem = false;
      }

      sb.AppendLine();
   }

   public static void FormatAsEmbeddedObjectList(PropertySavingMetadata meta, IndentedStringBuilder sb, IEnumerable collection, string commentChar)
   {
      foreach (var item in collection)
      {
         if (item is not IEu5Object eu5Obj)
            throw new
               InvalidOperationException($"Collection property '{meta.NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");

         var node = TreeBuilder.Construct(eu5Obj, meta.IsArray, meta);
         node.Write(sb, ref commentChar, eu5Obj.AgsSettings.AsOneLine);
         sb.AppendLine();
      }
   }
}