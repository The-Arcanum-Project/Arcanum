using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Arcanum.Core.GameObjects.Pops;

public partial class Pop(PopType type,
                         float size,
                         string culture,
                         string religion)
   : INUI, ICollectionProvider<Pop>, IEmpty<Pop>
{
   public PopType Type { get; set; } = type;
   public float Size { get; set; } = size;
   public string Culture { get; set; } = culture;
   public string Religion { get; set; } = religion;

   public override string ToString()
   {
      return $"{Type.Name} ({Size})";
   }

   public static IEnumerable<Pop> GetGlobalItems() => Globals.Locations.Values.SelectMany(l => l.Pops);

   public override bool Equals(object? obj)
   {
      if (obj is Pop other)
         return Type == other.Type &&
                Size.Equals(other.Size) &&
                string.Equals(Culture, other.Culture, StringComparison.Ordinal) &&
                string.Equals(Religion, other.Religion, StringComparison.Ordinal);

      return false;
   }

   public override int GetHashCode()
   {
      return HashCode.Combine(Type, Size, Culture, Religion);
   }

   public static bool operator ==(Pop? left, Pop? right)
   {
      if (left is null && right is null)
         return true;

      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(Pop? left, Pop? right)
   {
      return !(left == right);
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.PopSettings;

   public INUINavigation[] Navigations { get; } = [new NUINavigation(type, "Pop Type")];
   public static Pop Empty { get; } = new (PopType.Empty, 0f, string.Empty, string.Empty);
}