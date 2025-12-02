using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class NullToVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
      => value == null ? "Collapsed" : "Visible";

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}