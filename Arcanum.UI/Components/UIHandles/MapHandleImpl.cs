using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{

    private void NotifyMapLoadedInternal()
    {
        if (Application.Current.MainWindow is not MainWindow mainWindow) return;
        if(DescriptorDefinitions.MapTracingDescriptor.LoadingService[0] is not LocationMapTracing tracing)
            throw new ApplicationException("MapHandleImpl.NotifyMapLoaded");
        mainWindow.MainMap.SetupRendering(tracing.polygons, tracing.mapSize);
    }

    public void NotifyMapLoaded()
    {
        Application.Current.Dispatcher.Invoke(NotifyMapLoadedInternal);
    }
}