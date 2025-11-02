using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class RemoveFromCollectionCommand
   : ModifyCollectionCommand
{
   public RemoveFromCollectionCommand(IEu5Object target, Enum attribute, object value) : base(target, attribute, value)
   {
      target._removeFromCollection(attribute, value);
   }

   protected override string ActionDescription => "Remove";

   public override void Undo()
   {
      base.Undo();
      foreach (var target in Targets)
         target._addToCollection(Attribute, Value);
   }

   public override void Redo()
   {
      base.Redo();
      foreach (var target in Targets)
         target._removeFromCollection(Attribute, Value);
   }
}