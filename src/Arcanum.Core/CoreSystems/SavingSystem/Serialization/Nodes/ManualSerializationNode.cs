using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

/// <summary>
/// Wraps an object that has a custom 'SavingMethod' defined in metadata.
/// We cannot build an AST for this, so we defer execution until the Write phase.
/// </summary>
public class ManualSerializationNode(IEu5Object ags, List<PropertySavingMetadata> props, PropertySavingMetadata? meta) : SerializationNode
{
   public PropertySavingMetadata? Meta { get; } = meta;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine, bool writeDefaults)
   {
      WriteLeadingComment(sb, ref commentChar);
      if (Meta == null) // We have an entire object here
         sb.AppendInjRepType(ags.InjRepType);
      ags.ClassMetadata.SavingMethod?.Invoke(ags, [.. props], sb, ags.AgsSettings.AsOneLine, writeDefaults);
   }
}