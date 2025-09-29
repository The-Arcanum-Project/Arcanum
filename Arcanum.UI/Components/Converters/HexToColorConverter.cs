using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Arcanum.UI.Components.Converters;

public class HexToColorConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return value?.ToString()?.TrimStart('#') ?? string.Empty;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not string hex)
         return Binding.DoNothing;

      // Allow formats like #RRGGBB, RRGGBB, #RGB, RGB
      hex = hex.TrimStart('#');

      try
      {
         var color = (Color)ColorConverter.ConvertFromString("#" + hex);
         return color;
      }
      catch (Exception)
      {
         return Binding.DoNothing;
      }
   }
}