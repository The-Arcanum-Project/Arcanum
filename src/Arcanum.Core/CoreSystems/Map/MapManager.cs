using System.Diagnostics;
using System.Numerics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.CoreSystems.Map;

// In your Map or World management class
public class MapManager
{
   public bool IsMapDataInitialized => Lqt != null!;
   public Location[] AllLocations { get; private set; } = null!;
   public QuadTree Lqt { get; private set; } = null!;

   public void InitializeMapData(RectangleF mapBounds)
   {
      // TODO: @Minnator Subscribe to events to update this is locations are added/removed
      AllLocations = new Location[Globals.Locations.Count];
      foreach (var loc in Globals.Locations.Values)
      {
         Debug.Assert(loc != null);
         if (loc.ColorIndex < 0 || loc.ColorIndex >= AllLocations.Length)
            throw new($"Location {loc.UniqueId} has invalid ColorIndex {loc.ColorIndex}.");

         AllLocations[loc.ColorIndex] = loc;
      }

      Lqt = new(mapBounds, AllLocations);
      foreach (var location in AllLocations)
      {
         // TODO: HOW CAN LOCATIONS BE NULL HERE???
         //Debug.Assert(location != null);
         if (location != null!)
            Lqt.Insert(location);
      }
   }

   public Location? FindLocationAt(Vector2 point)
   {
      // TODO optimize with checks against the last known location
      return Lqt.Query(point);
   }
}