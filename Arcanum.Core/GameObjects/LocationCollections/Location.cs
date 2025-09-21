using System.ComponentModel;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Location
   : LocationComposite, INUI, ICollectionProvider<Location>, IMapInferable<Location>, IEmpty<Location>
{
   public Location(FileInformation information, int color, string name) : base(name, information)
   {
      Color = color;
   }

   #region game/in_game/map_data/named_locations.txt

   [ToStringArguments("X")]
   [Description("The color of the location in the map data.")]
   public int Color { get; set; }
   public new static Location Empty { get; } = new(FileInformation.Empty, 0, "EmptyArcanum_Location");

   #endregion

   #region Market: game/main_menu/setup/start

   [Description("The market associated with this location, if any.")]
   public Market Market { get; set; } = Market.Empty;
   public bool HasMarket => Market != Market.Empty;

   #endregion

   #region Pops: game/main_menu/setup/start/06_pops.txt

   [Description("The pops residing in this location.")]
   public ObservableRangeCollection<Pop> Pops { get; set; } = [];

   #endregion

   public override string ToString() => $"{Name}";

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override ICollection<Location> GetLocations() => [this];

   public override LocationCollectionType LCType => LocationCollectionType.Location;

   public static Dictionary<string, Location> GetGlobalItems() => Globals.Locations;

   public static List<Location> GetInferredList(IEnumerable<Location> sLocs) => sLocs.Distinct().ToList();
   public static IMapMode GetMapMode { get; } = new BaseMapMode();

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

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LocationSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.Province);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"Province: {parent.Name}"));

         navigations.Add(null);
         navigations.AddRange(Pops.Select(pop => new NUINavigation(pop,
                                                                   $"Pop: {pop.Type} ({pop.Culture}, {pop.Religion})")));

         if (HasMarket)
         {
            navigations.Add(null);
            navigations.Add(new NUINavigation(Market, "Market"));
         }

         return navigations.ToArray()!;
      }
   }
}