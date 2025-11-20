using System.Buffers;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Settings.BaseClasses;
using Arcanum.Core.Settings.SmallSettingsObjects;
using Arcanum.Core.Utils.Colors;

namespace Arcanum.Core.CoreSystems.Map.MapModes.Cache;

public class LruCacheManager
{
   private readonly int _maxSize;

   private readonly Location[] _locationsArray = Globals.Locations.Values.ToArray();
   private readonly LinkedList<KeyValuePair<MapModeManager.MapModeType, LruCacheValue>> _lruList = [];

   private readonly Dictionary<MapModeManager.MapModeType,
      LinkedListNode<KeyValuePair<MapModeManager.MapModeType, LruCacheValue>>> _cache = new();

   public LruCacheManager(int maxSize = 5)
   {
      _maxSize = maxSize;

      SettingsEventManager.RegisterSettingsHandler(nameof(MapSettingsObj.UseShadeOfColorOnWater),
                                                   (_, args) =>
                                                   {
                                                      if (args.SettingName ==
                                                          nameof(MapSettingsObj.UseShadeOfColorOnWater))
                                                         _cache.Clear();
                                                   });
   }

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

      if (_cache.Count <= _maxSize)
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
         if (mode.IsLandOnly)
         {
            var waterProvinces = new HashSet<Location>(Globals.DefaultMapDefinition.SeaZones);
            waterProvinces.UnionWith(Globals.DefaultMapDefinition.Lakes);
            var blueColors = ColorGenerator.GenerateVariations(Config.Settings.MapSettings.WaterShadeBaseColor, 40);
            var useLocWater = Config.Settings.MapSettings.UseShadeOfColorOnWater;
            Parallel.For(0,
                         count,
                         i =>
                         {
                            if (waterProvinces.Contains(_locationsArray[i]))
                            {
                               if (useLocWater)
                                  colors[i] = blueColors[Random.Shared.Next(blueColors.Count)].AsAbgrInt();
                               else
                                  colors[i] = _locationsArray[i].Color.AsInt();
                            }
                            else
                               colors[i] = mode.GetColorForLocation(_locationsArray[i]);
                         });
            waterProvinces.Clear();
         }
         else
         {
            Parallel.For(0,
                         count,
                         i => { colors[i] = mode.GetColorForLocation(_locationsArray[i]); });
         }
      }
      catch
      {
         ArrayPool<int>.Shared.Return(colors);
         throw;
      }

      var returnVal = colors[..count];
      ArrayPool<int>.Shared.Return(colors);
      return returnVal;
   }

   public void MarkInvalid(Location loc)
   {
      foreach (var node in _lruList)
         node.Value.InvalidLocations.Add(loc);
   }

   public void MarkInvalid(IEnumerable<Location> locs)
   {
      foreach (var loc in locs)
         MarkInvalid(loc);
   }
}