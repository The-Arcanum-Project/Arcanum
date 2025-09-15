using System.Collections;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingUtil
{
   public static string FormatValue(SavingValueType svl, object value)
   {
      if (svl == SavingValueType.Auto)
         svl = GetSavingValueType(value);

      return svl switch
      {
         SavingValueType.String => $"\"{value}\"",
         SavingValueType.Int => ((int)value).ToString(),
         SavingValueType.Float => ((float)value).ToString("0.##"),
         SavingValueType.Bool => (bool)value ? "yes" : "no",
         SavingValueType.Double => ((double)value).ToString("0.##"),
         SavingValueType.Identifier => $"{value}",
         SavingValueType.Color => ((JominiColor)value).ToString(),
         SavingValueType.Auto => throw new InvalidOperationException("SavingValueType cannot be Auto at this point."),
         _ => throw new ArgumentOutOfRangeException(nameof(svl), svl, null),
      };
   }

   public static string FormatObjectValue(SavingValueType svl, INexus target, Enum nxProp)
   {
      object value = null!;
      Nx.ForceGet(target, nxProp, ref value);
      return FormatValue(svl, value);
   }

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
      if (type == typeof(JominiColor))
         return SavingValueType.Color;

      throw new NotSupportedException($"Type {type} is not supported as item key type.");
   }

   public static SavingValueType GetSavingValueTypeForCollection(IEnumerable enumerable)
   {
      var enumerator = enumerable.GetEnumerator();
      using var enumerator1 = enumerator as IDisposable;
      if (!enumerator.MoveNext() || enumerator.Current == null)
         return SavingValueType.Auto;

      return GetSavingValueType(enumerator.Current);
   }
}