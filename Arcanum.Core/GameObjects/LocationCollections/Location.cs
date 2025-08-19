using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Location : LocationComposite
{
   public Location(FileInformation information, int color, string name) : base(name, information)
   {
      Color = color;
   }

   #region game/in_game/map_data/named_locations.txt

   public int Color { get; init; }
   public new static LocationComposite Empty { get; } = new Location(FileInformation.Empty, 0, "EmptyArcanum");

   #endregion

   #region Market: game/main_menu/setup/start

   public Market Market { get; set; } = Market.Empty;
   public bool HasMarket => Market != Market.Empty;

   #endregion
   
   
   public override string ToString() => $"{Name} (Color: {Color:X})";
   public override int GetHashCode() => Name.GetHashCode();

   public override ICollection<Location> GetLocations() => [this];

   public override LocationCollectionType LCType => LocationCollectionType.Location;

   public override bool Equals(object? obj)
   {
      if (obj is Location other)
         return string.Equals(Name, other.Name, StringComparison.Ordinal);

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