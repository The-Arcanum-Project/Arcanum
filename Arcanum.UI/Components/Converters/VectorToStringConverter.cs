using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class Vector2ToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Vector2 v)
            return $"Pos: [X: {(int)v.X:+00000;-00000}, Y: {(int)v.Y:+00000;-00000}]";
        return "Pos: [X: 0, Y: 0]";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}