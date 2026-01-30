using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
/// Wraps an object that has a custom 'SavingMethod' defined in metadata.
/// We cannot build an AST for this, so we defer execution until the Write phase.
/// </summary>
public class ManualSerializationNode(IEu5Object ags, List<PropertySavingMetadata> props) : SerializationNode
{
   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      WriteLeadingComment(sb, ref commentChar);

      ags.ClassMetadata.SavingMethod?.Invoke(ags, [.. props], sb, ags.AgsSettings.AsOneLine);
   }
}