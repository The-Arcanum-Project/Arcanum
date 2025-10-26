using Arcanum.Core.CoreSystems.History.Dtos;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public abstract class ModifyCollectionCommand : Eu5ObjectCommand
{
   protected IList<IEu5Object> Targets = new List<IEu5Object>();
   protected object Value = null!;

   public override string GetDescription => Targets.Count > 1
                                               ? $"{ActionDescription} {Value} to {Attribute} in {Targets.Count} objects of type {Type}"
                                               : $"{ActionDescription} {Value} to {Attribute} in {Targets.First()}";

   protected abstract string ActionDescription { get; }

   protected ModifyCollectionCommand(IEu5Object target, Enum attribute, object value)
      : base(target, attribute)
   {
      Value = value;
      Targets.Add(target);
   }

   /// <summary>
   /// Only used for deserialization from DTOs
   /// </summary>
   protected ModifyCollectionCommand()
   {
   }

   public override IEu5Object[] GetTargets() => Targets.ToArray();

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      Targets = Targets.ToArray();
   }

   public bool TryAdd(IEu5Object target, Enum attribute, object value)
   {
      if (DisallowMerge(target, attribute) || !Value.Equals(value))
         return false;

      Targets.Add(target);
      return true;
   }

   public override void DeserializeFromDto(object dto)
   {
      var modifyDto = (ModifyCollectionDto)dto;
      Attribute = modifyDto.TargetProperty;
      Targets = IEu5ObjectDtoConverter.FromDtoArray(modifyDto.Targets).ToList();
      Value = modifyDto.Value;
   }

   public override object SerializeToDto()
   {
      return new ModifyCollectionDto
      {
         TargetProperty = Attribute,
         Targets = IEu5ObjectDtoConverter.ToDtoArray(Targets),
         Value = Value,
      };
   }
}