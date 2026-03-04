using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;
using Common.UI.Map;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{
   private static bool NotifyMapLoadedInternal(MapParsingData data)
   {
      if (Application.Current.MainWindow is not MainWindow mainWindow || MapModeManager.IsMapReady)
         return false;

      Debug.Assert(data.Polygons is not null);
      _ = mainWindow.MainMap.SetupRenderer(data.Polygons, data.MapSize);
      MapModeManager.IsMapReady = true;
      
      return true;
   }

   public bool NotifyMapLoaded(MapParsingData data)
   {
      return Application.Current.Dispatcher.Invoke(() => NotifyMapLoadedInternal(data));
   }
}