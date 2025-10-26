using Arcanum.Core.CoreSystems.History.Dtos;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands;

public class SetValueCommand : Eu5ObjectCommand
{
   private readonly record struct ObjectData(IEu5Object Target, object OldValue);
   private IList<ObjectData> _targets = new List<ObjectData>();
   private object _value = null!;

   public override string GetDescription => _targets.Count > 1
                                               ? $"Set {Attribute} to {_value} on {_targets.Count} objects of type {Type}"
                                               : $"Set {Attribute} to {_value} on {_targets.First().Target}";

   public override object SerializeToDto()
   {
      return new SetValueDto
      {
         Value = _value,
         Targets = IEu5ObjectDtoConverter.ToDtoArray(_targets.Select(x => x.Target).ToArray()),
         OldValues = _targets.Select(x => x.OldValue).ToArray(),
      };
   }

   public override void DeserializeFromDto(object dto)
   {
      var dtoString = dto.ToString() ?? string.Empty;
      var data = JsonProcessor.Deserialize<SetValueDto>(dtoString)!;
      _value = data.Value;
      _targets = new List<ObjectData>();
      var targets = IEu5ObjectDtoConverter.FromDtoArray(data.Targets);
      for (var i = 0; i < targets.Length; i++)
         _targets.Add(new(targets[i], data.OldValues[i]));
   }

   public SetValueCommand(IEu5Object target, Enum attribute, object value)
      : base(target, attribute)
   {
      _value = value;
      _targets.Add(new(target, target._getValue(attribute)));
   }

   public SetValueCommand()
   {
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
      return true;
   }
}