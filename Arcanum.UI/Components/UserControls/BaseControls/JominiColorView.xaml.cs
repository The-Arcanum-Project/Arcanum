using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.UI.Components.Helpers;
using Arcanum.UI.Components.Windows.MinorWindows;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class JominiColorView
{
   public static readonly DependencyProperty ColorProperty =
      DependencyProperty.Register(nameof(Color),
                                  typeof(JominiColor),
                                  typeof(JominiColorView),
                                  new FrameworkPropertyMetadata(JominiColor.Empty,
                                                                FrameworkPropertyMetadataOptions
                                                                  .BindsTwoWayByDefault,
                                                                OnColorChanged));

   public JominiColor Color
   {
      get => (JominiColor)GetValue(ColorProperty);
      set => SetValue(ColorProperty, value);
   }

   public JominiColorView()
   {
      InitializeComponent();
   }

   public JominiColorView(JominiColor color) : this()
   {
      Color = color;
   }

   private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (JominiColorView)d;
      if (e.NewValue is JominiColor newColor)
         control.UpdateColorUI(newColor);
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      var buttonBottomRightCorner = ColorButton.PointToScreen(new(ColorButton.ActualWidth, ColorButton.ActualHeight));
      var result = PopupService.ShowColorPicker(Color.ToMediaColor(), buttonBottomRightCorner);

      if (result.Confirmed)
         Color = new JominiColor.MediaColor(result.Value);
   }

   private void UpdateColorUI(JominiColor color)
   {
      ColorBorder.Background = new SolidColorBrush(color.ToMediaColor());
      ColorTextBlock.Text = color.ToString();
   }
}