#region

using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

#endregion

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
///    Represents: key = { ... }
/// </summary>
public class BlockSerializationNode(string? key, bool writeEmpty, IEu5Object? target, TokenType separator = TokenType.Equals) : SerializationNode
{
   public string? Key { get; } = key; // Null for anonymous blocks
   public TokenType Separator { get; } = separator;
   public List<SerializationNode> Children { get; } = [];
   public bool WriteEmpty { get; } = writeEmpty;
   public string? ClosingComment { get; set; }
   public bool IsCompact { get; set; } // If true: { 1 2 3 } (one line)
   public IEu5Object? Target { get; } = target;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      var write = WriteEmpty || (Children.Count > 0 || LeadingComment != null || InlineComment != null || ClosingComment != null);

      if (!write)
         return;

      // TODO rn this order is ignored this has to be fixed
      if (Target != null)
         PropertyOrderCache.GetOrCreateSortedProperties(Target);
      WriteLeadingComment(sb, ref commentChar);

      // Header: "key = {" or "{"
      AppendHeaderToStringBuilder(sb);

      // Compact Mode (e.g. Colors, Arrays)
      if (IsCompact)
      {
         foreach (var child in Children)
            // Compact nodes usually don't have leading comments rendered, 
            // or we strip them to fit on one line.
            // Assuming child is ValueOutputNode or PropertyOutputNode
            child.Write(sb, ref commentChar, asOneLine);
         sb.AppendSpacer().Append('}');
         WriteInlineComment(sb, ref commentChar); // Inline comment for the whole block
         return;
      }

      // Expanded Mode

      using (sb.Indent())
      {
         WriteInlineComment(sb, ref commentChar); // Inline comment for the OPENING brace

         foreach (var child in Children)
            child.Write(sb, ref commentChar, asOneLine);
      }

      // Closing
      sb.Append('}');

      if (!string.IsNullOrEmpty(ClosingComment))
         sb.AppendSpacer().Append(ClosingComment);

      sb.AppendLine();
   }

   private void AppendHeaderToStringBuilder(IndentedStringBuilder sb)
   {
      if (!string.IsNullOrEmpty(Key))
      {
         sb.Append(Key)
           .AppendSpacer();
         AppendSeparator(sb, Separator);
         sb.AppendSpacer();
      }

      sb.Append('{');
   }
}