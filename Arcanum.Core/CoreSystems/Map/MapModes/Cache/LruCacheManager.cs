using System.Buffers;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.CoreSystems.Map.MapModes.Cache;

using System.Collections.Generic;

public class LruCacheManager(int maxSize = 5)
{
   private readonly Location[] _locationsArray = Globals.Locations.Values.ToArray();
   private readonly LinkedList<KeyValuePair<MapModeManager.MapModeType, LruCacheValue>> _lruList = [];

   private readonly Dictionary<MapModeManager.MapModeType,
      LinkedListNode<KeyValuePair<MapModeManager.MapModeType, LruCacheValue>>> _cache = new();

   public void InvalidateLocation(Location changedLocation)
   {
      foreach (var node in _lruList)
         node.Value.InvalidLocations.Add(changedLocation);
   }

   public int[] GetOrCreateColors(MapModeManager.MapModeType type)
   {
      if (_cache.TryGetValue(type, out var node))
      {
         var cacheValue = node.Value.Value;
         var mode = MapModeManager.Get(type);

         cacheValue.InvalidateCache(mode.GetColorForLocation);

         _lruList.Remove(node);
         _lruList.AddFirst(node);
         return cacheValue.Colors;
      }

      var newColorData = GenerateFullColorArray(type);
      var newNode = new LinkedListNode<KeyValuePair<MapModeManager.MapModeType, LruCacheValue>>(new(type,
              new(newColorData)));

      _cache.Add(type, newNode);
      _lruList.AddFirst(newNode);

      if (_cache.Count <= maxSize)
         return newColorData;

      if (_lruList.Last != null)
         _cache.Remove(_lruList.Last.Value.Key);
      _lruList.RemoveLast();

      return newColorData;
   }

   private int[] GenerateFullColorArray(MapModeManager.MapModeType type)
   {
      var mode = MapModeManager.Get(type);
      var count = _locationsArray.Length;
      var colors = ArrayPool<int>.Shared.Rent(count);

      try
      {
         Parallel.For(0,
                      count,
                      i => { colors[i] = mode.GetColorForLocation(_locationsArray[i]); });
      }
      catch
      {
         ArrayPool<int>.Shared.Return(colors);
         throw;
      }

      return colors;
   }

   public void MarkInvalid(Location loc)
   {
      foreach (var node in _lruList)
         node.Value.InvalidLocations.Add(loc);
   }
}