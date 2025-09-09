using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Country : INUI, ICollectionProvider<Country>, IEmpty<Country>
{
   public Country(Tag tag)
   {
      Tag = tag;
   }


   #region Nexus

   public Location Capital { get; set; } = (Location)Location.Empty;
   [ReadonlyNexus]
   public Tag Tag { get; set; }
   /// <summary>
   /// If this country is a rebel faction
   /// </summary>
   public bool Revolt { get; set; }
   public bool IsValidForRelease { get; set; }
   public CountryType Type { get; set; } = CountryType.Location;
   public string Color { get; set; } = string.Empty;
   public string ReligiousSchool { get; set; } = string.Empty;
   public string Dynasty { get; set; } = string.Empty;
   public string CourtLanguage { get; set; } = string.Empty;
   public string LiturgicalLanguage { get; set; } = string.Empty;
   public CountryRank CountryRank { get; set; } = Globals.CountryRanks.Find(x => x.Level == 1)!;
   
   public int StartingTechLevel { get; set; } = 0;

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

   public ObservableRangeCollection<string> Includes { get; set; } = [];

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
   public static Country Empty { get; } = new(Tag.Empty);
}