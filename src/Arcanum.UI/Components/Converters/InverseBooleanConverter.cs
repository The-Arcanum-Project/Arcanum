using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class InverseBooleanConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is bool boolVal)
         return !boolVal;

      return false;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is bool boolVal)
         return !boolVal;

      return false;
   }
}