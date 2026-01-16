#region

using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

#endregion

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
///    Base class for all output nodes. Handles Comment Writing.
/// </summary>
public abstract class SerializationNode
{
   public string? LeadingComment { get; set; }
   public string? InlineComment { get; set; }

   /// <summary>
   ///    Writes this node to the StringBuilder.
   /// </summary>
   public abstract void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine);

   protected void WriteLeadingComment(IndentedStringBuilder sb, ref string commentChar)
   {
      if (string.IsNullOrEmpty(LeadingComment))
         return;

      foreach (var line in LeadingComment.Split('\n'))
         AppendCommentLine(sb, ref commentChar, line, false);
   }

   protected void WriteInlineComment(IndentedStringBuilder sb, ref string commentChar)
   {
      AppendCommentLine(sb, ref commentChar, InlineComment, true);
   }

   private static void AppendCommentLine(IndentedStringBuilder sb, ref string commentChar, string? line, bool leadingSpace)
   {
      if (string.IsNullOrWhiteSpace(line))
         return;

      if (leadingSpace)
         sb.AppendSpacer();
      if (!line.StartsWith(commentChar))
         sb.Append(commentChar).AppendSpacer();
      sb.AppendLine(line.Trim());
   }

   protected static void AppendSeparator(IndentedStringBuilder sb, TokenType type)
   {
      switch (type)
      {
         case TokenType.Equals:
            sb.Append('=');
            break;
         case TokenType.Greater:
            sb.Append('>');
            break;
         case TokenType.Less:
            sb.Append('<');
            break;
         case TokenType.GreaterOrEqual:
            sb.Append(">=");
            break;
         case TokenType.LessOrEqual:
            sb.Append("<=");
            break;
         case TokenType.LeftBrace:
            sb.Append('{');
            break;
         case TokenType.RightBrace:
            sb.Append('}');
            break;
         case TokenType.RightBracket:
            sb.Append(']');
            break;
         case TokenType.Plus:
            sb.Append('+');
            break;
         case TokenType.Minus:
            sb.Append('-');
            break;
         case TokenType.Multiply:
            sb.Append('*');
            break;
         case TokenType.Divide:
            sb.Append('/');
            break;
         case TokenType.NotEquals:
            sb.Append("!=");
            break;
         case TokenType.QuestionEquals:
            sb.Append("?=");
            break;
         case TokenType.LeftBracket:
            sb.Append('[');
            break;
         case TokenType.Identifier:
            throw new NotSupportedException("Identifier is not a valid separator.");
         case TokenType.ScopeSeparator:
            sb.Append(":");
            break;
         case TokenType.AtIdentifier:
            sb.Append("@");
            break;
         case TokenType.String:
            throw new NotSupportedException("String is not a valid separator.");
         case TokenType.Number:
            throw new NotSupportedException("Number is not a valid separator.");
         case TokenType.Date:
            throw new NotSupportedException("Date is not a valid separator.");
         case TokenType.Yes:
            throw new NotSupportedException("Yes is not a valid separator.");
         case TokenType.No:
            throw new NotSupportedException("No is not a valid separator.");
         case TokenType.Comment:
            sb.Append("#");
            break;
         case TokenType.EndOfFile:
            break;
         case TokenType.Unexpected:
            throw new NotSupportedException("Unexpected is not a valid separator.");
         case TokenType.Whitespace:
         case TokenType.NewLine:
            sb.AppendLine();
            break;
         default:
            sb.Append('=');
            break;
      }
   }
}