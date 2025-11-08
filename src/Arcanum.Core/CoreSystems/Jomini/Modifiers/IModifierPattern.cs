using Arcanum.Core.CoreSystems.NUI.Attributes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

/// <summary>
/// An interface to define a modifier pattern with a unique identifier and an associated value.
/// </summary>
public interface IModifierPattern
{
   /// <summary>
   /// The unique identifier for the modifier pattern.
   /// </summary>
   [AddModifiable]
   [PropertyConfig(isReadonly: true)]
   public string UniqueId { get; set; }

   /// <summary>
   /// The associated value for the modifier pattern.
   /// </summary>
   [AddModifiable]
   public object Value { get; set; }

   [AddModifiable]
   public ModifierType Type { get; set; }
}