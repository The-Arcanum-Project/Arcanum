using System.Windows;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MainWindowHandleImpl : IMainWindowsHandle
{
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
}