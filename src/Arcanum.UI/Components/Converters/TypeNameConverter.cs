using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class TypeNameConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is string fullName)
         return fullName.Split('.').Last(); // simple name

      return value;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}