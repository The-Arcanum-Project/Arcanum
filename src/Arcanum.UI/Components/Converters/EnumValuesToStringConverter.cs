using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class EnumValuesToStringConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return null;

      // if we get an array of enum values we return for the first element
      if (value is Array { Length: > 0 } enumArray)
      {
         List<string> enumValues = new(enumArray.Length);
         foreach (var val in enumArray)
            if (val is Enum enumVal)
               enumValues.Add(enumVal.ToString());
         return string.Join(", ", enumValues);
      }

      return "No Enum Values";
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}