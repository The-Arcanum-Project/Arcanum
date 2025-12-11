using System.Diagnostics;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Settings.BaseClasses;
using Arcanum.Core.Settings.SmallSettingsObjects;
using Arcanum.Core.Utils.Colors;
using Vortice.Mathematics;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

/// <summary>
/// An enum containing all available map modes is generated in <see cref="MapModeType"/>. <br/>
/// A dictionary of all available map modes is available in <see cref="AllModes"/>. <br/>
/// A helper method to get a map mode by its enum type is available in <see cref="Get(MapModeType)"/>. <br/>
/// </summary>
public static partial class MapModeManager
{
   private static readonly Location[] LocationsArray = Globals.Locations.Values.ToArray();
   private static Color[] _blueColors = ColorGenerator.GenerateVariations(Config.Settings.MapSettings.WaterShadeBaseColor, 40);
   private static Random _random = new ("Arcanum".GetHashCode());

   #region Plugin MapModes

   /// <summary>
   /// MapModes provided by plugins, keyed by their enum type.
   /// Each plugin provides their own enum to identify their map modes.
   /// </summary>
   private static readonly Dictionary<Type, IReadOnlyDictionary<Enum, IMapMode>> Providers = new ();

   /// <summary>
   /// Registers a new provider of map modes, identified by its unique enum type.
   /// </summary>
   public static void RegisterProvider(Type enumType, IReadOnlyDictionary<Enum, IMapMode> mapModes)
   {
      if (!enumType.IsEnum)
         throw new ArgumentException("Provider key must be an enum type.", nameof(enumType));

      Providers.TryAdd(enumType, mapModes);
   }

   /// <summary>
   /// Provides access to all registered providers, keyed by their enum type.
   /// </summary>
   public static IReadOnlyDictionary<Type, IReadOnlyDictionary<Enum, IMapMode>> AllProviders => Providers;

   /// <summary>
   /// Retrieves a specific map mode using its enum value.
   /// </summary>
   public static IMapMode Get(Enum mapModeEnum)
   {
      var enumType = mapModeEnum.GetType();
      if (Providers.TryGetValue(enumType, out var provider) && provider.TryGetValue(mapModeEnum, out var mapMode))
         return mapMode;

      throw new KeyNotFoundException($"The map mode '{mapModeEnum}' from enum '{enumType.Name}' is not registered.");
   }

   #endregion

   private static MapModeType CurrentMode { get; set; } = MapModeType.Locations;

   public static IMapMode GetCurrent() => Get(CurrentMode);
   // This is set once the map is ready
   public static bool IsMapReady { get; set; } = false;

   // Event to notify that the mapmode has been changed.
   public static event Action<MapModeType>? OnMapModeChanged;

   private static void InitializeMapModeManager()
   {
      SettingsEventManager.RegisterSettingsHandler(nameof(MapSettingsObj.WaterShadeBaseColor), Handler);
   }

   private static void Handler(object sender, SettingsEventArgs e)
   {
      _blueColors = ColorGenerator.GenerateVariations(((Color)e.NewValue!), 40);
   }

   public static void SetMapMode(MapModeType type)
   {
      CurrentMode = type;
      OnMapModeChanged?.Invoke(type);
   }

   public static void RenderCurrent(Color4[] colors)
   {
#if DEBUG
      var sw = Stopwatch.StartNew();
#endif
      UpdateColors(colors, CurrentMode);
#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("MMM", LogLevel.INF, $"Set colors for {CurrentMode} in {sw.ElapsedMilliseconds} ms");
#endif
   }

   public static IMapMode? GetMapModeForButtonIndex(int i)
   {
      if (Config.Settings.MapModeConfig.QuickAccessMapModes.Count > i)
      {
         var modeType = Config.Settings.MapModeConfig.QuickAccessMapModes[i];
         return modeType != MapModeType.Locations ? Get(modeType) : null;
      }

      if (!Config.Settings.MapModeConfig.DefaultAssignMapModes)
         return null;

      return i < Enum.GetNames<MapModeType>().Length ? Get((MapModeType)i) : null;
   }

   private static void UpdateColors(Color4[] colors, MapModeType type)
   {
      var mode = Get(type);
      var count = LocationsArray.Length;

      if (mode is LocationBasedMapMode lbm)
         GenerateLocationbaseMapMode(colors, lbm, count);
      else
         mode.Render(colors);
   }

   private static int DeterministicRandom(int input, int min, int max)
   {
      var x = (uint)input;

      x ^= x << 13;
      x ^= x >> 17;
      x ^= x << 5;

      var range = max - min;
      return min + (int)(x % range);
   }

   private static void GenerateLocationbaseMapMode(Color4[] colors, LocationBasedMapMode mode, int count)
   {
      if (!IsMapReady)
         return;

      if (((IMapMode)mode).IsLandOnly)
      {
         var waterProvinces = new HashSet<Location>(Globals.DefaultMapDefinition.SeaZones);
         waterProvinces.UnionWith(Globals.DefaultMapDefinition.Lakes);
         var useLocWater = Config.Settings.MapSettings.UseShadeOfColorOnWater;
         Parallel.For(0, count, ProcessLocation);
         waterProvinces.Clear();

         void ProcessLocation(int i)
         {
            var location = LocationsArray[i];
            if (waterProvinces.Contains(location))
            {
               if (useLocWater)
                  colors[i] = new (_blueColors[DeterministicRandom(location.ColorIndex, 0, _blueColors.Length)].AsAbgrInt());
               else
                  colors[i] = new (location.Color.AsInt());
            }
            else
               colors[i] = new (mode.GetColorForLocation(location));
         }
      }
      else
      {
         Parallel.For(0,
                      count,
                      i => { colors[i] = new (mode.GetColorForLocation(LocationsArray[i])); });
      }
   }
}