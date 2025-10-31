using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public abstract class ModifyCollectionCommand : Eu5ObjectCommand
{
   protected IList<IEu5Object> Targets = new List<IEu5Object>();
   protected readonly object Value;

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

   public override IEu5Object[] GetTargets() => Targets.ToArray();

   public override void FinalizeSetup()
   {
      base.FinalizeSetup();
      Targets = Targets.ToArray();
   }

   public bool TryAdd(IEu5Object target, Enum attribute, object value, bool isAdd)
   {
      if (DisallowMerge(target, attribute) || !Value.Equals(value))
         return false;

      Targets.Add(target);
      if (isAdd)
         target._addToCollection(Attribute, Value);
      else
         target._removeFromCollection(Attribute, Value);
      return true;
   }

   public override Type? GetTargetPropertyType() => Targets.FirstOrDefault()?.GetNxItemType(Attribute);

   public override IEu5Object[]? GetTargetProperties()
   {
      if (Targets.Count == 0)
         return null;

      if (Targets[0].GetNxItemType(Attribute) != typeof(IEu5Object))
         return null;

      List<IEu5Object> allObjects = [];
      foreach (var target in Targets)
         allObjects.AddRange(target._getValue(Attribute) as IEnumerable<IEu5Object> ?? []);

      return allObjects.ToArray();
   }
}