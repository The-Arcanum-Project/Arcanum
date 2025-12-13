using System.Diagnostics;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class SetValueCommand : Eu5ObjectCommand
{
   //TODO @Melco @Minnator: Make a setting for this:
   private const float MAX_MERGE_TIME_SECONDS = 1.5f;
   private readonly record struct ObjectData(IEu5Object Target, object OldValue);

   private IList<ObjectData> _targets = new List<ObjectData>();
   private object _value;
   private DateTime? _lastUpdate;

   public override string GetDescription => _targets.Count > 1
                                               ? $"Set {Attribute} to {_value} on {_targets.Count} objects of type {Type}"
                                               : $"Set {Attribute} to {_value} on {_targets.First().Target}";

   public SetValueCommand(IEu5Object target, Enum attribute, object value)
      : base(target, attribute)
   {
      Debug.Assert(Attribute != null);
      _value = value;
      _lastUpdate = DateTime.Now;
      _targets.Add(new (target, target._getValue(attribute)));
      target._setValue(Attribute, _value);
      //TODO: @Melco make this more pretty, so that InvalidateUI is not called in the constructor and in try add but handled in EU5ObjectCommand
      InvalidateUI();
   }

   public SetValueCommand(IEu5Object[] targets, Enum attribute, object value)
      : base(targets, attribute)
   {
      Debug.Assert(Attribute != null);
      _value = value;
      _lastUpdate = DateTime.Now;
      _targets = new ObjectData[targets.Length];
      for (var i = 0; i < targets.Length; i++)
      {
         _targets[i] = new (targets[i], targets[i]._getValue(attribute));
         targets[i]._setValue(Attribute, _value);
      }

      InvalidateUI();
   }

   public override IEu5Object[] GetTargets() => _targets.Select(x => x.Target).ToArray();

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      _lastUpdate = null;
      _targets = _targets.ToArray();
   }

   public override void Undo()
   {
      Debug.Assert(Attribute != null);
      foreach (var data in _targets)
         data.Target._setValue(Attribute, data.OldValue);
      base.Undo();
   }

   public override void Redo()
   {
      Debug.Assert(Attribute != null);
      foreach (var data in _targets)
         data.Target._setValue(Attribute, _value);
      base.Redo();
   }

   public bool TryAdd(IEu5Object target, Enum attribute, object value)
   {
      if (DisallowMerge(target, attribute))
         return false;

      if (_value.Equals(value))
      {
         Debug.Assert(Attribute != null);
         _targets.Add(new (target, target._getValue(attribute)));
         target._setValue(Attribute, _value);
         InvalidateUI();
         return true;
      }

      var currentTime = DateTime.Now;
      if ((currentTime - _lastUpdate!.Value).TotalSeconds > MAX_MERGE_TIME_SECONDS)
         return false;

      _lastUpdate = currentTime;

      if (_targets.Count != 1)
         return false;
      if (!target.Equals(_targets[0].Target))
         return false;

      _value = value;
      Debug.Assert(Attribute != null);
      target._setValue(Attribute, _value);
      InvalidateUI();
      return true;
   }

   public bool TryAdd(IEu5Object[] targets, Enum attribute, object value)
   {
      if (DisallowValueReplace(targets, attribute))
         return false;

      var currentTime = DateTime.Now;
      if ((currentTime - _lastUpdate!.Value).TotalSeconds > MAX_MERGE_TIME_SECONDS)
         return false;

      _lastUpdate = currentTime;

      // In this case, we make a completely new command instead of merging since this is called for multiple objects
      if (_value.Equals(value))
         return false;

      //Check for overwriting
      if (targets.Length != _targets.Count)
         return false;
      if (targets.Where((t, index) => !t.Equals(_targets[index].Target)).Any())
         return false;

      _value = value;
      Debug.Assert(Attribute != null);
      foreach (var target in targets)
         target._setValue(Attribute, _value);
      InvalidateUI();
      return true;
   }
}