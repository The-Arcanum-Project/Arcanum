using System.Windows;
using System.Windows.Media;

namespace Arcanum.UI.Components.StyleClasses;

public static class IconHelper
{
   public static readonly DependencyProperty CheckedIconProperty =
      DependencyProperty.RegisterAttached("CheckedIcon", typeof(Geometry), typeof(IconHelper), new(null));

   public static readonly DependencyProperty UncheckedIconProperty =
      DependencyProperty.RegisterAttached("UncheckedIcon", typeof(Geometry), typeof(IconHelper), new(null));

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.RegisterAttached("Value", typeof(bool), typeof(IconHelper), new(false));

   public static void SetCheckedIcon(DependencyObject obj, Geometry value) => obj.SetValue(CheckedIconProperty, value);
   public static Geometry GetCheckedIcon(DependencyObject obj) => (Geometry)obj.GetValue(CheckedIconProperty);

   public static void SetUncheckedIcon(DependencyObject obj, Geometry value) => obj.SetValue(UncheckedIconProperty, value);
   public static Geometry GetUncheckedIcon(DependencyObject obj) => (Geometry)obj.GetValue(UncheckedIconProperty);

   public static void SetValue(DependencyObject obj, bool value) => obj.SetValue(ValueProperty, value);
   public static bool GetValue(DependencyObject obj) => (bool)obj.GetValue(ValueProperty);
}