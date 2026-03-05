using System.Windows;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{
   private static void NotifyMapLoadedInternal()
   {
      if (Application.Current.MainWindow is MainWindow mainWindow)
         mainWindow.TryLoadMapData();
   }

   public void NotifyMapLoaded()
   {
      Application.Current.Dispatcher.BeginInvoke(NotifyMapLoadedInternal);
   }
}