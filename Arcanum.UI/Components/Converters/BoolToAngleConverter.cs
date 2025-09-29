using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class BoolToAngleConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return value is true ? 180.0 : 0.0;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return (value is double d && Math.Abs(d - 180.0) < 0.01);
   }
}