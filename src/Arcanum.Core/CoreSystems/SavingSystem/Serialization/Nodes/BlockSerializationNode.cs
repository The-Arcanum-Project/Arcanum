#region

using System.Collections;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

#endregion

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
///    Represents: key = { ... }
/// </summary>
public class BlockSerializationNode(string? key, bool writeEmpty, IEu5Object? target, PropertySavingMetadata? meta, TokenType separator = TokenType.Equals)
   : SerializationNode
{
   public string? Key { get; } = key; // Null for anonymous blocks
   public TokenType Separator { get; } = separator;
   public List<SerializationNode> Children { get; } = [];
   public bool WriteEmpty { get; } = writeEmpty;
   public string? ClosingComment { get; set; }
   public bool IsCompact { get; set; } // If true: { 1 2 3 } (one line)
   public IEu5Object? Target { get; } = target;
   public PropertySavingMetadata? Metadata { get; } = meta;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      var write = WriteEmpty || Children.Count > 0 || LeadingComment != null || InlineComment != null || ClosingComment != null;
      if (Children.Count > 0 && Children[0] is BulkValueSerializationNode node1)
         write = HasItems(node1.Collection);

      if (!write)
         return;

      // TODO rn this order is ignored this has to be fixed
      if (Target != null)
         PropertyOrderCache.GetOrCreateSortedProperties(Target);

      sb.AppendLineFormat(Metadata, IsCompact);

      WriteLeadingComment(sb, ref commentChar);

      // There is also a check for this in the FormattingService.Format() method, but this only yields control over a property's value serialization.
      // Here we can control the entire block serialization.
      if (Metadata?.SavingMethod != null)
         Metadata.SavingMethod.Invoke(Target!, Metadata, sb, asOneLine);
      else
      {
         // Header: "key = {" or "{"
         if (Metadata is not { IsShattered: true })
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

         if (Metadata is not { IsShattered: true })
            using (sb.Indent())
               SerializeChildElements(sb, ref commentChar, asOneLine);
         else
            SerializeChildElements(sb, ref commentChar, asOneLine);

         // Closing
         if (Metadata is not { IsShattered: true })
         {
            sb.EnforceNewLineCount(1);
            sb.Append('}');
         }
      }

      if (!string.IsNullOrEmpty(ClosingComment))
         sb.AppendSpacer().Append(ClosingComment);

      return;

      bool HasItems(IEnumerable e)
      {
         if (e is ICollection c)
            return c.Count != 0;

         var enumerator = e.GetEnumerator();
         try
         {
            return enumerator.MoveNext();
         }
         finally
         {
            (enumerator as IDisposable)?.Dispose();
         }
      }
   }

   private void SerializeChildElements(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      WriteInlineComment(sb, ref commentChar); // Inline comment for the OPENING brace

      foreach (var child in Children)
         child.Write(sb, ref commentChar, asOneLine);
   }

   private void AppendHeaderToStringBuilder(IndentedStringBuilder sb)
   {
      if (!string.IsNullOrEmpty(Key))
      {
#if DEBUG
         if (key == Globals.DO_NOT_PARSE_ME)
            Debugger.Break();
#endif
         sb.Append(Key).AppendOpeningBrace(separator: SavingUtil.GetSeparator(Separator));
      }
      else
         sb.AppendOpeningBrace(asOneLine: true).AppendSpacer();

      if (!IsCompact)
         sb.AppendLine();
   }
}