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
   }

   public override IEu5Object[] GetTargets() => _targets.Select(x => x.Target).ToArray();

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      _targets = _targets.ToArray();
   }

   public override void Undo()
   {
      base.Undo();
      foreach (var data in _targets)
         data.Target._setValue(Attribute, data.OldValue);
   }

   public override void Redo()
   {
      base.Redo();
      foreach (var data in _targets)
         data.Target._setValue(Attribute, _value);
   }

   public bool TryAdd(IEu5Object target, Enum attribute, object value)
   {
      if (DisallowMerge(target, attribute) || !_value.Equals(value))
         return false;

      _targets.Add(new(target, target._getValue(attribute)));
      target._setValue(Attribute, _value);
      return true;
   }

   public override Type GetTargetPropertyType() => _targets.FirstOrDefault().Target.GetNxPropType(Attribute);

   public override IEu5Object[]? GetTargetProperties()
   {
      if (_targets.Count == 0)
         return null;

      var attrVal = _targets[0].Target.GetNxPropType(Attribute);
      return !attrVal.IsAssignableTo(typeof(IEu5Object))
                ? null
                : _targets.Select(t => (IEu5Object)t.Target._getValue(Attribute)).ToArray();
   }
}