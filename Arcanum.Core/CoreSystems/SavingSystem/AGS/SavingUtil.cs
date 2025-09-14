using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingUtil
{
   public static void SaveContentNxProperty(INexus target,
                                            Enum nxProp,
                                            string key,
                                            SavingValueType svl,
                                            ref IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, nxProp, ref value);

      AppendFormattedObject(svl, value, ref sb);
   }

   public static void AppendFormattedObject(SavingValueType svl, object value, ref IndentedStringBuilder sb)
   {
      switch (svl)
      {
         case SavingValueType.String:
            sb.Append($"\"{value}\"");
            break;
         case SavingValueType.Int:
            sb.Append(((int)value).ToString());
            break;
         case SavingValueType.Float:
            sb.Append(((float)value).ToString("0.##"));
            break;
         case SavingValueType.Bool:
            sb.Append((bool)value ? "yes" : "no");
            break;
         case SavingValueType.Double:
            sb.Append(((double)value).ToString("0.##"));
            break;
         case SavingValueType.Identifier:
            sb.Append($"{value}");
            break;
         case SavingValueType.Color:
            sb.Append(((JominiColor)value).ToString());
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(svl), svl, null);
      }
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
         or TokenType.AtLeftBracket
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
         _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null)
      };
   }
}