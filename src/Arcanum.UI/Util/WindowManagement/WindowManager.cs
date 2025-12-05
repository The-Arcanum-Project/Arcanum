using System.Diagnostics;
using System.Windows;

namespace Arcanum.UI.Util.WindowManagement;

public static class WindowManager
{
   private const int DEFAULT_MAX_WINDOWS = 1;
   // Makes sure only the max number of allowed instances of a window are opened at the same time
   // If a window is being opened while there are already the max number of windows for it opened, we bring it to the front.

   private static readonly Dictionary<Type, List<Window>> OpenWindows = [];
   private static readonly Dictionary<Type, WindowInformation> WindowInfos = [];

   public static void RegisterWindow(Type windowType, WindowInformation windowInfo) => WindowInfos[windowType] = windowInfo;
   public static void UnregisterWindow(Type windowType) => WindowInfos.Remove(windowType);

   public static bool CanOpenWindow(Type windowType)
   {
      if (!WindowInfos.TryGetValue(windowType, out var windowInfo))
      {
         WindowInfos[windowType] = new(DEFAULT_MAX_WINDOWS, windowType);
         windowInfo = WindowInfos[windowType];
      }

      if (!OpenWindows.ContainsKey(windowType))
         OpenWindows[windowType] = [];

      // If max instances is 0 or less, unlimited instances are allowed
      if (windowInfo.MaxInstances <= 0)
         return true;

      return OpenWindows[windowType].Count < windowInfo.MaxInstances;
   }

   public static void OpenWindow<T>(bool asDialog = false) where T : Window, new()
   {
      OpenWindow(new T(), asDialog);
   }

   public static void OpenWindow(Window window, bool asDialog = false)
   {
      var windowType = window.GetType();
      if (!CanOpenWindow(windowType))
      {
         // Bring the first opened window of this type to the front
         Debug.Assert(OpenWindows.ContainsKey(windowType));
         Debug.Assert(OpenWindows[windowType].Count != 0);

         var existingWindow = OpenWindows[windowType][0];
         existingWindow.Activate();
         return;
      }

      if (!OpenWindows.ContainsKey(windowType))
         OpenWindows[windowType] = [];

      OpenWindows[windowType].Add(window);
      EventHandler onWindowClosed = (_, _) => OpenWindows[windowType].Remove(window);
      window.Unloaded += WindowOnUnloaded;
      window.Closed += onWindowClosed;

      if (Application.Current.Dispatcher.CheckAccess())
         if (asDialog)
            window.ShowDialog();
         else
            window.Show();
      else
         Application.Current.Dispatcher.Invoke(asDialog ? window.ShowDialog : window.Show);

      return;

      void WindowOnUnloaded(object sender, RoutedEventArgs e)
      {
         window.Unloaded -= WindowOnUnloaded;
         window.Closed -= onWindowClosed;
      }
   }
}