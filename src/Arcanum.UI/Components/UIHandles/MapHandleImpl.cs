using System.Diagnostics;
using System.Windows;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MapHandleImpl : IMapHandle
{
   private static void NotifyMapLoadedInternal()
   {
      if (Application.Current.MainWindow is not MainWindow mainWindow || MapModeManager.IsMapReady)
         return;

      if (DescriptorDefinitions.MapTracingDescriptor.LoadingService[0] is not LocationMapTracing tracing)
         throw new ApplicationException("MapHandleImpl.NotifyMapLoaded");

      Debug.Assert(tracing.Polygons is not null);
      _ = mainWindow.MainMap.SetupRenderer(tracing.Polygons!, tracing.MapSize);
      MapModeManager.IsMapReady = true;
   }

   public void NotifyMapLoaded()
   {
      Application.Current.Dispatcher.BeginInvoke(NotifyMapLoadedInternal);
   }
}