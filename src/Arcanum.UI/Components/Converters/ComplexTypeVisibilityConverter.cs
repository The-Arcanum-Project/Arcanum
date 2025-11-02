using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class ComplexTypeVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return Visibility.Visible;

      var type = value.GetType();
      if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
         return Visibility.Collapsed;

      return Visibility.Visible;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}