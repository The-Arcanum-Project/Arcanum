using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

   public BaseWindow()
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
      ShowMaximizeButton &= ResizeMode != ResizeMode.NoResize && ResizeMode != ResizeMode.CanMinimize;
      ShowMinimizeButton &= ResizeMode != ResizeMode.NoResize;
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
         separator.Visibility = presenter.Content != null ? Visibility.Visible : Visibility.Collapsed;
   }

   // Define the DependencyProperty for the Header
   public static readonly DependencyProperty HeaderContentProperty =
      DependencyProperty.Register(nameof(HeaderContent),
                                  typeof(object),
                                  typeof(BaseWindow),
                                  new(null));

   public static readonly DependencyProperty VersionNumberProperty =
      DependencyProperty.Register(nameof(VersionNumber),
                                  typeof(string),
                                  typeof(BaseWindow),
                                  new(default(string?)));

   public string? VersionNumber
   {
      get => (string?)GetValue(VersionNumberProperty);
      set => SetValue(VersionNumberProperty, value);
   }

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
                                  new(new GridLength(32.0)));

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

   // Step 1: Assume your source Dependency Properties are already defined.
   // They MUST have a PropertyChangedCallback to trigger the update.

   public static readonly DependencyProperty ShowWindowBorderProperty =
      DependencyProperty.Register(nameof(ShowWindowBorder),
                                  typeof(bool),
                                  typeof(BaseWindow),
                                  new(true, OnDependencyChanged));

   public bool ShowWindowBorder
   {
      get => (bool)GetValue(ShowWindowBorderProperty);
      set => SetValue(ShowWindowBorderProperty, value);
   }

   public static readonly DependencyProperty WindowCornerRadiusProperty =
      DependencyProperty.Register(nameof(WindowCornerRadius),
                                  typeof(CornerRadius),
                                  typeof(BaseWindow),
                                  new(default(CornerRadius), OnDependencyChanged));

   public CornerRadius WindowCornerRadius
   {
      get => (CornerRadius)GetValue(WindowCornerRadiusProperty);
      set => SetValue(WindowCornerRadiusProperty, value);
   }

   public static readonly DependencyProperty WindowBorderThicknessProperty =
      DependencyProperty.Register(nameof(WindowBorderThickness),
                                  typeof(Thickness),
                                  typeof(BaseWindow),
                                  new(default(Thickness), OnDependencyChanged));

   public Thickness WindowBorderThickness
   {
      get => (Thickness)GetValue(WindowBorderThicknessProperty);
      set => SetValue(WindowBorderThicknessProperty, value);
   }

   public Brush HeaderBackGroundBrush
   {
      get => (Brush)GetValue(WindowCornerRadiusProperty);
      set => SetValue(HeaderBackGroundBrushProperty, value);
   }

   public static readonly DependencyProperty HeaderBackGroundBrushProperty =
      DependencyProperty.Register(nameof(HeaderBackGroundBrush),
                                  typeof(Brush),
                                  typeof(BaseWindow),
                                  new(Brushes.Transparent, OnDependencyChanged));

   public static readonly DependencyProperty HeaderBorderProperty =
      DependencyProperty.Register(nameof(HeaderBorder),
                                  typeof(Thickness),
                                  typeof(BaseWindow),
                                  new(default(Thickness)));

   public static readonly DependencyProperty HeaderMarginProperty =
      DependencyProperty.Register(nameof(HeaderMargin), typeof(Thickness), typeof(BaseWindow), new(default(Thickness)));

   public Thickness HeaderMargin
   {
      get => (Thickness)GetValue(HeaderMarginProperty);
      set => SetValue(HeaderMarginProperty, value);
   }

   public Thickness HeaderBorder
   {
      get => (Thickness)GetValue(HeaderBorderProperty);
      set => SetValue(HeaderBorderProperty, value);
   }

   // This single callback will handle changes for all source properties.
   private static void OnDependencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      // When a source property changes, we need to force the system to re-evaluate
      // our calculated property. CoerceValue is the perfect tool for this.
      d.CoerceValue(WindowTransparencyProperty);
   }

   private static readonly DependencyPropertyKey WindowTransparencyPropertyKey =
      DependencyProperty.RegisterReadOnly("WindowTransparency",
                                          typeof(bool),
                                          typeof(BaseWindow),
                                          new FrameworkPropertyMetadata(false,
                                                                        null,
                                                                        CoerceWindowTransparencyValue)); // The COERCE callback does all the work!

   // The public, read-only DependencyProperty identifier.
   public static readonly DependencyProperty WindowTransparencyProperty =
      WindowTransparencyPropertyKey.DependencyProperty;

   // The Coerce Callback that contains your calculation logic.
   // It is called automatically on initialization and whenever CoerceValue is called.
   private static object CoerceWindowTransparencyValue(DependencyObject d, object baseValue)
   {
      var window = (BaseWindow)d;

      return window.ShowWindowBorder &&
             window.WindowCornerRadius != default &&
             window.WindowBorderThickness != default;
   }

   // Note it only has a 'get' accessor, making it read-only to consumers.
   public bool WindowTransparency => (bool)GetValue(WindowTransparencyProperty);

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
         WindowState.Maximized => "/Arcanum_UI;component/Assets/Icons/20x20/RestoreWindow20x20.png",
         _ => "/Arcanum_UI;component/Assets/Icons/20x20/FullScreen20x20.png",
      };

      image.Source = new BitmapImage(new(imagePath, UriKind.RelativeOrAbsolute));
   }

   private void OnSourceInitialized(object? sender, EventArgs e)
   {
      if (ResizeMode != ResizeMode.NoResize && new WindowInteropHelper(this).Handle != IntPtr.Zero)
         HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(WindowProc);
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