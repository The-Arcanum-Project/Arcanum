using System.ComponentModel;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapModeConfig() : InternalSearchableSetting(Config.Settings)
{
   private bool _defaultAssignMapModes = true;
   private List<MapModeManager.MapModeType> _quickAccessMapModes = new(10);
   private int _numOfMapModeButtons = 10;
   [Description("The number of map mode buttons to show in the map mode selection UI.")]
   [DefaultValue(10)]
   public int NumOfMapModeButtons
   {
      get => _numOfMapModeButtons;
      set => SetNotifyProperty(ref _numOfMapModeButtons, value);
   }

   [Description("The map modes that will be shown in the quick access bar. " +
                "These must be valid enum values from the MapModeType enum or from any registered plugin map mode enum.")]
   public List<MapModeManager.MapModeType> QuickAccessMapModes
   {
      get => _quickAccessMapModes;
      set => SetNotifyProperty(ref _quickAccessMapModes, value);
   }

   [Description("If enabled, MapModeBUttons will automatically have the default map modes assigned to them.")]
   [DefaultValue(true)]
   public bool DefaultAssignMapModes
   {
      get => _defaultAssignMapModes;
      set => SetNotifyProperty(ref _defaultAssignMapModes, value);
   }
}