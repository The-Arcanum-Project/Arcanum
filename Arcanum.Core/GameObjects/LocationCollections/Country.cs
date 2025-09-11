using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Country : INUI, ICollectionProvider<Country>, IEmpty<Country>
{
   public Country(Tag tag)
   {
      Tag = tag;
   }

   #region Nexus

   public Location Capital { get; set; } = Location.Empty;
   [ReadonlyNexus]
   [Description("The unique tag for this country.")]
   public Tag Tag { get; set; }
   /// <summary>
   /// If this country is a rebel faction
   /// </summary>
   [Description("If this country is a rebel faction.")]
   public bool Revolt { get; set; }
   [Description("If this country is valid for release by the player.")]
   public bool IsValidForRelease { get; set; }
   [Description("The type of this country.\nValid types: Location, Army, Pop, Building")]
   public CountryType Type { get; set; } = CountryType.Location;
   [Description("The color key of this country")]
   public string Color { get; set; } = string.Empty;
   [Description("The religious school of this country.")]
   public ReligiousSchool ReligiousSchool { get; set; } = ReligiousSchool.Empty;
   [Description("The ruling dynasty of this country.")]
   public string Dynasty { get; set; } = string.Empty;
   [Description("The court language of this country.")]
   public string CourtLanguage { get; set; } = string.Empty;
   [Description("The liturgical language of this country.")]
   public string LiturgicalLanguage { get; set; } = string.Empty;
   [Description("The rank of this country.")]
   public CountryRank CountryRank { get; set; } = Globals.CountryRanks.Find(x => x.Level == 1)!;
   [Description("The technology level this country starts with.")]
   public int StartingTechLevel { get; set; }

   [Description("The owned and controlled locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlCores { get; set; } = [];
   [Description("The owned and controlled locations that are integrated of this country.")]
   public ObservableRangeCollection<Location> OwnControlIntegrated { get; set; } = [];
   [Description("All Locations conquered but controlled by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnControlConquered { get; set; } = [];
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnControlColony { get; set; } = [];
   [Description("The owned core locations of this country.")]
   public ObservableRangeCollection<Location> OwnCores { get; set; } = [];
   [Description("All Locations conquered but owned by someone else than this country.")]
   public ObservableRangeCollection<Location> OwnConquered { get; set; } = [];
   [Description("The owned and integrated locations of this country.")]
   public ObservableRangeCollection<Location> OwnIntegrated { get; set; } = [];
   [Description("The owned colony locations of this country.")]
   public ObservableRangeCollection<Location> OwnColony { get; set; } = [];
   [Description("The controlled core locations of this country.")]
   public ObservableRangeCollection<Location> ControlCores { get; set; } = [];
   [Description("The controlled locations of this country.")]
   public ObservableRangeCollection<Location> Control { get; set; } = [];
   [Description("All Locations that are our cores but conquered by other countries.")]
   public ObservableRangeCollection<Location> OurCoresConqueredByOthers { get; set; } = [];
   [Description("A list of included ??? for this country.")]
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