using System.Windows;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{
    public void NotifyMapLoaded()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.MainMap.SetupRendering();
        }
    }
}