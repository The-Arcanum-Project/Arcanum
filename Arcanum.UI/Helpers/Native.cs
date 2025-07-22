using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Arcanum.UI.Helpers;

public static class NativeMethods
{
   
   // Constants
   public const int WM_GETMINMAXINFO = 0x0024;
   private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
   
   
   [StructLayout(LayoutKind.Sequential)] 
   public struct Rect {
      public int Left; 
      public int Top; 
      public int Right;
      public int Bottom; 
   }
   
   [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto, Pack=4)]
   private class MonitorInfoEx { 
      public int     cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
      public Rect    rcMonitor = new Rect(); 
      public Rect    rcWork = new Rect(); 
      public int     dwFlags = 0;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] 
      public char[]  szDevice = new char[32];
   }
   
   [DllImport("user32.dll")]
   private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);
   
   [DllImport("User32.dll", CharSet=CharSet.Auto)]
   private static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out]MonitorInfoEx info);
   
   [DllImport("user32.dll")]
   public static extern uint GetDoubleClickTime();
   
   public static MinMaxInfo GetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
   {
      var mmi = (MinMaxInfo)(Marshal.PtrToStructure(lParam, typeof(MinMaxInfo)) ?? throw new InvalidOperationException());
      var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
      
      if (monitor == IntPtr.Zero) return mmi;
      var monitorInfo = new MonitorInfoEx();
      GetMonitorInfo(new HandleRef(null, monitor), monitorInfo); 
      var rcWorkArea = monitorInfo.rcWork;
      var rcMonitorArea = monitorInfo.rcMonitor;
      mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
      mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
      mmi.ptMaxSize.x = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
      mmi.ptMaxSize.y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
      
      var source = HwndSource.FromHwnd(hwnd);
      if (source is not { RootVisual: Window window, CompositionTarget: not null }) return mmi;

      var matrix = source.CompositionTarget.TransformToDevice;
      var minWidth = (int)(window.MinWidth * matrix.M11);
      var minHeight = (int)(window.MinHeight * matrix.M22);

      mmi.ptMinTrackSize.x = minWidth;
      mmi.ptMinTrackSize.y = minHeight;

      return mmi;
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
}
