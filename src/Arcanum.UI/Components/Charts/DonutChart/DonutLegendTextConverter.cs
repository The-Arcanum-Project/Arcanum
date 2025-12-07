using System.Globalization;
using System.Windows.Data;

namespace Arcanum.UI.Components.Charts.DonutChart;

public class DonutLegendTextConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      // Bindings: [0]=Value, [1]=Total, [2]=ShowValues(bool), [3]=ShowPercentage(bool)
      if (values.Length < 4 || values[0] is not double val || values[1] is not double total)
         return string.Empty;

      var showVal = values[2] is true;
      var showPct = values[3] is true;

      if (!showVal && !showPct)
         return string.Empty;

      var valText = val.ToString("N0"); // You could expose FormatString property too
      var pctText = total > 0 ? (val / total).ToString("P1") : "0%";

      if (showVal && showPct)
         return $"{valText} ({pctText})";
      if (showPct)
         return pctText;

      return valText;
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}