#region

using System.Globalization;
using System.Windows.Data;

#endregion

namespace Arcanum.UI.Components.Converters;

public class EnumerableToStringConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is IEnumerable<string> list)
         return string.Join(", ", list);

      return string.Empty;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}