using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class EnumValuesFromValueConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return null;

      var enumType = value.GetType();

      // if we get an array of enum values we return for the first element
      if (value is Array { Length: > 0 } enumArray)
      {
         if (enumArray.GetValue(0) is not null)
            enumType = enumArray.GetValue(0)!.GetType();
         else
            return null;
      }

      if (!enumType.IsEnum)
         return null;

      var enumValues = Enum.GetValues(enumType);

      return enumValues;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}

public class EnumValueFromValueConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return null;

      var enumType = value.GetType();
      if (!enumType.IsEnum)
         return null;

      return Enum.Parse(enumType, value.ToString() ?? string.Empty);
   }

   public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}