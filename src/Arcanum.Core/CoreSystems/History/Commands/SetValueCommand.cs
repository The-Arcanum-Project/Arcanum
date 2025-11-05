using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class SetValueCommand : Eu5ObjectCommand
{
   private readonly record struct ObjectData(IEu5Object Target, object OldValue);
   private IList<ObjectData> _targets = new List<ObjectData>();
   private readonly object _value;

   public override string GetDescription => _targets.Count > 1
                                               ? $"Set {Attribute} to {_value} on {_targets.Count} objects of type {Type}"
                                               : $"Set {Attribute} to {_value} on {_targets.First().Target}";

   public SetValueCommand(IEu5Object target, Enum attribute, object value)
      : base(target, attribute)
   {
      _value = value;
      _targets.Add(new(target, target._getValue(attribute)));
      target._setValue(Attribute, _value); 
      //TODO: @Melco make this more pretty, so that InvalidateUI is not called in the constructor and in try add but handled in EU5ObjectCommand
      InvalidateUI();
   }
   
   public SetValueCommand(IEu5Object[] targets, Enum attribute, object value)
      : base(targets, attribute)
   {
      _value = value;
      _targets = new ObjectData[targets.Length];
      for (var i = 0; i < targets.Length; i++)
      {
         _targets[i] = new(targets[i], targets[i]._getValue(attribute));
         targets[i]._setValue(Attribute, _value);
      }
      InvalidateUI();
   }

   public override IEu5Object[] GetTargets() => _targets.Select(x => x.Target).ToArray();

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      _targets = _targets.ToArray();
   }

   public override void Undo()
   {
      foreach (var data in _targets)
         data.Target._setValue(Attribute, data.OldValue);
      base.Undo();
   }

   public override void Redo()
   {
      foreach (var data in _targets)
         data.Target._setValue(Attribute, _value);
      base.Redo();
   }

   public bool TryAdd(IEu5Object target, Enum attribute, object value)
   {
      if (DisallowMerge(target, attribute) || !_value.Equals(value))
         return false;

      _targets.Add(new(target, target._getValue(attribute)));
      target._setValue(Attribute, _value);
      InvalidateUI();
      return true;
   }
}