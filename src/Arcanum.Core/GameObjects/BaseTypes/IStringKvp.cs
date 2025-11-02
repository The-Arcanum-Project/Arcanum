using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// A simple key-value pair where both key and value are strings
/// </summary>
public interface IStringKvp
{
   [SuppressAgs]
   [AddModifiable]
   public string Key { get; set; }
   [SuppressAgs]
   [AddModifiable]
   public string Value { get; set; }
}