using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
/// Represents: key = value
/// </summary>
public class PropertySerializationNode(PropertySavingMetadata psm, object value, IEu5Object target) : SerializationNode
{
   public PropertySavingMetadata Psm { get; } = psm;
   public object Value { get; } = value;
   public IEu5Object Target { get; } = target;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine, bool writeDefaults)
   {
      FormattingService.AssignValueType(Psm, Value);

      var shouldSkipCheck = FormattingService.ShouldSkipCheck(Psm, Target, Value, false, writeDefaults);
      if (shouldSkipCheck)
         return;

      sb.AppendLineFormat(Psm, asOneLine);
      WriteLeadingComment(sb, ref commentChar);
      FormattingService.Format(Psm, sb, Target, commentChar, asOneLine, Value, false, writeDefaults);
      WriteInlineComment(sb, ref commentChar);
   }
}