using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
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
}