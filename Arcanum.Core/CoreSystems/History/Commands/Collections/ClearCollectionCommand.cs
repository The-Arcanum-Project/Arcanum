using System.Collections;
using System.Diagnostics;
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
      target._clearCollection(attribute);
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
      {
         target.Target._clearCollection(Attribute);
      }
   }

   /// <summary>
   /// Clears the collections in all target objects.
   /// The first time this is run, it backs up the original values.
   /// </summary>
   public override void Redo()
   {
      base.Redo();
      foreach (var target in _targets)
      {
         target.Target._clearCollection(Attribute);
      }
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
      target._clearCollection(attribute);
      return true;
   }

   public override IEu5Object[] GetTargets()
   {
      return _targets.Select(x => x.Target).ToArray();
   }

   public override string GetDescription => _targets.Count > 1
                                               ? $"Clear {Attribute} in {_targets.Count} objects of type {Type}"
                                               : $"Clear {Attribute} in {_targets.First().Target}";
   public override Type? GetTargetPropertyType() => _targets.FirstOrDefault().Target.GetNxItemType(Attribute);

   public override IEu5Object[]? GetTargetProperties()
   {
      if (_targets.Count == 0)
         return null;

      if (_targets[0].Target.GetNxItemType(Attribute) != typeof(IEu5Object))
         return null;

      List<IEu5Object> allObjects = [];
      foreach (var target in _targets)
         allObjects.AddRange(target.Target._getValue(Attribute) as IEnumerable<IEu5Object> ?? []);

      return allObjects.ToArray();
   }
}