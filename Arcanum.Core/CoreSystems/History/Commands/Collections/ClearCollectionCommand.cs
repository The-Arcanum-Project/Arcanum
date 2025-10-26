using System.Collections;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.History.Dtos;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public class ClearCollectionCommand : Eu5ObjectCommand
{
   private readonly record struct ObjectData(IEu5Object Target, object[] OldValue);
   private IList<ObjectData> _targets = new List<ObjectData>();

   public ClearCollectionCommand(IEu5Object target, Enum attribute) : base(target, attribute)
   {
      Debug.Assert(target._getValue(attribute) is IEnumerable);

      _targets.Add(new(target, (target._getValue(attribute) as IEnumerable)!.Cast<object>().ToArray()));
   }

   public ClearCollectionCommand()
   {
   }

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      _targets = _targets.ToArray();
   }

   public override void Undo()
   {
      base.Undo();
      foreach (var target in _targets)
         target.Target._addRangeToCollection(Attribute, target.OldValue);
   }

   /// <summary>
   /// Clears the collections in all target objects.
   /// The first time this is run, it backs up the original values.
   /// </summary>
   public override void Redo()
   {
      base.Redo();
      foreach (var target in _targets)
         target.Target._clearCollection(Attribute);
   }

   public override object SerializeToDto()
   {
      return new ClearCollectionDto
      {
         Targets = IEu5ObjectDtoConverter.ToDtoArray(_targets.Select(x => x.Target).ToArray()),
         OldValues = _targets.Select(x => x.OldValue).ToArray(),
      };
   }

   public override void DeserializeFromDto(object dto)
   {
      var clearDto = (ClearCollectionDto)dto;
      _targets = new List<ObjectData>();
      var targetObjects = IEu5ObjectDtoConverter.FromDtoArray(clearDto.Targets);
      for (var i = 0; i < targetObjects.Length; i++)
         _targets.Add(new(targetObjects[i], clearDto.OldValues[i]));
   }

   /// <summary>
   /// Merges another command into this one if it affects the same attribute.
   /// </summary>
   public bool TryAdd(IEu5Object target, Enum attribute)
   {
      if (DisallowMerge(target, attribute))
         return false;

      Debug.Assert(target._getValue(attribute) is IEnumerable);
      _targets.Add(new(target, (target._getValue(attribute) as IEnumerable)!.Cast<object>().ToArray()));
      return true;
   }

   public override IEu5Object[] GetTargets()
   {
      return _targets.Select(x => x.Target).ToArray();
   }

   public override string GetDescription => _targets.Count > 1
                                               ? $"Clear {Attribute} in {_targets.Count} objects of type {Type}"
                                               : $"Clear {Attribute} in {_targets.First().Target}";
}