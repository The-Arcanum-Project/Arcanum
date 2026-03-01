using System.Windows;
using Arcanum.UI.Helpers;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class UIUtilsImpl : IUIUtils
{
   public void OpenWindowOnSTAThread(Window window, bool asDialog)
   {
      if (Application.Current.Dispatcher.CheckAccess())
         window.Show();
      else
         Application.Current.Dispatcher.Invoke(asDialog ? window.ShowDialog : window.Show);
   }

   public void SetStartupScreen(bool force)
   {
      ScreenManager.SetMainScreen(force);
   }
}