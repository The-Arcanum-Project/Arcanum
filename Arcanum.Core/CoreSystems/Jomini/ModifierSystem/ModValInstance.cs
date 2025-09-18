using Arcanum.Core.GameObjects.Common;

namespace Arcanum.Core.CoreSystems.Jomini.ModifierSystem;

/// <summary>
/// An instance of a modifier definition with an associated value.
/// </summary>
public class ModValInstance
{
   /// <summary>
   /// An instance of a modifier definition with an associated value.
   /// </summary>
   /// <param name="definition"></param>
   /// <param name="value"></param>
   /// <param name="type"></param>
   [Obsolete("Use the ModifierManager to create instances.")]
   public ModValInstance(ModifierDefinition definition, object value, ModifierType type)
   {
      Definition = definition;
      Value = value;
      Type = type;
   }

   /// <summary>
   /// The definition of the modifier.
   /// </summary>
   public ModifierDefinition Definition { get; }
   /// <summary>
   /// The value of the modifier.
   /// </summary>
   public object Value { get; private set; }
   /// <summary>
   /// The type of the modifier, inferred from the definition.
   /// </summary>
   public ModifierType Type { get; }

   public override string ToString()
   {
      return $"{Definition.UniqueKey} : {Value} ({Type})";
   }
}