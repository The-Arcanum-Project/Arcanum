using System.Windows.Interop;

namespace Arcanum.Core.Utils.ScreenManagement;

using System.Windows;

public static class ScreenManager
{
   public static Screen MainScreen { get; set; } = Screen.FromHandle(new WindowInteropHelper(new()).Handle);

   public static Screen GetScreenFrom(Window window)
   {
      var handle = new WindowInteropHelper(window).Handle;
      return Screen.FromHandle(handle);
   }

   public static void SetScreen(this Window window, Screen screen)
   {
      var workingArea = screen.WorkingArea;
      window.Left = workingArea.Left + (workingArea.Width - window.Width) / 2;
      window.Top = workingArea.Top + (workingArea.Height - window.Height) / 2;
   }
}