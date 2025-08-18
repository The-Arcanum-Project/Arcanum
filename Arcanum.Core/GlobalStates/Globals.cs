#define BROWSABLE_HASHSETS

using Arcanum.Core.CoreSystems.Parsing.Steps;
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
   public static DefaultMapDefinition DefaultMapDefinition { get; set; }
   

#if BROWSABLE_HASHSETS
   private static List<Location> _locationsList = [];
   public static List<Location> LocationsList
   {
      get
      {
         if (_locationsList.Count == 0)
            _locationsList = Locations.Values.ToList();
         return _locationsList;
      }
      set => _locationsList = value;
   }
#endif
}