using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using Image = System.Windows.Controls.Image;

namespace Arcanum.UI.Components.Base;

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


public class BaseWindow : Window
{
   public ICommand CloseCommand { get; }
   public ICommand MinimizeCommand { get; }
   public ICommand MaximizeRestoreCommand { get; }
   
   static BaseWindow()
   {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow),
                                               new FrameworkPropertyMetadata(typeof(BaseWindow)));
   }

   protected BaseWindow()
   {
      StateChanged += MainWindow_OnStateChanged;
      CloseCommand = new RelayCommand(Close);
      MinimizeCommand = new RelayCommand(Minimize);
      MaximizeRestoreCommand = new RelayCommand(MaximizeRestore);
      SourceInitialized += OnSourceInitialized;
   }

   public override void OnApplyTemplate()
   {
      base.OnApplyTemplate();
      UpdateHeaderSeparator();
   }

   protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
   {
      base.OnPropertyChanged(e);

      if (e.Property == HeaderContentProperty)
         UpdateHeaderSeparator();
   }

   private void UpdateHeaderSeparator()
   {
      var presenter = GetTemplateChild("HeaderContentPresenter") as ContentPresenter;
      var separator = GetTemplateChild("HeaderSeparator") as Rectangle;

      if (presenter != null && separator != null)
      {
         separator.Visibility = presenter.Content != null ? Visibility.Visible : Visibility.Collapsed;
      }
   }

   // Define the DependencyProperty for the Header
   public static readonly DependencyProperty HeaderContentProperty =
      DependencyProperty.Register(nameof(HeaderContent),
                                  typeof(object),
                                  typeof(BaseWindow),
                                  new(null));

   // Create a CLR wrapper for the HeaderContent property
   public object HeaderContent
   {
      get => GetValue(HeaderContentProperty);
      set => SetValue(HeaderContentProperty, value);
   }   
   
   public GridLength HeaderHeight
   {
      get => (GridLength)GetValue(HeaderHeightProperty);
      set => SetValue(HeaderHeightProperty, value);
   } 
   
   public GridLength FooterHeight
   {
      get => (GridLength)GetValue(FooterHeightProperty);
      set => SetValue(FooterHeightProperty, value);
   }
   
   public static readonly DependencyProperty FooterHeightProperty =
      DependencyProperty.Register(nameof(FooterHeight),
                                  typeof(GridLength),
                                  typeof(BaseWindow),
                                  new(new GridLength(0.0)));
   
   public static readonly DependencyProperty HeaderHeightProperty =
      DependencyProperty.Register(nameof(HeaderHeight),
                                  typeof(GridLength),
                                  typeof(BaseWindow),
                                  new(new GridLength(30.0)));
   
   

   // Define the DependencyProperty for the Footer
   public static readonly DependencyProperty FooterContentProperty =
      DependencyProperty.Register(nameof(FooterContent),
                                  typeof(object),
                                  typeof(BaseWindow),
                                  new(null));

   // Create a CLR wrapper for the FooterContent property
   public object FooterContent
   {
      get => GetValue(FooterContentProperty);
      set => SetValue(FooterContentProperty, value);
   }

   // Bonus: Add properties for DataTemplates for even more flexibility
   public static readonly DependencyProperty HeaderContentTemplateProperty =
      DependencyProperty.Register(nameof(HeaderContentTemplate),
                                  typeof(DataTemplate),
                                  typeof(BaseWindow),
                                  new(null));

   public DataTemplate HeaderContentTemplate
   {
      get => (DataTemplate)GetValue(HeaderContentTemplateProperty);
      set => SetValue(HeaderContentTemplateProperty, value);
   }

   public static readonly DependencyProperty FooterContentTemplateProperty =
      DependencyProperty.Register(nameof(FooterContentTemplate),
                                  typeof(DataTemplate),
                                  typeof(BaseWindow),
                                  new(null));

   public DataTemplate FooterContentTemplate
   {
      get => (DataTemplate)GetValue(FooterContentTemplateProperty);
      set => SetValue(FooterContentTemplateProperty, value);
   }

   private void Minimize()
   {
      WindowState = WindowState.Minimized;
   }

   private void MaximizeRestore()
   {
      if (WindowState == WindowState.Normal)
         WindowState = WindowState.Maximized;
      else
         WindowState = WindowState.Normal;
   }
   
   private void MainWindow_OnStateChanged(object? sender, EventArgs e)
   {
      if (GetTemplateChild("StateImage") is not Image image)
         return;
      
      var imagePath = WindowState switch
      {
         WindowState.Maximized => "../../Assets/Icons/RestoreWindow.png",
         _ => "../../Assets/Icons/FullScreen.png",
      };

      image.Source = new BitmapImage(new(imagePath, UriKind.Relative));
   }
   private void OnSourceInitialized(object? sender, EventArgs e)
   {
      if (ResizeMode != ResizeMode.NoResize && new WindowInteropHelper(this).Handle != IntPtr.Zero)
      {
         HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WindowProc);
      }
   }

   private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
   {
      if (msg == NativeMethods.WM_GETMINMAXINFO)
      {
         // Let our helper class handle the logic
         var mmi = NativeMethods.GetMinMaxInfo(hwnd, lParam);
         Marshal.StructureToPtr(mmi, lParam, true);
         handled = true;
      }
      return IntPtr.Zero;
   }
}