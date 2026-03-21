using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.StyleClasses;

public class MapModeButton : BaseButton
{
   public static readonly MapModeButton[] QuickMapModeButtons = new MapModeButton[10];
   public static readonly Dictionary<ICommand, MapModeManager.MapModeType> CommandToMapModeType = new();

   private bool IsContextTarget { get; set; }
   public int ButtonIndex { get; set; }

   public MapModeManager.MapModeType MapModeType
   {
      get;
      set
      {
         field = value;
         Content = value.ToString();
      }
   }

   private static void ClearContextTarget()
   {
      foreach (var button in QuickMapModeButtons)
         button.IsContextTarget = false;
   }

   protected override void OnMouseUp(MouseButtonEventArgs e)
   {
      if (e.ChangedButton == MouseButton.Left)
         return;

      ClearContextTarget();
      IsContextTarget = true;

      var contextMenu = new ContextMenu();
      foreach (var enumValue in Enum.GetValues<MapModeManager.MapModeType>())
      {
         var menuItem = new MenuItem
         {
            Header = enumValue.ToString(), Command = new RelayCommand(() => { SetMapModeCommand(enumValue, true); }),
         };
         contextMenu.Items.Add(menuItem);
      }

      // Section for VariableMapModes
      AppendVariableMapModePresets(contextMenu);

      if (Config.Settings.MapModeConfig.MapModePresets.Count > 0)
         contextMenu.Items.Add(new Separator());

      // Append all presets if any are available
      foreach (var preset in Config.Settings.MapModeConfig.MapModePresets)
         contextMenu.Items.Add(GetMapModePresetItem(preset));

      // Add option to create new preset from current settings
      contextMenu.Items.Add(new Separator());

      var createPresetItem = new MenuItem
      {
         Header = "New Preset",
         Command = new RelayCommand(() =>
         {
            var modes = new MapModeManager.MapModeType[QuickMapModeButtons.Length];
            for (var i = 0; i < QuickMapModeButtons.Length; i++)
               modes[i] = QuickMapModeButtons[i].MapModeType;

            var presetName = new InputDialog("Map Mode Preset",
                                             "Enter the name for the new MapMode preset:",
                                             InputKind.String) { Owner = Application.Current.MainWindow };
            if (presetName.ShowDialog() != true)
               return;

            var newPreset = new MapModePreset(modes, (string)presetName.Value!);
            Config.Settings.MapModeConfig.MapModePresets.Add(newPreset);
         }),
      };

      contextMenu.Items.Add(createPresetItem);

      contextMenu.IsOpen = true;
      contextMenu.PlacementTarget = this;
   }

   private void AppendVariableMapModePresets(ContextMenu contextMenu)
   {
      var mapMode = MapModeManager.Get(MapModeType);
      if (mapMode is not IVariableSelectionMapMode variableMapMode)
         return;

      var possibelValues = variableMapMode.AvailableTargets.GetEnumerator();
      using var possibelValues1 = possibelValues as IDisposable;
      if (!possibelValues.MoveNext())
         return;

      contextMenu.Items.Add(new Separator());

      var variableMapModeMenu = new MenuItem { Header = "Variable Map Mode Options" };
      do
      {
         var target = possibelValues.Current;
         var targetMenuItem = new MenuItem
         {
            Header = target?.ToString(),
            FontSize = 12,
            Command = new RelayCommand(() =>
            {
               variableMapMode.SelectedTarget = target;
               SetMapModeCommand(MapModeType, true);
            }),
         };
         variableMapModeMenu.Items.Add(targetMenuItem);
      }
      while (possibelValues.MoveNext());

      contextMenu.Items.Add(variableMapModeMenu);
   }

   public void SetMapModeCommand(MapModeManager.MapModeType enumValue, bool render)
   {
      MapModeType = enumValue;

      var mapMode = MapModeManager.Get(enumValue);
      ToolTip = mapMode != null!
                   ? mapMode.Name +
                     '\n' +
                     mapMode.Description +
                     "\nRMB to assign a new map mode.\nShortcut: Ctrl + " +
                     (ButtonIndex != 9 ? ButtonIndex + 1 : 0)
                   : "No map mode assigned to this button.\nRMB to assign a new map mode.\nShortcut: Ctrl + " +
                     (ButtonIndex != 9 ? ButtonIndex + 1 : 0);

      if (render)
      {
         MapModeManager.SetMapMode(enumValue);
         Focus();
      }

      MapModeManager.SetQuickMapModeSetting(ButtonIndex, enumValue);

      if (!CommandToMapModeType.TryAdd(Command, enumValue))
         CommandToMapModeType[Command] = enumValue;
   }

   private static MenuItem GetMapModePresetItem(MapModePreset preset)
   {
      var root = new MenuItem { Header = preset.Name };

      root.Items.Add(new MenuItem
      {
         Header = "Apply",
         Command = new RelayCommand(() =>
         {
            for (var i = 0; i < preset.Modes.Length && i < QuickMapModeButtons.Length; i++)
            {
               var button = QuickMapModeButtons[i];
               button.SetMapModeCommand(preset.Modes[i], button.IsContextTarget);
            }
         }),
      });

      root.Items.Add(new Separator());

      root.Items.Add(new MenuItem
      {
         Header = "Delete", Command = new RelayCommand(() => Config.Settings.MapModeConfig.MapModePresets.Remove(preset)),
      });

      return root;
   }
}