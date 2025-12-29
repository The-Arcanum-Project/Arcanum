using System.Collections;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.DataStructures;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Nexus.AggregateLinkHandling;

public static class NxAggregateLinkChild
{
    public static void ForceSet(IEu5Object parent,
        IEu5Object target,
        Enum e)
    {
        if (!target.IgnoreCommand(e))
            CommandManager.TransferBetweenLinksCommand(parent, target.GetCorrespondingEnum(e)!, target);
        else
            ((IList)parent._getValue(target.GetCorrespondingEnum(e)!)).Add(target);    
    }

    public static void ForceSet(IEu5Object parent, IEu5Object[] targets, Enum e)
    {
        if(targets.Length == 0)
            return;
        
        if (!targets[0].IgnoreCommand(e))
            CommandManager.TransferBetweenLinksCommand(parent, targets[0].GetCorrespondingEnum(e)!, targets);
        else
        {
            var list = ((IList)parent._getValue(targets[0].GetCorrespondingEnum(e)!));
            for (var index = 0; index < targets.Length; index++)
            {
                list.Add(targets[index]);
            }
        }
    }
}