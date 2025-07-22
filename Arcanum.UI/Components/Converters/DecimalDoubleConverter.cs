using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class DecimalDoubleConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
   {
      double d => System.Convert.ToDecimal(d),
      decimal m => m,
      _ => Binding.DoNothing
   };

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => targetType == typeof(double)
            ? System.Convert.ToDouble(value)
            : value!;
}