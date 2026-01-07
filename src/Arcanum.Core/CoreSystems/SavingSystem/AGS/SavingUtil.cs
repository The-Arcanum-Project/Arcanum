using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
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
      return 1;
   }

   public static void AsOneLine(bool asOneLine, IndentedStringBuilder sb, string str)
   {
      if (asOneLine)
         sb.Append($"{str} ");
      else
         sb.AppendLine(str);
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
            if (psm == null)
               return ((float)value).ToString("F2", CultureInfo.InvariantCulture);

            return ((float)value).ToString($"F{psm.NumOfDecimalPlaces}", CultureInfo.InvariantCulture);
         case SavingValueType.Bool:
            return (bool)value ? "yes" : "no";
         case SavingValueType.Double:
            if (psm == null)
               return ((double)value).ToString("F2", CultureInfo.InvariantCulture);

            return ((double)value).ToString($"F{psm.NumOfDecimalPlaces}", CultureInfo.InvariantCulture);
         case SavingValueType.Identifier:
            if (value is IEu5Object eu5Obj)
            {
               Debug.Assert(!string.IsNullOrWhiteSpace(eu5Obj.UniqueId));
               return eu5Obj.UniqueId;
            }

            return $"{value}";
         case SavingValueType.Color:
            return ((JominiColor)value).ToString();
         case SavingValueType.Auto:
            if (value is Vector2 vec2)
               return FormatVec2ToCode(vec2);
            if (value is Vector3 vec3)
               return FormatVec3ToCode(vec3);
            if (value is Vector4 vec4)
               return FormatVec4ToCode(vec4);
            if (value is Quaternion quat)
               return FormatQuaternionToCode(quat);

            throw new InvalidOperationException("Auto type could not be resolved to a known complex type.");
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

   public static string FormatVec2ToCode(Vector2 vec)
      => $"{{ {vec.X.ToString("F6", CultureInfo.InvariantCulture)} {vec.Y.ToString("F6", CultureInfo.InvariantCulture)}";

   public static string FormatVec3ToCode(Vector3 vec)
      => $"{{ {vec.X.ToString("F6", CultureInfo.InvariantCulture)} {vec.Y.ToString("F6", CultureInfo.InvariantCulture)} {vec.Z.ToString("F6", CultureInfo.InvariantCulture)}";

   public static string FormatVec4ToCode(Vector4 vec)
      => $"{{ {vec.X.ToString("F6", CultureInfo.InvariantCulture)} {vec.Y.ToString("F6", CultureInfo.InvariantCulture)} {vec.Z.ToString("F6", CultureInfo.InvariantCulture)} {vec.W.ToString("F6", CultureInfo.InvariantCulture)} }}";

   public static string FormatQuaternionToCode(Quaternion quat)
      => $"{{ {quat.X.ToString("F6", CultureInfo.InvariantCulture)} {quat.Y.ToString("F6", CultureInfo.InvariantCulture)} {quat.Z.ToString("F6", CultureInfo.InvariantCulture)} {quat.W.ToString("F6", CultureInfo.InvariantCulture)} }}";

   /// <summary>
   /// Formats a property value from a Nexus object according to the specified SavingValueType.
   /// </summary>
   public static string FormatValue(SavingValueType svl, IEu5Object target, Enum nxProp)
   {
      PropertySavingMetadata? psm = null;
      if (!nxProp.ToString().EndsWith("UniqueId"))
      {
         psm = target.SaveableProps.FirstOrDefault(propertySavingMetadata => propertySavingMetadata.NxProp.Equals(nxProp));
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
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

      return SavingValueType.Auto;
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