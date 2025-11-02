using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class IsListTypeVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return Visibility.Collapsed;

      var type = value.GetType();

      if (type.IsArray)
         return Visibility.Visible;

      if (type.IsGenericType)
      {
         var genType = type.GetGenericTypeDefinition();
         if (genType == typeof(List<>) ||
             genType == typeof(ICollection<>) ||
             genType == typeof(IEnumerable<>))
            return Visibility.Visible;
      }

      return Visibility.Collapsed;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}