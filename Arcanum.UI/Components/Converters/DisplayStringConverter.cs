using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class DisplayStringConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is null)
         return "null";

      var type = value.GetType();
      var toStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes);

      if (toStringMethod != null && toStringMethod.DeclaringType != typeof(object))
         return value.ToString() ?? string.Empty;

      return $"<{type.Name}>";
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}