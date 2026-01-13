using System.Collections;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Serialization.Nodes;

public class BulkValueSerializationNode(IEnumerable collection, PropertySavingMetadata meta, IEu5Object target)
   : SerializationNode
{
   public IEnumerable Collection { get; } = collection;
   public PropertySavingMetadata Meta { get; } = meta;
   public IEu5Object Target { get; } = target;

   public override void Write(IndentedStringBuilder sb, ref string commentChar, bool asOneLine)
   {
      FormattingService.AssignValueType(meta, Collection);
      FormattingService.HandleCollectionSerialization(meta, sb, Target, commentChar, Collection);
   }
}