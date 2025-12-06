using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class LimitToWidthConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      // [0] = Limit Value (on Slider Scale) e.g. 20
      // [1] = Total (on Slider Scale) e.g. 100
      // [2] = Container Width (Pixels)

      if (values.Length < 3 ||
          values[0] is not double limitVal ||
          values[1] is not int total ||
          values[2] is not double width)
         return 0.0;

      if (total <= 0 || width <= 0)
         return 0.0;

      // Simple linear mapping because the ViewModel already handled the Log transformation
      // for the 'limitVal' input.
      var result = (limitVal / total) * width;
      return result > 0 ? result : 0.0;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [];
}