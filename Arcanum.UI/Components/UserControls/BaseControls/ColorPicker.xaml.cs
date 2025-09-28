using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class ColorPicker
{
   private ColorPickerViewModel? ViewModel => DataContext as ColorPickerViewModel;

   public ColorPicker()
   {
      InitializeComponent();

      DataContextChanged += OnDataContextChanged;

      Loaded += OnColorPickerLoaded;
   }

   private void OnColorPickerLoaded(object sender, RoutedEventArgs e)
   {
      UpdateThumbPosition();
   }

   private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
   {
      if (e.OldValue is ColorPickerViewModel oldVm)
         oldVm.PropertyChanged -= ViewModel_PropertyChanged;

      if (e.NewValue is ColorPickerViewModel newVm)
      {
         newVm.PropertyChanged += ViewModel_PropertyChanged;

         if (IsLoaded)
            UpdateThumbPosition();
      }
   }

   private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      UpdateThumbPosition();
   }

   private void ColorSquare_MouseDown(object sender, MouseButtonEventArgs e)
   {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
         ColorSquareCanvas.CaptureMouse();
         UpdateColorFromMousePosition(e.GetPosition(ColorSquareCanvas));
      }
   }

   private void ColorSquare_MouseMove(object sender, MouseEventArgs e)
   {
      if (ColorSquareCanvas.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
         UpdateColorFromMousePosition(e.GetPosition(ColorSquareCanvas));
   }

   private void ColorSquare_MouseUp(object sender, MouseButtonEventArgs e)
   {
      ColorSquareCanvas.ReleaseMouseCapture();
   }

   private void UpdateColorFromMousePosition(Point position)
   {
      if (ViewModel == null)
         return;

      var x = Math.Clamp(position.X, 0, ColorSquareCanvas.ActualWidth);
      var y = Math.Clamp(position.Y, 0, ColorSquareCanvas.ActualHeight);

      Canvas.SetLeft(ColorSelectorThumb, x - ColorSelectorThumb.Width / 2);
      Canvas.SetTop(ColorSelectorThumb, y - ColorSelectorThumb.Height / 2);

      var saturation = x / ColorSquareCanvas.ActualWidth;
      var value = 1.0 - y / ColorSquareCanvas.ActualHeight;

      ViewModel.Saturation = saturation;
      ViewModel.Value = value;
   }

   private void UpdateThumbPosition()
   {
      if (ViewModel == null || ColorSquareCanvas.ActualWidth == 0)
         return;

      var x = ViewModel.Saturation * ColorSquareCanvas.ActualWidth;
      var y = (1.0 - ViewModel.Value) * ColorSquareCanvas.ActualHeight;

      Canvas.SetLeft(ColorSelectorThumb, x - ColorSelectorThumb.Width / 2);
      Canvas.SetTop(ColorSelectorThumb, y - ColorSelectorThumb.Height / 2);
   }

   private void HueSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is not Slider slider ||
          slider.Template.FindName("PART_Track", slider) is not Track { Thumb: { } thumb })
         return;

      if (Equals(e.OriginalSource, thumb))
         return;

      var clickPosition = e.GetPosition(slider);
      var proportion = 1.0 - clickPosition.Y / slider.ActualHeight;
      var newValue = slider.Minimum + proportion * (slider.Maximum - slider.Minimum);
      slider.Value = newValue;

      slider.UpdateLayout();

      thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
      {
         RoutedEvent = MouseLeftButtonDownEvent, Source = e.Source,
      });

      e.Handled = true;
   }
}