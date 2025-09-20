using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GameObjects.Character;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates.BackingClasses;
using Adjacency = Arcanum.Core.GameObjects.Map.Adjacency;
using Country = Arcanum.Core.GameObjects.LocationCollections.Country;
using LocationRank = Arcanum.Core.GameObjects.LocationCollections.LocationRank;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;
using Road = Arcanum.Core.GameObjects.Map.Road;

namespace Arcanum.Core.GlobalStates;

public enum AppState
{
   Error,
   Loading,
   EditingAllowed,
   EditingDisabled,
   Saving,
}

public static class Globals
{
   public const string REPLACE_DESCRIPTION = "???REPLACE_ME???";

   public static DefaultMapDefinition DefaultMapDefinition { get; set; } = null!;
   public static Dictionary<string, PopType> PopTypes { get; } = [];
   public static Dictionary<string, LocationRank> LocationRanks { get; } = [];
   public static List<CountryRank> CountryRanks { get; } = [];
   public static List<Road> Roads { get; set; } = [];

   public static Dictionary<string, Country> Countries { get; } = [];
   public static Dictionary<string, Institution> Institutions { get; set; } = [];
   public static Dictionary<string, ReligiousSchool> ReligiousSchools { get; set; } = [];

   public static Dictionary<string, Culture> Cultures { get; } = [];

   #region Map

   public static Dictionary<string, Climate> Climates { get; set; } = [];
   public static Dictionary<string, Vegetation> Vegetation { get; set; } = [];
   public static Dictionary<string, Topography> Topography { get; set; } = [];
   public static Dictionary<string, Location> Locations { get; } = [];
   public static Dictionary<string, Province> Provinces { get; } = [];
   public static Dictionary<string, Area> Areas { get; } = [];
   public static Dictionary<string, Region> Regions { get; } = [];
   public static Dictionary<string, SuperRegion> SuperRegions { get; } = [];
   public static Dictionary<string, Continent> Continents { get; } = [];
   public static List<Adjacency> Adjacencies { get; } = [];

   #endregion

   #region Court

   public static Dictionary<string, Language> Languages { get; set; } = [];
   public static Dictionary<string, Regency> Regencies { get; set; } = [];
   public static Dictionary<string, Character> Characters { get; } = [];

   #endregion

   #region Common

   public static Dictionary<string, ModifierDefinition> ModifierDefinitions { get; set; } = [];

   #endregion

   public static List<Age> Ages { get; set; } = [];
#if DEBUG
   public static List<TestINUI> TestNUIObjects { get; } = [];
#endif
}