using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// A simple key-value pair where the key is an IAgs object and the value is an Enum
/// </summary>
public interface IIagsEnumKvp<T, TEnum> : IAgs where T : IAgs where TEnum : Enum
{
   [SuppressAgs]
   [AddModifiable]
   public T Key { get; set; }
   [SuppressAgs]
   [AddModifiable]
   public TEnum Value { get; set; }
}