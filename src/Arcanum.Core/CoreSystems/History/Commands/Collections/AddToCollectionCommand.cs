using System.Diagnostics;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class AddToCollectionCommand
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
   : ModifyCollectionCommand
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
{
   public AddToCollectionCommand(IEu5Object target, Enum attribute, object value) : base(target, attribute, value)
   {
      target._addToCollection(attribute, value);
   }

   public override string GetDescription => Targets.Count > 1
                                               ? $"Add {Value} to {Attribute} in {Targets.Count} objects of type {Type}"
                                               : $"Add {Value} to {Attribute} in {Targets.First()}";

   public override void Undo()
   {
      base.Undo();
      Debug.Assert(Attribute != null);
      foreach (var r in Targets)
         r._removeFromCollection(Attribute, Value);
   }

   public override void Redo()
   {
      base.Redo();
      Debug.Assert(Attribute != null);
      foreach (var r in Targets)
         r._addToCollection(Attribute, Value);
   }
}