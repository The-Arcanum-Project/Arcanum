using Arcanum.Core.CoreSystems.NUI.Attributes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects;

public abstract class NameKeyDefined(string name)
{
   [ReadonlyNexus]
   [AddModifiable]
   public string Name { get; set; } = name;

   public override string ToString() => Name;

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override bool Equals(object? obj)
   {
      if (obj is not NameKeyDefined other)
         return false;

      return string.Equals(Name, other.Name, StringComparison.Ordinal);
   }
}