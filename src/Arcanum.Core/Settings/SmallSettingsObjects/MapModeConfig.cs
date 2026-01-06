using Arcanum.API.Attributes;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapModeConfig() : InternalSearchableSetting(Config.Settings)
{
   public MapModeManager.MapModeType MapMode01
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Locations;

   public MapModeManager.MapModeType MapMode02
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Provinces;

   public MapModeManager.MapModeType MapMode03
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Areas;

   public MapModeManager.MapModeType MapMode04
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Topography;

   public MapModeManager.MapModeType MapMode05
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Climate;

   public MapModeManager.MapModeType MapMode06
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Religion;

   public MapModeManager.MapModeType MapMode07
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Culture;

   public MapModeManager.MapModeType MapMode08
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Political;

   public MapModeManager.MapModeType MapMode09
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.Vegetation;

   public MapModeManager.MapModeType MapMode10
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = MapModeManager.MapModeType.PluralityCulture;

   [IgnoreInPropertyGrid]
   public List<MapModePreset> MapModePresets
   {
      get => field;
      set => SetNotifyProperty(ref field, value);
   } = [];
}