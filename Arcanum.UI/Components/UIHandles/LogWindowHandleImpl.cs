using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.MinorWindows;

namespace Arcanum.UI.Components.UIHandles;

public class LogWindowHandleImpl : Common.UI.Interfaces.ILogWindowHandle
{
   private bool _isVisible;
   private LogWindow? _logWindow;

   public void ShowWindow()
   {
      if (_isVisible)
         return;

      if (_logWindow == null)
      {
         _logWindow = new LogWindow();
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
}