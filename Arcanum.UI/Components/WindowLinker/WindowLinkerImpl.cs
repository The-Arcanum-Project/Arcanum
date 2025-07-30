using System.Windows;
using Arcanum.API.UtilServices;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.Components.Windows.PopUp;

namespace Arcanum.UI.Components.WindowLinker;

public class WindowLinkerImpl : IWindowLinker
{
   public void OpenPropertyGridWindow(object obj)
   {
      new PropertyGridWindow(obj).ShowDialog();
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
      mainMenu.Show();
      mainMenu.Activate();
   }
}