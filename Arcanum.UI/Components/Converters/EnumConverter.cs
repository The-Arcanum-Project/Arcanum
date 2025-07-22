using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

class EnumValuesConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not Type { IsEnum: true } enumType)
         return null;

      return Enum.GetValues(enumType);
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}