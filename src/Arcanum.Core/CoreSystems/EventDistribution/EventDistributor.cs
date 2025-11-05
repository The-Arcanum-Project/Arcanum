using System.Collections;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.EventDistribution;

public static class EventDistributor
{
   /// <summary>
   /// The UI subscribes to this action to update the UI when this is invoked.
   /// </summary>
   public static Action? UpdateNUI;

   /// <summary>
   /// Is invoked for any objects changing a property
   /// </summary>
   public static Action<Type, Enum, IEu5Object[]>? ObjectOfTypeModified;

   public static void RegisterChanges(Type type, Enum nxProp, IEu5Object[] targets)
   {
      if (targets.Length == 0)
         return;

      var itemType = targets[0].GetNxItemType(nxProp);

      // We have a collection
      if (itemType is not null && itemType.IsAssignableTo(typeof(IEu5Object)))
      {
         List<IEu5Object> allNestedObjects = [];
         foreach (var target in targets)
         {
            if (target._getValue(nxProp) is not IEnumerable collection)
               continue;

            foreach (var item in collection)
               if (item is IEu5Object eu5Object)
                  allNestedObjects.Add(eu5Object);
         }
         
         ObjectOfTypeModified?.Invoke(type, nxProp, allNestedObjects.ToArray());
      }
      // We have a normal property
      else
      {
         var nxPropType = targets[0].GetNxPropType(nxProp);

         // We have nested objects and want to invoke for them instead.
         if (nxPropType.IsAssignableTo(typeof(IEu5Object)))
         {
            HashSet<IEu5Object> nestedObjects = new (targets.Length);
            for (var i = 0; i < targets.Length; i++)
               nestedObjects.Add((IEu5Object)targets[i]._getValue(nxProp));

            ObjectOfTypeModified?.Invoke(nxPropType, nxProp, nestedObjects.ToArray());
            return;
         }

         ObjectOfTypeModified?.Invoke(type, nxProp, targets);
      }
   }
}