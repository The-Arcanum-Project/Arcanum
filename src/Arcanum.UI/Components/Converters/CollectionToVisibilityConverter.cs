using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class CollectionToVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is IEnumerable enumerable)
      {
         var enumerator = enumerable.GetEnumerator();
         using var enumerator1 = enumerator as IDisposable;
         return enumerator.MoveNext() ? Visibility.Visible : Visibility.Collapsed;
      }

      return Visibility.Collapsed;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}