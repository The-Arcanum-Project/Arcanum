using System.ComponentModel;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapModeConfig() : InternalSearchableSetting(Config.Settings)
{
   [Description("The number of map mode buttons to show in the map mode selection UI.")]
   [DefaultValue(10)]
   public int NumOfMapModeButtons
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 10;

   [Description("The map modes that will be shown in the quick access bar. " +
                "These must be valid enum values from the MapModeType enum or from any registered plugin map mode enum.")]
   public List<MapModeManager.MapModeType> QuickAccessMapModes
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = new(10);

   [Description("If enabled, MapModeBUttons will automatically have the default map modes assigned to them.")]
   [DefaultValue(true)]
   public bool DefaultAssignMapModes
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;
}