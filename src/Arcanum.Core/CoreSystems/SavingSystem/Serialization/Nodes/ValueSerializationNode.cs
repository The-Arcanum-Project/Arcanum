using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

public class ValueSerializationNode(object value) : SerializationNode
{
   public object Value { get; } = value;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine, bool writeDefaults)
   {
      WriteLeadingComment(sb, ref commentChar);
      sb.Append(Value.ToString());
      WriteInlineComment(sb, ref commentChar);
   }
}