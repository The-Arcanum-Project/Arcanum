using Arcanum.Core.CoreSystems.Parsing.MapParsing;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Country = Arcanum.Core.GameObjects.LocationCollections.Country;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

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
   public static Dictionary<string, Location> Locations { get; } = [];
   public static Dictionary<string, Province> Provinces { get; } = [];
   public static Dictionary<string, Area> Areas { get; } = [];
   public static Dictionary<string, Region> Regions { get; } = [];
   public static Dictionary<string, SuperRegion> SuperRegions { get; } = [];
   public static Dictionary<string, Continent> Continents { get; } = [];
   public static DefaultMapDefinition DefaultMapDefinition { get; set; } = null!;
   public static List<Adjacency> Adjacencies { get; } = [];
   public static Dictionary<string, PopType> PopTypes { get; } = [];
   public static List<LocationRank> LocationRanks { get; } = [];
   public static List<CountryRank> CountryRanks { get; } = [];
   public static List<Road> Roads { get; set; } = [];

   public static Dictionary<Tag, Country> Countries { get; } = [];

}