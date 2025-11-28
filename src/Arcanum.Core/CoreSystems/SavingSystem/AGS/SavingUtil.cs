using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingUtil
{
   public static IndentedStringBuilder FormatFilesMultithreadedIf(List<IEu5Object> items, int minComplexity = 3)
   {
      // We clear the cache to ensure that property order is recalculated for each run.

      PropertyOrderCache.Clear();

      if (minComplexity > items.Count)
      {
         var sb = new IndentedStringBuilder();
         foreach (var item in items)
            item.ToAgsContext().BuildContext(sb);
         return sb;
      }

      var partitioner = Partitioner.Create(0, items.Count);

      var chunkResults = new ConcurrentDictionary<long, IndentedStringBuilder>();

      Parallel.ForEach(partitioner,
                       (range, _, partitionIndex) =>
                       {
                          var localSb = new IndentedStringBuilder();

                          for (var i = range.Item1; i < range.Item2; i++)
                             items[i].ToAgsContext().BuildContext(localSb);

                          chunkResults.TryAdd(partitionIndex, localSb);
                       });

      var orderedResults = chunkResults.OrderBy(kvp => kvp.Key).ToList();
      var totalCapacity = orderedResults.Sum(kvp => kvp.Value.InnerBuilder.Length);
      var finalBuilder = new IndentedStringBuilder(totalCapacity);

      foreach (var kvp in orderedResults)
         kvp.Value.Merge(finalBuilder);

      return finalBuilder;
   }

   public static StringBuilder AppendInjectionType(this StringBuilder sb, InjRepType type)
   {
      return sb.Append(FormatInjectionType(type));
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
   /// Estimates the depth of the <see cref="IAgs"/> object graph for this object type.
   /// This method is safe against cyclical references.
   /// The depth is determined by the maximum depth of any unique <see cref="IAgs"/> properties it contains.
   /// </summary>
   /// <param name="ags">The root object to start the estimation from.</param>
   /// <returns>The calculated complexity depth.</returns>
   public static int EstimateObjectComplexity(this IAgs ags)
   {
      if (ags == null!)
         return 0;

      // Use a HashSet with a reference comparer to track visited *instances* in the current path.
      var visited = new HashSet<IAgs>(ReferenceEqualityComparer.Instance);
      return EstimateObjectComplexityRecursive(ags, visited);
   }

   /// <summary>
   /// The recursive helper method that performs the depth calculation.
   /// </summary>
   /// <param name="ags">The current object being evaluated.</param>
   /// <param name="visited">The set of objects already visited in this recursion path.</param>
   /// <returns>The complexity depth from this node downwards.</returns>
   private static int EstimateObjectComplexityRecursive(IAgs ags, HashSet<IAgs> visited)
   {
      if (!visited.Add(ags))
         return 0;

      var maxChildDepth = 0;
      foreach (var prop in ags.SaveableProps)
      {
         object? value = null;
         Nx.ForceGet(ags, prop.NxProp, ref value);

         if (value is IAgs nestedAgs)
         {
            var depth = EstimateObjectComplexityRecursive(nestedAgs, visited);
            if (depth > maxChildDepth)
               maxChildDepth = depth;
         }

         else if (value is IEnumerable enumerable and not string)
         {
            foreach (var item in enumerable)
               if (item is IAgs itemAgs)
               {
                  var depth = EstimateObjectComplexityRecursive(itemAgs, visited);
                  if (depth > maxChildDepth)
                     maxChildDepth = depth;
                  break;
               }
         }
      }

      visited.Remove(ags);

      return maxChildDepth + 1;
   }

   /// <summary>
   /// Formats a value according to the specified SavingValueType.
   /// </summary>
   public static string FormatValue(SavingValueType svl, object value, PropertySavingMetadata? psm)
   {
      if (svl == SavingValueType.Auto)
         svl = GetSavingValueType(value);

      switch (svl)
      {
         case SavingValueType.String:
            return $"\"{value}\"";
         case SavingValueType.Int:
            return ((int)value).ToString();
         case SavingValueType.Float:
            Debug.Assert(psm != null, "PropertySavingMetadata must be provided for float formatting.");
            return ((float)value).ToString($"F{psm.NumOfDecimalPlaces}", CultureInfo.InvariantCulture);
         case SavingValueType.Bool:
            return (bool)value ? "yes" : "no";
         case SavingValueType.Double:
            Debug.Assert(psm != null, "PropertySavingMetadata must be provided for double formatting.");
            return ((double)value).ToString($"F{psm.NumOfDecimalPlaces}", CultureInfo.InvariantCulture);
         case SavingValueType.Identifier:
            return $"{value}";
         case SavingValueType.Color:
            return ((JominiColor)value).ToString();
         case SavingValueType.Auto:
            throw new InvalidOperationException("SavingValueType cannot be Auto at this point.");
         case SavingValueType.Enum:
            return value.ToString()!;
         case SavingValueType.IAgs:
            throw new InvalidOperationException("IAgs type needs to be handled in advance");
         case SavingValueType.Modifier:
            if (value is ModValInstance mvi)
               return $"{mvi.UniqueId} = {mvi.FormatModifierPatternToCode()}";

            var instance = (IModifierPattern)value;
            return $"{instance.UniqueId} = {instance.FormatModifierPatternToCode()}";
         default:
            throw new ArgumentOutOfRangeException(nameof(svl), svl, null);
      }
   }

   /// <summary>
   /// Formats a property value from a Nexus object according to the specified SavingValueType.
   /// </summary>
   public static string FormatValue(SavingValueType svl, IEu5Object target, Enum nxProp)
   {
      PropertySavingMetadata? psm = null;
      if (!nxProp.ToString().EndsWith("UniqueId"))
      {
         psm = target.SaveableProps.FirstOrDefault(psm => psm.NxProp.Equals(nxProp));
         Debug.Assert(psm != null,
                      $"PropertySavingMetadata for property {nxProp} not found in {target.GetType().Name}.");
      }

      return FormatValue(svl, target._getValue(nxProp), psm);
   }

   /// <summary>
   /// Gets the string representation of a separator token.
   /// </summary>
   /// <param name="separator"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public static string GetSeparator(TokenType separator)
   {
      return separator switch
      {
         TokenType.LeftBrace
         or TokenType.RightBrace
         or TokenType.RightBracket
         or TokenType.Plus
         or TokenType.Minus
         or TokenType.Multiply
         or TokenType.Divide
         or TokenType.Less
         or TokenType.LeftBracket
         or TokenType.Identifier
         or TokenType.AtIdentifier
         or TokenType.String
         or TokenType.Number
         or TokenType.Date
         or TokenType.Yes
         or TokenType.No
         or TokenType.Comment
         or TokenType.EndOfFile
         or TokenType.Unexpected => throw new ArgumentOutOfRangeException(nameof(separator), separator, null),
         TokenType.Equals => "=",
         TokenType.Greater => ">",
         TokenType.GreaterOrEqual => ">=",
         TokenType.QuestionEquals => "?=",
         TokenType.LessOrEqual => "<=",
         _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null),
      };
   }

   /// <summary>
   /// Gets the string representation of a brace token.
   /// </summary>
   /// <param name="separator"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public static string GetBrace(TokenType separator)
   {
      return separator switch
      {
         TokenType.LeftBrace => "{",
         TokenType.RightBrace => "}",
         TokenType.LeftBracket => "[",
         TokenType.RightBracket => "]",
         _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null),
      };
   }

   /// <summary>
   /// Determines the SavingValueType for a given object instance.
   /// </summary>
   /// <param name="item"></param>
   /// <returns></returns>
   /// <exception cref="NotSupportedException"></exception>
   public static SavingValueType GetSavingValueType(object item)
   {
      var type = item.GetType();
      if (type == typeof(string))
         return SavingValueType.String;
      if (type == typeof(int))
         return SavingValueType.Int;
      if (type == typeof(float))
         return SavingValueType.Float;
      if (type == typeof(double))
         return SavingValueType.Double;
      if (type == typeof(bool))
         return SavingValueType.Bool;
      if (item is JominiColor)
         return SavingValueType.Color;
      if (type.IsEnum)
         return SavingValueType.Enum;
      if (item is IAgs)
         return SavingValueType.IAgs;
      if (item is IEnumerable enumerable)
         return GetSavingValueTypeForCollection(enumerable);
      if (item is IModifierPattern)
         return SavingValueType.Modifier;

      return SavingValueType.String;
      //throw new NotSupportedException($"Type {type} is not supported as item key type. Is it not defined as an IAgs?");
   }

   /// <summary>
   /// Determines the SavingValueType for the first non-null item in a collection. <br/>
   /// If the collection is empty or all items are null, returns SavingValueType.Auto.
   /// As if a collection is empty we do not necessarily need to know the type of its items.
   /// </summary>
   /// <param name="enumerable"></param>
   /// <returns></returns>
   public static SavingValueType GetSavingValueTypeForCollection(IEnumerable enumerable)
   {
      var enumerator = enumerable.GetEnumerator();
      using var enumerator1 = enumerator as IDisposable;
      if (!enumerator.MoveNext() || enumerator.Current == null)
         return SavingValueType.Auto;

      return GetSavingValueType(enumerator.Current);
   }
}