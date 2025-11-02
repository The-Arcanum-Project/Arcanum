using System.Globalization;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Components.Converters;

public class IsListNotEmptyConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is System.Collections.IEnumerable enumerable)
      {
         return enumerable.HasItems();
      }

      return false; // The list is empty or null
   }

   public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}