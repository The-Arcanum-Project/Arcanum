using System.IO;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
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

   public void SetToNui(object obj)
   {
      if (obj is not IEu5Object eu5Obj)
      {
#if DEBUG
         throw new InvalidDataException("Object is not of type IEu5Object");
#endif
         return;
      }

      if (Application.Current.MainWindow is not MainWindow mw)
         return;

      Eu5UiGen.GenerateAndSetView(new(eu5Obj, true, mw.UiPresenter));
   }
}