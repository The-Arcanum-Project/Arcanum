using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Helpers;
using Common.UI.Interfaces;

namespace Arcanum.UI.Components.UIHandles;

public class LogWindowHandleImpl : ILogWindowHandle
{
   private bool _isVisible;
   private LogWindow? _logWindow;

   public void ShowWindow()
   {
      if (_isVisible)
         return;

      if (_logWindow == null)
      {
         _logWindow = new();
         if (ScreenManager.MainScreen is not null)
         {
            _logWindow.Top = ScreenManager.MainScreen.WorkingArea.Top;
            _logWindow.Left = ScreenManager.MainScreen.WorkingArea.Left;
         }

         _logWindow.Show();
      }
      else
      {
         _logWindow.Show();
         _logWindow.Activate();
      }

      _isVisible = true;
   }

   public void HideWindow()
   {
      if (!_isVisible || _logWindow == null)
         return;

      _logWindow.Hide();
      _isVisible = false;
   }

   public void CloseWindow()
   {
      if (_logWindow == null)
         return;

      _logWindow.Close();
      _logWindow = null;
      _isVisible = false;
   }
}