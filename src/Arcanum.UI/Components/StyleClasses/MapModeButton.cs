using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.GlobalStates;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.StyleClasses;

public class MapModeButton : BaseButton
{
   public static Dictionary<ICommand, MapModeManager.MapModeType> CommandToMapModeType = new();

   public int ButtonIndex { get; set; }

   public MapModeManager.MapModeType MapModeType
   {
      get;
      set
      {
         field = value;
         Content = value.ToString();
         InvalidateVisual();
      }
   }

   protected override void OnMouseUp(MouseButtonEventArgs e)
   {
      if (e.ChangedButton == MouseButton.Left)
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
               MapModeManager.SetMapMode(enumValue);
               if (Config.Settings.MapModeConfig.QuickAccessMapModes.Count > ButtonIndex)
                  Config.Settings.MapModeConfig.QuickAccessMapModes[ButtonIndex] = enumValue;
               else
                  Config.Settings.MapModeConfig.QuickAccessMapModes
                        .Add(enumValue); // Weird fallback but will do for now.
               if (!CommandToMapModeType.TryAdd(Command, enumValue))
                  CommandToMapModeType[Command] = enumValue;
            }),
         };
         contextMenu.Items.Add(menuItem);
      }

      contextMenu.IsOpen = true;
      contextMenu.PlacementTarget = this;
   }
}