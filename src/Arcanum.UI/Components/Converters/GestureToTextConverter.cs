using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace Arcanum.UI.Components.Converters;

public class GestureToTextConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is KeyGesture kg)
         return kg.GetDisplayStringForCulture(culture);

      var s = value?.ToString();
      if (s is not null && s.StartsWith("None+"))
         return s[5..];

      return s ?? string.Empty;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}