using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.UI.Components.Converters;

public class JominiColorToBrushConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is JominiColor jominiColor)
         return new SolidColorBrush(jominiColor.ToMediaColor());

      return Brushes.Transparent;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}