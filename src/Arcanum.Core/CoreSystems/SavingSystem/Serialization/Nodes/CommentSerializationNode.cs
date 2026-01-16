using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
/// Represents a standalone comment (Body Comment).
/// </summary>
public class CommentSerializationNode(string text) : SerializationNode
{
   public string Text { get; } = text;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      sb.AppendCommentLine(Text);
   }
}