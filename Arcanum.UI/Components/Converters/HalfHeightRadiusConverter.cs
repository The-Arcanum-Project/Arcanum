using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class HalfRadiusConverter : IValueConverter
{
    public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
    {
        return values is not (double height and > 0) ? DependencyProperty.UnsetValue : new CornerRadius(height / 2);
    }

    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}