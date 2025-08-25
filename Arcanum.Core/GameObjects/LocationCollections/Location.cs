using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Location : LocationComposite, INUI
{
   public Location(FileInformation information, int color, string name) : base(name, information)
   {
      Color = color;
   }

   #region game/in_game/map_data/named_locations.txt

   [ToStringArguments("X")]
   public int Color { get; set; }
   public new static LocationComposite Empty { get; } = new Location(FileInformation.Empty, 0, "EmptyArcanum");

   #endregion

   #region Market: game/main_menu/setup/start

   public Market Market { get; set; } = Market.Empty;
   public bool HasMarket => Market != Market.Empty;

   #endregion

   #region Pops: game/main_menu/setup/start/06_pops.txt

   public List<Pop> Pops { get; set; } = [];

   #endregion

   public override string ToString() => $"{Name} (Color: {Color:X})";
   // ReSharper disable once NonReadonlyMemberInGetHashCode
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

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.LocationSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.Province);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"Province: {parent.Name}"));

         navigations.Add(null);
         navigations.AddRange(Pops.Select(pop => new NUINavigation(pop, $"Pop: {pop.Type} ({pop.Culture}, {pop.Religion})")));

         if (HasMarket)
         {
            navigations.Add(null);
            navigations.Add(new NUINavigation(Market, "Market"));
         }

         return navigations.ToArray()!;
      }
   }
}