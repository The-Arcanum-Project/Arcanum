using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Location(int color, string name) : LocationComposite(name)
{
   #region game/in_game/map_data/named_locations.txt

   public int Color { get; init; } = color;
   public new static LocationComposite Empty { get; } = new Location(0,"EmptyArcanum");

   #endregion

   public override string ToString() => $"{Name} (Color: {Color:X})";
   public override int GetHashCode() => Name.GetHashCode();

   public override ICollection<Location> GetLocations() => [this];

   public override LocationCollectionType LCType => LocationCollectionType.Location;

   public override bool Equals(object? obj)
   {
      if (obj is Location other)
         return Color == other.Color && Name == other.Name;

      return false;
   }

   public static bool operator ==(Location? left, Location? right)
   {
      if (left is null)
         return right is null;

      return left.Equals(right);
   }

   public static bool operator !=(Location? left, Location? right) => !(left == right);
}