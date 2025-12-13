using System.Diagnostics;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History.Commands.Collections;

public abstract class ModifyCollectionCommand : Eu5ObjectCommand
{
   protected IList<IEu5Object> Targets = new List<IEu5Object>();
   protected readonly object Value;

   protected ModifyCollectionCommand(IEu5Object target, Enum attribute, object value)
      : base(target, attribute)
   {
      Value = value;
      Targets.Add(target);

      InvalidateUI();
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
      Debug.Assert(Attribute != null);
      if (isAdd)
         target._addToCollection(Attribute, Value);
      else
         target._removeFromCollection(Attribute, Value);

      InvalidateUI();

      return true;
   }
   
   public bool TryAddMultiple(IEu5Object target, Enum attribute, object value, bool isAdd)
   {
      if (DisallowMerge(target, attribute) || !Value.Equals(value))
         return false;

      Targets.Add(target);
      Debug.Assert(Attribute != null);
      if (isAdd)
         target._addToCollection(Attribute, Value);
      else
         target._removeFromCollection(Attribute, Value);

      InvalidateUI();

      return true;
   }
}