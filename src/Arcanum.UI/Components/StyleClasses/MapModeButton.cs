using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.GlobalStates;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.StyleClasses;

public class MapModeButton : BaseButton
{
   private MapModeManager.MapModeType _mapModeType;
   public int ButtonIndex { get; set; }
   public MapModeManager.MapModeType MapModeType
   {
      get => _mapModeType;
      set
      {
         _mapModeType = value;
         Content = value.ToString();
         InvalidateVisual();
      }
   }

   protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
   {
      if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
         return;

      var contextMenu = new ContextMenu();
      foreach (var enumValue in Enum.GetValues<MapModeManager.MapModeType>())
      {
         var menuItem = new MenuItem
         {
            Header = enumValue.ToString(),
            Command = new RelayCommand(() =>
            {
               MapModeType = enumValue;
               MapModeManager.Activate(enumValue);
               if (Config.Settings.MapModeConfig.QuickAccessMapModes.Count > ButtonIndex)
                  Config.Settings.MapModeConfig.QuickAccessMapModes[ButtonIndex] = enumValue;
               else
                  Config.Settings.MapModeConfig.QuickAccessMapModes
                        .Add(enumValue); // Weird fallback but will do for now.
            }),
         };
         contextMenu.Items.Add(menuItem);
      }

      contextMenu.IsOpen = true;
      contextMenu.PlacementTarget = this;
   }
}