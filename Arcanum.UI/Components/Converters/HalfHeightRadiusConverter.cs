using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class HeightToRadiusConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length <= 0 || values[0] is not double height || !(height > 0)) return DependencyProperty.UnsetValue;
        // For CornerRadius
        if (targetType == typeof(CornerRadius))
        {
            return new CornerRadius(height / 2);
        }
                
        // For Thumb Size (Width/Height)
        if (targetType == typeof(double))
        {
            return height - 4; // 2px padding on each side
        }

        // For Thumb Margin
        return targetType == typeof(Thickness) ? new Thickness(2,0,0,0) : DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}