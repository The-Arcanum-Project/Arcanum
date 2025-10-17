using System.IO;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.UI.Components.Windows.MainWindows;
using Common.UI;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class MainWindowHandleImpl : IMainWindowsHandle
{
   public event Action? OnOpenMainMenuScreen;

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

   public void TransferToMainMenuScreen(Window sender,
                                        Enum view)
   {
      ParsingMaster.UnloadAll();
      FileStateManager.Shutdown();
      OnOpenMainMenuScreen?.Invoke();
      var mw = new MainMenuScreen { MainMenuViewModel = { TargetedView = (MainMenuScreen.MainMenuScreenView)view } };
      Application.Current.MainWindow = mw;
      Application.Current.MainWindow.Show();
      mw.Activate();
      sender.Close();
   }
}