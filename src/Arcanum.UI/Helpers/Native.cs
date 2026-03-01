using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Arcanum.UI.Helpers;

public static class NativeMethods
{
    #region Constants

    public const int WM_GETMINMAXINFO = 0x0024;
    private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInfo
    {
        public Point ptReserved;
        public Point ptMaxSize;
        public Point ptMaxPosition;
        public Point ptMinTrackSize;
        public Point ptMaxTrackSize;
    }

    public record ScreenInfo(Rect WorkingArea, Rect MonitorArea);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private class MonitorInfoEx
    {
        public int cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));

        public readonly Rect rcMonitor = new();
        public readonly Rect rcWork = new();
#pragma warning disable CS0414 // Field is assigned but its value is never used
        public int dwFlags = 0;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
    }

    #endregion

    #region private methods

    private static bool GetScreenInfoFromMonitor(IntPtr monitor, out ScreenInfo screenInfo)
    {
        var monitorInfo = new MonitorInfoEx();

        if (GetMonitorInfo(new(null, monitor), monitorInfo))
        {
            screenInfo = new(monitorInfo.rcMonitor, monitorInfo.rcMonitor);
            return true;
        }

        screenInfo = new(new(), new());
        return false;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(Point pt, int dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MonitorInfoEx info);

    #endregion

    [DllImport("user32.dll")]
    public static extern uint GetDoubleClickTime();

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point cursorPoint);

    public static bool GetCurrentScreen(Point point, out ScreenInfo screenInfo)
    {
        var monitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        return GetScreenInfoFromMonitor(monitor, out screenInfo);
    }

    public static bool GetCurrentScreen(Window window, out ScreenInfo screenInfo)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        return GetScreenInfoFromMonitor(monitor, out screenInfo);
    }

    public static MinMaxInfo GetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        var mmi =
            (MinMaxInfo)(Marshal.PtrToStructure(lParam, typeof(MinMaxInfo)) ?? throw new InvalidOperationException());
        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (monitor == IntPtr.Zero)
            return mmi;

        var monitorInfo = new MonitorInfoEx();
        GetMonitorInfo(new(null, monitor), monitorInfo);
        var rcWorkArea = monitorInfo.rcWork;
        var rcMonitorArea = monitorInfo.rcMonitor;
        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);

        var source = HwndSource.FromHwnd(hwnd);
        if (source is not { RootVisual: Window window, CompositionTarget: not null })
            return mmi;

        var matrix = source.CompositionTarget.TransformToDevice;
        var minWidth = (int)(window.MinWidth * matrix.M11);
        var minHeight = (int)(window.MinHeight * matrix.M22);

        mmi.ptMinTrackSize.x = minWidth;
        mmi.ptMinTrackSize.y = minHeight;

        return mmi;
    }
}