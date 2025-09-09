using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class DoubleToDecimalConverter : IValueConverter
{
   /// <summary>
   /// Converts from the source (double?) to the target (decimal?).
   /// </summary>
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      switch (value)
      {
         case null:
            return null;
         case double doubleValue:
            try
            {
               return (decimal)doubleValue;
            }
            catch (OverflowException)
            {
               return DependencyProperty.UnsetValue;
            }
         default:
            return DependencyProperty.UnsetValue;
      }
   }

   /// <summary>
   /// Converts from the target (decimal?) back to the source (double?).
   /// </summary>
   public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return value switch
      {
         null => null,
         decimal decimalValue => (double)decimalValue,
         _ => DependencyProperty.UnsetValue,
      };
   }
}