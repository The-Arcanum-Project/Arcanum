using System.Reflection;
using System.Windows;
using Arcanum.API.UI;
using Arcanum.API.UtilServices;
using Arcanum.UI.Components.UIHandles;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI;

namespace Arcanum.UI.Components.WindowLinker;

public class InClassName(PropertyInfo info)
{
   public PropertyInfo Info { get; } = info;
}

public class WindowLinkerImpl : IWindowLinker
{
   public void OpenPropertyGridWindow(object obj)
   {
      UIHandle.Instance.UIUtils.OpenWindowOnSTAThread(GetPropertyGridOrCollectionView(obj), true);
   }

   public void OpenMainMenuScreen()
   {
      if (Application.Current.MainWindow is MainMenuScreen mainMenuScreen)
      {
         mainMenuScreen.Activate();
         return;
      }

      var mainMenu = new MainMenuScreen();
      Application.Current.MainWindow = mainMenu;
      // Close all other windows
      foreach (var window in Application.Current.Windows)
         if (window is not MainMenuScreen)
            ((Window)window).Close();
      UIHandle.Instance.UIUtils.OpenWindowOnSTAThread(mainMenu, false);
      mainMenu.Activate();
   }

   public MBoxResult ShowMBox(string message,
                              string title = "Message",
                              MBoxButton buttons = MBoxButton.OK,
                              MessageBoxImage icon = MessageBoxImage.Asterisk,
                              int height = 150,
                              int width = 400)
   {
      return MBox.Show(message, title, buttons, icon, height, width);
   }

   public Window GetPropertyGridOrCollectionView(object? obj)
   {
      if (obj is null)
         throw new ArgumentNullException(nameof(obj), "Object cannot be null");

      if (obj is System.Collections.IEnumerable enumerable)
         return new BaseCollectionView(enumerable);

      return new PropertyGridWindow(obj);
   }

}