using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class PropertyNameAndCountConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      return values.Length switch
      {
         // Expects two values: [0] = Enum propertyName, [1] = int collectionCount
         2 when values[0] is Enum propertyName && values[1] is int count => $"{propertyName.ToString()} ({count})",
         // Fallback for simple properties that don't have a count
         > 0 when values[0] is Enum propNameOnly => propNameOnly.ToString(),
         _ => string.Empty,
      };
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}