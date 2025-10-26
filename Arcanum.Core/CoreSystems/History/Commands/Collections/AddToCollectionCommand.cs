using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class AddToCollectionCommand
   : ModifyCollectionCommand
{
   public AddToCollectionCommand()
   {
   }

   public AddToCollectionCommand(IEu5Object target, Enum attribute, object value) : base(target, attribute, value)
   {
   }

   protected override string ActionDescription => "Add";

   public override void Undo()
   {
      base.Undo();
      foreach (var target in Targets)
         target._removeFromCollection(Attribute, Value);
   }

   public override void Redo()
   {
      base.Redo();
      foreach (var target in Targets)
         target._addToCollection(Attribute, Value);
   }
}