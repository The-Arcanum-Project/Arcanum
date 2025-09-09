using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class MultiSelectBooleanConverter : IValueConverter
{
   /// <summary>
   /// Converts the ViewModel's value to a nullable boolean for the CheckBox.
   /// </summary>
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      // The value from MultiSelectPropertyViewModel.Value is already an object.
      // It can be true, false, or null. The CheckBox.IsChecked property (a bool?)
      // can handle this directly. We just need to ensure the type is correct.
      if (value is bool b)
      {
         return b;
      }
      return null; // For mixed values, display the indeterminate state.
   }

   /// <summary>
   /// Converts the CheckBox's state back to a value for the ViewModel.
   /// This is where we prevent the user from setting a null state.
   /// </summary>
   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      // value is the bool? from CheckBox.IsChecked
      var isChecked = (bool?)value;

      // THE CORE LOGIC:
      // If the user cycles to the indeterminate state (null),
      // we interpret that as wanting to set the value to 'true'.
      // This is a common and intuitive behavior: clicking an empty box checks it,
      // clicking a mixed-value box also checks it (sets all to true).
      return isChecked ?? true;
   }
}