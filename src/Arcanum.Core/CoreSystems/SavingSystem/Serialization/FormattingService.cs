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
         if (meta is { ValueType: SavingValueType.IAgs, IsCollection: false })
         {
            SavingUtil.HandleIAgsProperty((IAgs)value, sb, commentChar, asOneLine, meta);
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
              .Append(' ')
              .Append(SavingUtil.GetSeparator(meta.Separator))
              .Append(' ')
              .Append(SavingUtil.FormatValue(meta.ValueType, value, meta));

            if (asOneLine)
               sb.AppendLine();
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
            HandleShatteredCollection(meta, sb, commentChar, ags.AgsSettings.Format, meta.CollectionSeparator, meta.ValueType, collection);
         else
            HandleCollection(ags, meta, sb, commentChar, ags.AgsSettings.Format, meta.CollectionSeparator, collection);
      else
         throw new
            InvalidOperationException($"Property '{meta.NxProp}' is marked as a collection but the value is not IEnumerable (actual type: '{value.GetType().Name}').");
   }

   internal static bool ShouldSkipCheck(PropertySavingMetadata meta, IEu5Object ags, object value, bool alwaysSerializeAll)
   {
      // Required fields must always be saved
      if (!meta.AlwaysWrite)
         if ((meta.MustNotBeWritten != null && meta.MustNotBeWritten(ags)) ||
             (!alwaysSerializeAll && ShouldSkipValueProcessing(meta, ags.AgsSettings, value) && !ags.IsRequired(meta.NxProp)))
            return true;

      return false;
   }

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

            var str = $"{meta.Keyword} {SavingUtil.GetSeparator(meta.Separator)} {stringRep}";
            if (isOneLine)
               sb.Append(str + " ");
            else
               sb.AppendLine(str);
         }
      else
      {
         if (!data.Mapping.TryGetValue(value.ToString()!, out var stringRep))
            return;

         sb.AppendLine($"{meta.Keyword} {SavingUtil.GetSeparator(meta.Separator)} {stringRep}");
      }
   }

   public static void HandleShatteredCollection(PropertySavingMetadata meta,
                                                IndentedStringBuilder sb,
                                                string commentChar,
                                                SavingFormat format,
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
         FormatAsIdentifierList(sb, collection, collectionSeparator);
         sb.MaxItemsInCollectionLine = maxItemsPerLine;
      }
      else if (meta.IsEmbeddedObject)
         foreach (var item in collection)
            if (item is IAgs ia)
            {
               if (meta.SaveEmbeddedAsIdentifier)
                  sb.Append($"{meta.Keyword}");
               ia.ToAgsContext(commentChar).BuildContext(sb);
            }
            else
               throw new
                  InvalidOperationException($"Collection property '{meta.NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");
      else
      {
         if (!collection.HasItems())
            return;

         if (format == SavingFormat.Spacious)
            sb.AppendLine();

         foreach (var item in collection)
         {
            if (svt == SavingValueType.Auto)
               svt = SavingUtil.GetSavingValueType(item);
            if (svt == SavingValueType.IAgs && item is IAgs ia)
               sb.AppendLine($"{meta.Keyword} {SavingUtil.GetSeparator(meta.Separator)} {ia.SavingKey}");
            else
               sb.AppendLine($"{meta.Keyword} {SavingUtil.GetSeparator(meta.Separator)} {SavingUtil.FormatValue(svt, item, meta)}");
         }
      }
   }

   public static void HandleCollection(IAgs ags,
                                       PropertySavingMetadata meta,
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

      if (meta.CollectionAsPureIdentifierList)
         FormatAsIdentifierList(sb, collection, collectionSeparator);
      else if (meta.IsEmbeddedObject || meta.ValueType == SavingValueType.IAgs)
         FormatAsEmbeddedObjectList(meta, sb, collection, commentChar);
      else
         FormatAsValueList(meta, sb, collection, collectionSeparator);

      if (format == SavingFormat.Spacious)
         sb.AppendLine();
   }

   public static void FormatAsIdentifierList(IndentedStringBuilder sb, IEnumerable collection, string separator)
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

   public static void FormatAsValueList(PropertySavingMetadata meta, IndentedStringBuilder sb, IEnumerable collection, string separator)
   {
      var padding = 0;
      if (sb.PadCollectionItems)
      {
         if (sb.AutoCollectionPadding)
         {
            var maxLength = 0;
            foreach (var item in collection)
            {
               var value = meta.CollectionItemKeyProvider != null
                              ? meta.CollectionItemKeyProvider(item)
                              : SavingUtil.FormatValue(meta.ValueType, item, meta);
               if (value.Length > maxLength)
                  maxLength = value.Length;
            }

            padding = maxLength + 1;
         }
         else
            padding = sb.CollectionItemPadding;
      }

      var lineItemCount = 0;
      var currentLineStartPos = sb.InnerBuilder.Length;
      var isFirstItem = true;

      foreach (var item in collection)
      {
         var valueToAppend = meta.CollectionItemKeyProvider != null
                                ? meta.CollectionItemKeyProvider(item)
                                : SavingUtil.FormatValue(meta.ValueType, item, meta);

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

   public static void FormatAsEmbeddedObjectList(PropertySavingMetadata meta, IndentedStringBuilder sb, IEnumerable collection, string commentChar)
   {
      foreach (var item in collection)
         if (item is IAgs ia)
            ia.ToAgsContext(commentChar).BuildContext(sb, meta.IsArray);
         else
            throw new
               InvalidOperationException($"Collection property '{meta.NxProp}' contains non-IAgs item of type '{item?.GetType().Name ?? "null"}'.");
   }
}