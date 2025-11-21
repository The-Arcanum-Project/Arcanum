using Arcanum.UI.Components.Windows.MinorWindows;
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