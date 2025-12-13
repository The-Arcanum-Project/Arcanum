using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Arcanum.UI.Components.Converters;

public class BorderClipConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values.Length < 3 ||
          values[0] is not double width ||
          values[1] is not double height ||
          values[2] is not CornerRadius radius)
      {
         return Geometry.Empty;
      }

      if (width < 1 || height < 1)
         return Geometry.Empty;

      // Create a rounded rectangle geometry matching the container
      // We use radius.TopLeft as the uniform radius for simplicity
      return new RectangleGeometry(new(0, 0, width, height),
                                   radius.TopLeft,
                                   radius.TopLeft);
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}