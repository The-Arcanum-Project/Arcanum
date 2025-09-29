using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Arcanum.UI.Components.Converters;

public class ColorToContrastingBrushConverter : IValueConverter
{
   /// <summary>
   /// Converts a Color to a contrasting brush (either Black or White).
   /// </summary>
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not Color color)
         return Brushes.Black;

      // This is a standard formula for calculating perceived luminance (brightness).
      // It accounts for how the human eye perceives the brightness of R, G, and B differently.
      var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B);

      // We use a threshold of 128 (half of 255).
      // If the color is dark (luminance < 128), we return a White brush.
      // If the color is light (luminance >= 128), we return a Black brush.
      return luminance < 128 ? Brushes.White : Brushes.Black;
   }

   /// <summary>
   /// This converter does not support converting back.
   /// </summary>
   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}