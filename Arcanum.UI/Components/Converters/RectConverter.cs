using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class RectConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      var width = (double)values[0];
      var height = (double)values[1];
      return new Rect(0, 0, width, height);
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      => throw new NotImplementedException();
}