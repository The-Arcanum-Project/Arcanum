using System.Windows;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.UI.Components.Helpers;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class JominiColorView
{
   private bool _isReadOnly;

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

   public JominiColorView(JominiColor color, bool isReadOnly) : this()
   {
      Color = color;
      _isReadOnly = isReadOnly;
      if (isReadOnly)
         ColorButton.ToolTip = "This field is read-only.";
   }

   private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (JominiColorView)d;
      if (e.NewValue is JominiColor newColor)
         control.UpdateColorUI(newColor);
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (_isReadOnly)
         return;

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