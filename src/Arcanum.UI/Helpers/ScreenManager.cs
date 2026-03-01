using System.Windows;

namespace Arcanum.UI.Helpers;

public static class ScreenManager
{
    public static NativeMethods.ScreenInfo? MainScreen { get; set; }
    
    public static void SetMainScreen(bool force = false)
    {
        if (!NativeMethods.GetCursorPos(out var point))
        {
            if (!force) return;
            point = new()
            {
                x = 0,
                y = 0
            };
        }

        MainScreen = NativeMethods.GetCurrentMonitorRect(point);

    }
    
    extension(Window window)
    {
        public void SetScreen()
        {
            if (MainScreen == null) return;
            var workingArea = MainScreen.WorkingArea;
            window.Left = workingArea.Left + (workingArea.Width - window.Width) / 2;
            window.Top = workingArea.Top + (workingArea.Height - window.Height) / 2;
        }
        
        public void SetScreenOffset(int offsetX, int offsetY, int width, int height)
        {
            if (MainScreen == null) return;
            var workingArea = MainScreen.WorkingArea;

            if (workingArea.Right < workingArea.Left + offsetX + width ||
                workingArea.Bottom < workingArea.Top + offsetY + height)
            {
                // The window would go off the screen, so we need to adjust the offset
                // If width or height is larger than working area limit them to the working area
                if (width > workingArea.Width)
                    width = workingArea.Width;
                if (height > workingArea.Height)
                    height = workingArea.Height;
                
                // center the window on the screen
                offsetX = ((workingArea.Width - width) / 2);
                offsetY = ((workingArea.Height - height) / 2);
            }
            window.Width = width;
            window.Height = height;
            window.Left = workingArea.Left + offsetX;
            window.Top = workingArea.Top + offsetY;
        }
        
        public void SetScreen(int minWidth, int minHeight)
        {
            if (MainScreen == null) return;
        
            if (MainScreen.WorkingArea.Width <= minWidth || MainScreen.WorkingArea.Height <= minHeight)
            {
                window.Height = MainScreen.WorkingArea.Height * 0.8;
                window.Width = MainScreen.WorkingArea.Width * 0.8;
                window.WindowState = WindowState.Maximized;
            }

            window.SetScreen();
        }

        public (int, int) GetRelativePosition()
        {
            // Get the position of the window relative to the screen of the window
            var rect = NativeMethods.GetCurrentMonitorRect(window, true);
            
            return ((int, int))(window.Left - rect.Left, window.Top - rect.Top);
        }
    }
}