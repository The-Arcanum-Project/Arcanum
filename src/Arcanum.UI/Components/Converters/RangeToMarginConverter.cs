#region

using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Arcanum.UI.Components.Converters;

public sealed class RangeToMarginConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values.Length < 5 || values.Any(v => v == null || v == DependencyProperty.UnsetValue))
         return new Thickness(0);

      var minVal = System.Convert.ToDouble(values[0]);
      var maxVal = System.Convert.ToDouble(values[1]);
      var minLimit = System.Convert.ToDouble(values[2]);
      var maxLimit = System.Convert.ToDouble(values[3]);
      var actualWidth = System.Convert.ToDouble(values[4]);

      if (maxLimit <= minLimit || actualWidth <= 0)
         return new Thickness(0);

      // This indent matches the 10px margin on your background rail
      var indent = 10.0;
      var usableWidth = actualWidth - indent * 2;

      var range = maxLimit - minLimit;

      // Calculate how far from the left the Min thumb is
      var leftPos = (minVal - minLimit) / range * usableWidth;

      // Calculate how far from the right the Max thumb is
      var rightPos = (maxLimit - maxVal) / range * usableWidth;

      // Return the margin. We add the 'indent' to shift the whole system 
      // away from the control edges to match the rail.
      return new Thickness(leftPos + indent, 0, rightPos + indent, 0);
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}