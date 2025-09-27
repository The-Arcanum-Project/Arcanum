using System.Globalization;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.Jomini.Date;

namespace Arcanum.UI.Components.Converters;

public class JominiDateToStringConverter : IValueConverter
{
   // This method converts from the source (JominiDate) to the target (string)
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not JominiDate date)
         return string.Empty;
      if (parameter is string format && !string.IsNullOrEmpty(format))
         return date.FormatJominiDate(format);

      return date.ToString();
   }

   // This method converts from the target (string) back to the source (JominiDate)
   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is string inputString && JominiDate.TryParse(inputString, out var date))
         return date;

      return Binding.DoNothing;
   }
}