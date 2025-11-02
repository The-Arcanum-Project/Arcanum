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
         return b;

      return null; // For mixed values, display the indeterminate state.
   }

   /// <summary>
   /// Converts the CheckBox's state back to a value for the ViewModel.
   /// This is where we prevent the user from setting a null state.
   /// </summary>
   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      return (bool?)value switch
      {
         // If the user clicked and the new state is CHECKED (true)
         true => true,
         // If the user clicked and the new state is UNCHECKED (false)
         // This happens when clicking from an Indeterminate box in the standard cycle.
         false => true,
         // If the user clicked and the new state is INDETERMINATE (null),
         // this means they clicked a CHECKED box. Their intent was to UNCHECK it.
         // Therefore, we should set the value to FALSE.
         null => false
      };
   }
}