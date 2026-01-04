using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.NUI.Generator.StructConverters;

internal class QuaternionComponentConverter(string component) : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is Quaternion q)
         return component switch
         {
            "X" => q.X,
            "Y" => q.Y,
            "Z" => q.Z,
            "W" => q.W,
            _ => 0.0f,
         };

      return 0.0f;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
}