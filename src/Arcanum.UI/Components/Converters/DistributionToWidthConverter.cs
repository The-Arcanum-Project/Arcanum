using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class DistributionToWidthConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      // Expected inputs:
      // [0] = Item Value (int)
      // [1] = Total Limit (int)
      // [2] = Container ActualWidth (double)

      if (values.Length < 3 ||
          values[0] is not int itemVal ||
          values[1] is not int total ||
          values[2] is not double containerWidth)
         return 0.0;

      if (total <= 0 || containerWidth <= 0)
         return 0.0;

      // Math: Percentage * Available Pixels
      return (double)itemVal / total * containerWidth;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}