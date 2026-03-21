using System.Diagnostics;
using System.Runtime.CompilerServices;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class BulkCollectionModificationCommand : Eu5ObjectCommand
{
   private readonly IEu5Object[] _targets;
   private readonly object[] _values;
   private bool _add;

   public BulkCollectionModificationCommand(IEu5Object[] targets, Enum attribute, object[] values, bool add) : base(targets, attribute)
   {
      _targets = targets;
      _values = values;
      _add = add;

#if DEBUG
      if (_targets.Length > 0)
      {
         Debug.Assert(_targets.Length == values.Length);
         Debug.Assert(targets[0].GetAllProperties().Contains(Attribute));
         Debug.Assert(targets[0].GetNxItemType(attribute) == values[0].GetType());
      }
#endif

      Execute();
   }

   public sealed override void Execute()
   {
      if (_add)
         for (var i = 0; i < _targets.Length; i++)
            _targets[i]._addToCollection(Attribute!, _values[i]);
      else
         for (var i = 0; i < _targets.Length; i++)
            _targets[i]._removeFromCollection(Attribute!, _values[i]);

      // base.Execute();
   }

   public override void Undo()
   {
      _add = !_add;
      Execute();
      _add = !_add;
   }

   public override void Redo()
   {
      Execute();
   }

   public override IEu5Object[] GetTargets() => _targets;

   public override string GetDescription
      => $"{(_add ? "Add" : "Remove")} {_values.Length} items to/from {Attribute} on {_targets.Length} targets of type {_targets[0].GetType().Name}";

   public override bool Equals(object? obj) => ReferenceEquals(this, obj);

   public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}