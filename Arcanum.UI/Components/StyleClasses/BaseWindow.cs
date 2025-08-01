using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.UI.Helpers;
using CommunityToolkit.Mvvm.Input;
using Image = System.Windows.Controls.Image;

namespace Arcanum.UI.Components.StyleClasses;

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

   public static readonly DependencyProperty ShowMinimizeButtonProperty =
      DependencyProperty.Register(nameof(ShowMinimizeButton),
                                  typeof(bool),
                                  typeof(BaseWindow),
                                  new(true));

   public bool ShowMinimizeButton
   {
      get => (bool)GetValue(ShowMinimizeButtonProperty);
      set => SetValue(ShowMinimizeButtonProperty, value);
   }
   
   public static readonly DependencyProperty ShowMaximizeButtonProperty =
      DependencyProperty.Register(nameof(ShowMaximizeButton),
                                  typeof(bool),
                                  typeof(BaseWindow),
                                  new(true));
   
   public bool ShowMaximizeButton
   {
      get => (bool)GetValue(ShowMaximizeButtonProperty);
      set => SetValue(ShowMaximizeButtonProperty, value);
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
         WindowState.Maximized => "pack://application:,,,/Assets/Icons/20x20/RestoreWindow20x20.png",
         _ => "pack://application:,,,/Assets/Icons/20x20/FullScreen20x20.png",
      };

      image.Source = new BitmapImage(new(imagePath));
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