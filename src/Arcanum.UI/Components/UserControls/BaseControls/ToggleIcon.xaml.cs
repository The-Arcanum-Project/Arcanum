using System.Windows;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class ToggleIcon
{
   public static readonly DependencyProperty IsCheckedProperty =
      DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ToggleIcon), new(false));

   public static readonly DependencyProperty CheckedIconProperty =
      DependencyProperty.Register(nameof(CheckedIcon), typeof(Geometry), typeof(ToggleIcon), new(null));

   public static readonly DependencyProperty UncheckedIconProperty =
      DependencyProperty.Register(nameof(UncheckedIcon), typeof(Geometry), typeof(ToggleIcon), new(null));

   public ToggleIcon()
   {
      InitializeComponent();
   }

   public bool IsChecked
   {
      get => (bool)GetValue(IsCheckedProperty);
      set => SetValue(IsCheckedProperty, value);
   }

   public Geometry CheckedIcon
   {
      get => (Geometry)GetValue(CheckedIconProperty);
      set => SetValue(CheckedIconProperty, value);
   }

   public Geometry UncheckedIcon
   {
      get => (Geometry)GetValue(UncheckedIconProperty);
      set => SetValue(UncheckedIconProperty, value);
   }
}