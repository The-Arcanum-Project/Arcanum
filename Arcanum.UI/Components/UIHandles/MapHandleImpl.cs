using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;
using Vortice.Mathematics;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{

    public void SetColor(int[] colors)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Application.Current.MainWindow is not MainWindow mainWindow) return;
            var colorArray = new Color4[colors.Length];

            for (var index = 0; index < colors.Length; index++)
            {
                colorArray[index] = new(colors[index]);
            }

            mainWindow.MainMap.SetColors(colorArray);
        });
    }

    private static void NotifyMapLoadedInternal()
    {
        if (Application.Current.MainWindow is not MainWindow mainWindow) return;
        if(DescriptorDefinitions.MapTracingDescriptor.LoadingService[0] is not LocationMapTracing tracing)
            throw new ApplicationException("MapHandleImpl.NotifyMapLoaded");
        _ = mainWindow.MainMap.SetupRenderer(tracing.Polygons, tracing.MapSize);
    }

    public static void LoadMap()
    {
        Application.Current.Dispatcher.Invoke(NotifyMapLoadedInternal);
    }

    public void NotifyMapLoaded()
    {
        Application.Current.Dispatcher.Invoke(NotifyMapLoadedInternal);
    }
}