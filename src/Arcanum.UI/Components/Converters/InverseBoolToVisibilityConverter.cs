using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Arcanum.UI.Components.Converters;

public class InverseBoolToVisibilityConverter : MarkupExtension, IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is bool b)
         return b ? Visibility.Collapsed : Visibility.Visible;

      return Visibility.Visible;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotSupportedException();

   public override object ProvideValue(IServiceProvider serviceProvider) => this;
}