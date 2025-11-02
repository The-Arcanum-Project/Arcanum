using Arcanum.Core.CoreSystems.Jomini.Modifiers;

namespace Arcanum.Core.CoreSystems.Jomini.Effects;

/// <summary>
/// An instance of an effect definition with an associated value.
/// </summary>
public class EffectInstance : IModifierPattern
{
   [Obsolete("Use the EffectManager to create instances.")]
   public EffectInstance(EffectDefinition definition, object value, ModifierType type)
   {
      Definition = definition;
      Value = value;
      Type = type;
   }

   public EffectDefinition Definition { get; set; }
   public string UniqueId
   {
      get => Definition.Name;
      set => throw new NotSupportedException();
   }
   public object Value { get; set; }
   public ModifierType Type { get; set; }

   public override string ToString()
   {
      return $"{Definition.Name} : {Value} ({Type})";
   }

   protected bool Equals(EffectInstance other) => Definition.Equals(other.Definition) && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((EffectInstance)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(Definition, Value);
}