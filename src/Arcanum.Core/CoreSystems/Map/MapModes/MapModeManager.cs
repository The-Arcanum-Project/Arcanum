using System.Diagnostics;
using Arcanum.Core.CoreSystems.Map.MapModes.Cache;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

/// <summary>
/// An enum containing all available map modes is generated in <see cref="MapModeType"/>. <br/>
/// A dictionary of all available map modes is available in <see cref="AllModes"/>. <br/>
/// A helper method to get a map mode by its enum type is available in <see cref="Get(MapModeType)"/>. <br/>
/// </summary>
public static partial class MapModeManager
{
   #region Plugin MapModes

   /// <summary>
   /// MapModes provided by plugins, keyed by their enum type.
   /// Each plugin provides their own enum to identify their map modes.
   /// </summary>
   private static readonly Dictionary<Type, IReadOnlyDictionary<Enum, IMapMode>> Providers = new();

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

   private static readonly LruCacheManager LruCache = new();
   public static MapModeType CurrentMode { get; private set; } = MapModeType.Locations;
   public static List<MapModeType> RecentModes { get; } = new(25);
   private const int MAX_RECENT_MODES = 25;
   public static bool IsInitialized = false;

   private static void InitializeMapModeManager()
   {
      AppData.HistoryManager.ModifiedType += DataChanged;
   }

   public static void Activate(MapModeType type)
   {
      if (!IsInitialized)
      {
         UIHandle.Instance.PopUpHandle
                 .ShowMBox("Please wait for the map to finish initializing before changing map modes.",
                           "Map Initializing");
         return;
      }

      if (type == CurrentMode)
         return;

      var sw = RenderMapMode(type);
      ArcLog.WriteLine("MMM", LogLevel.INF, $"Set colors for {type} in {sw.ElapsedMilliseconds} ms");
      CurrentMode = type;
      AddToRecentHistory(type);
   }

   private static Stopwatch RenderMapMode(MapModeType type)
   {
      var sw = Stopwatch.StartNew();
      GPUContracts.SetColors(LruCache.GetOrCreateColors(type));
      sw.Stop();
      return sw;
   }

   private static void AddToRecentHistory(MapModeType type)
   {
      if (RecentModes.Count > MAX_RECENT_MODES)
         RecentModes.RemoveAt(RecentModes.Count - 1);
      RecentModes.Insert(0, type);
   }

   public static void DataChanged(Type type, IEu5Object[] objects)
   {
      if (objects.Length == 0)
         return;

      if (objects[0] is not IMapInferable mapInferable)
         return;

      LruCache.MarkInvalid(mapInferable.GetRelevantLocations(objects));

      if (CurrentMode == mapInferable.GetMapMode)
         RenderMapMode(CurrentMode);
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

   public static IMapMode GetCurrent() => Get(CurrentMode);
}