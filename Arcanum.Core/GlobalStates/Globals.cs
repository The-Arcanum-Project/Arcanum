using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.LocationCollections;
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
   public static HashSet<Province> Provinces { get; } = [];
   public static HashSet<Area> Areas { get; } = [];
   public static HashSet<Region> Regions { get; } = [];
   public static HashSet<SuperRegion> SuperRegions { get; } = [];
   public static HashSet<Continent> Continents { get; } = [];
   public static DefaultMapDefinition DefaultMapDefinition { get; set; } = null!;
   public static List<Adjacency> Adjacencies { get; } = [];
}