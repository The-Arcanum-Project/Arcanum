using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

namespace Arcanum.UI.Components.Converters;

public class StateToBackgroundConverter : IValueConverter
{
   public static Brush InSomeBrush { get; set; } = (Brush)Application.Current.FindResource("DarkGreenColorBrush")!;
   public static Brush MarkedForAdditionBrush { get; set; } =
      (Brush)Application.Current.FindResource("DarkYellowColorBrush")!;

   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return value is EditState state
                ? state switch
                {
                   EditState.InSome => MarkedForAdditionBrush,
                   EditState.InAll => InSomeBrush,
                   EditState.MarkedForAddition => InSomeBrush,
                   _ => Brushes.Transparent,
                }
                : Brushes.Transparent;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}