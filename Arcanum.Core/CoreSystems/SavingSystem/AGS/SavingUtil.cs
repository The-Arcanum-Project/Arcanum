using System.Collections;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingUtil
{
   /// <summary>
   /// Formats a value according to the specified SavingValueType.
   /// </summary>
   /// <param name="svl"></param>
   /// <param name="value"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   /// <exception cref="ArgumentOutOfRangeException"></exception>
   public static string FormatValue(SavingValueType svl, object value)
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
            return ((float)value).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
         case SavingValueType.Bool:
            return (bool)value ? "yes" : "no";
         case SavingValueType.Double:
            return ((double)value).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
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
            var instance = (IModifierPattern)value;
            return $"{instance.UniqueId} = {instance.FormatModifierPatternToCode()}";
         default:
            throw new ArgumentOutOfRangeException(nameof(svl), svl, null);
      }
   }

   /// <summary>
   /// Formats a property value from a Nexus object according to the specified SavingValueType.
   /// </summary>
   /// <param name="svl"></param>
   /// <param name="target"></param>
   /// <param name="nxProp"></param>
   /// <returns></returns>
   public static string FormatObjectValue(SavingValueType svl, INexus target, Enum nxProp)
   {
      object value = null!;
      Nx.ForceGet(target, nxProp, ref value);
      return FormatValue(svl, value);
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