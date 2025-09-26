using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.UI.Components.Converters;

namespace Arcanum.UI.Components.StyleClasses;

public class BaseButton : Button
{
   public static readonly DependencyProperty HoverBackgroundProperty =
      DependencyProperty.Register(nameof(HoverBackground), typeof(Brush), typeof(BaseButton), new(Brushes.Transparent));

   public Brush HoverBackground
   {
      get => (Brush)GetValue(HoverBackgroundProperty);
      set => SetValue(HoverBackgroundProperty, value);
   }

   public CornerRadius CornerRadius
   {
      get => (CornerRadius)GetValue(CornerRadiusProperty);
      set => SetValue(CornerRadiusProperty, value);
   }

   public static readonly DependencyProperty CornerRadiusProperty =
      DependencyProperty.Register(nameof(CornerRadius),
                                  typeof(CornerRadius),
                                  typeof(CorneredTextBox),
                                  new(new CornerRadius(3)));
}