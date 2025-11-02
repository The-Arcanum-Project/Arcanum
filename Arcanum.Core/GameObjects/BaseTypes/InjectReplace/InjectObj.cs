using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

public class InjectObj
{
   public InjRepType InjRepType { get; init; }

   public IEu5Object Target { get; init; }

   public KeyValuePair<Enum, object>[] InjectedProperties { get; init; }
}