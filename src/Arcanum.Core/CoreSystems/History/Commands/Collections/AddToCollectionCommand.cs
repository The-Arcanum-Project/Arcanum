using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class AddToCollectionCommand
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
   : ModifyCollectionCommand
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{
   private readonly IEu5Object _target;

   public AddToCollectionCommand(IEu5Object target, Enum attribute, object value) : base(target, attribute, value)
   {
      _target = target;
      target._addToCollection(attribute, value);
   }

   protected override string ActionDescription => "Add";

   public override void Undo()
   {
      base.Undo();
      foreach (var r in Targets)
         r._removeFromCollection(Attribute, Value);
   }

   public override void Redo()
   {
      base.Redo();
      foreach (var r in Targets)
         r._addToCollection(Attribute, Value);
   }
}