using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

class EnumValuesConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not Type { IsEnum: true } enumType)
         return null;

      var isFlags = Attribute.IsDefined(enumType, typeof(FlagsAttribute));

      var values = Enum.GetValues(enumType)
                       .Cast<Enum>()
                       .Where(v =>
                        {
                           var val = System.Convert.ToInt64(v);
                           return Enum.IsDefined(enumType, v) &&
                                  (!isFlags || (val != 0 && IsSingleBit(val)));
                        })
                       .Distinct()
                       .ToArray();

      return values;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();

   private static bool IsSingleBit(long value) => (value & (value - 1)) == 0;
}