using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.Cache;

public readonly struct LruCacheValue(int[] colors)
{
   public readonly int[] Colors = colors;
   public readonly List<Location> InvalidLocations = [];

   public void InvalidateCache(Func<Location, int> colorCreator)
   {
      if (InvalidLocations.Count == 0)
         return;

      foreach (var loc in InvalidLocations)
         Colors[loc.ColorIndex] = colorCreator(loc);

      InvalidLocations.Clear();
   }
}