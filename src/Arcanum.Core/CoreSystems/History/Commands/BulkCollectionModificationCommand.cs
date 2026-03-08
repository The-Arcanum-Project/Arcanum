using System.Diagnostics;
using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class BulkCollectionModificationCommand : ICommand
{
   private readonly IEu5Object[] _targets;
   private readonly Enum _attribute;
   private readonly object[] _values;
   private bool _add;

   public BulkCollectionModificationCommand(IEu5Object[] targets, Enum attribute, object[] values, bool add)
   {
      _targets = targets;
      _attribute = attribute;
      _values = values;
      _add = add;

#if DEBUG
      if (_targets.Length > 0)
      {
         Debug.Assert(_targets.Length == values.Length);
         Debug.Assert(targets[0].GetAllProperties().Contains(_attribute));
         Debug.Assert(targets[0].GetNxItemType(attribute) == values[0].GetType());
      }
#endif

      Execute();
   }

   public void Execute()
   {
      if (_add)
         for (var i = 0; i < _targets.Length; i++)
            _targets[i]._addToCollection(_attribute, _values[i]);
      else
         for (var i = 0; i < _targets.Length; i++)
            _targets[i]._removeFromCollection(_attribute, _values[i]);

      EventDistributor.RegisterChanges(_values[0].GetType(), _attribute, _targets);
   }

   public void Undo()
   {
      _add = !_add;
      Execute();
      _add = !_add;
   }

   public void Redo()
   {
      Execute();
   }

   public List<int> GetTargetHash() => _targets.Select(x => x.GetHashCode()).ToList();

   public string GetDescription
      => $"{(_add ? "Add" : "Remove")} {_values.Length} items to/from {_attribute} on {_targets.Length} targets of type {_targets[0].GetType().Name}";

   public string GetDebugInformation(int indent)
      => $"{new string(' ', indent)}BulkCollectionModificationCommand: Attribute={_attribute}, Add={_add}, Targets={string.Join(", ", _targets.Select(t => t.GetHashCode()))}, Values={string.Join(", ", _values)}";
}