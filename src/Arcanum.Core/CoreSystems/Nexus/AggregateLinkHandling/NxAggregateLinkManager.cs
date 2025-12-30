using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Nexus.AggregateLinkHandling;

public class NxAggregateLinkManager
{
   public static bool ForceSet(IEu5Object parent,
                               IEu5Object target,
                               Enum e)
   {
      switch (target.GetNxPropAggregateLinkType(e))
      {
         case AggregateLinkType.None:
            throw new InvalidOperationException("The provided enum is not an aggregate link.");
         case AggregateLinkType.Child:
            NxAggregateLinkChild.ForceSet(parent, target, e);
            return true;
         case AggregateLinkType.Parent:
         case AggregateLinkType.ReverseParent:
         case AggregateLinkType.ReverseChild:
         default:
            return false;
      }
   }

   public static bool ForceSet(IEu5Object parent, IEu5Object[] targets, Enum e)
   {
      if (targets.Length == 0)
         return false;

      switch (targets[0].GetNxPropAggregateLinkType(e))
      {
         case AggregateLinkType.None:
            throw new InvalidOperationException("The provided enum is not an aggregate link.");
         case AggregateLinkType.Child:
            NxAggregateLinkChild.ForceSet(parent, targets, e);
            return true;
         case AggregateLinkType.Parent:
         case AggregateLinkType.ReverseParent:
         case AggregateLinkType.ReverseChild:
         default:
            return false;
      }
   }
}