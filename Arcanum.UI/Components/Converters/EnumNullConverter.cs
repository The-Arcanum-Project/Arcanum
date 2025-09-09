using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class EnumConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      // When the ViewModel provides a value (or null), just pass it to the ComboBox.
      // A null will clear the selection, a valid enum will select the item.
      return value;
   }

   public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      // When the ComboBox selection changes, the value will be a valid enum member or null.
      // If it's null (e.g., user clears the text), we want to avoid setting a null on the 
      // actual property if it's not nullable.
      // However, in our multi-select ViewModel, null is a valid state, so we just pass it through.
      return value;
   }
}