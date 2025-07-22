using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class WidthMinusConverter : IValueConverter
{
   public double Subtract { get; set; } = 0;

   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is null || targetType != typeof(double) || parameter != null)
         throw new ArgumentNullException(nameof(value));

      return Math.Max(0, (double)value - Subtract);
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}