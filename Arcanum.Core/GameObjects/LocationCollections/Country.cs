using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Country : INUI, ICollectionProvider<Country>
{
   public Country(Tag tag)
   {
      Tag = tag;
   }

   [SuppressMessage("ReSharper", "InconsistentNaming")]
   public enum OwnerShipType
   {
      own_control_core,
      own_control_integrated,
      own_control_conquered,
      own_control_colony,
      own_core,
      own_conquered,
      own_integrated,
      own_colony,
      control_core,
      control,
      our_cores_conquered_by_others,
   }

   #region Nexus

   public Tag Tag { get; set; }

   public ObservableRangeCollection<Location> OwnControlCores { get; set; } = [];
   public ObservableRangeCollection<Location> OwnControlIntegrated { get; set; } = [];
   public ObservableRangeCollection<Location> OwnControlConquered { get; set; } = [];
   public ObservableRangeCollection<Location> OwnControlColony { get; set; } = [];
   public ObservableRangeCollection<Location> OwnCores { get; set; } = [];
   public ObservableRangeCollection<Location> OwnConquered { get; set; } = [];
   public ObservableRangeCollection<Location> OwnIntegrated { get; set; } = [];
   public ObservableRangeCollection<Location> OwnColony { get; set; } = [];
   public ObservableRangeCollection<Location> ControlCores { get; set; } = [];
   public ObservableRangeCollection<Location> Control { get; set; } = [];
   public ObservableRangeCollection<Location> OurCoresConqueredByOthers { get; set; } = [];

   #endregion

   public bool SetCollection(string name, IEnumerable<Location> locs)
   {
      switch (name.ToLower())
      {
         case "own_control_core":
            OwnControlCores.AddRange(locs);
            return true;
         case "own_control_integrated":
            OwnControlIntegrated.AddRange(locs);
            return true;
         case "own_control_conquered":
            OwnControlConquered.AddRange(locs);
            return true;
         case "own_control_colony":
            OwnControlColony.AddRange(locs);
            return true;
         case "own_core":
            OwnCores.AddRange(locs);
            return true;
         case "own_conquered":
            OwnConquered.AddRange(locs);
            return true;
         case "own_integrated":
            OwnIntegrated.AddRange(locs);
            return true;
         case "own_colony":
            OwnColony.AddRange(locs);
            return true;
         case "control_core":
            ControlCores.AddRange(locs);
            return true;
         case "control":
            Control.AddRange(locs);
            return true;
         case "our_cores_conquered_by_others":
            OurCoresConqueredByOthers.AddRange(locs);
            return true;
         default:
            return false;
      }
   }
   
   public bool IsReadonly => false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.CountrySettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<Country> GetGlobalItems() => Globals.Countries.Values;
   
   public override string ToString() => Tag.Name;
}