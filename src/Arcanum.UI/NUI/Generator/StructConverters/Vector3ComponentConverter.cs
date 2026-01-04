using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.NUI.Generator.StructConverters;

internal class Vector3ComponentConverter(string component) : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is Vector3 v)
      {
         return component switch
         {
            "X" => v.X,
            "Y" => v.Y,
            "Z" => v.Z,
            _ => 0.0f,
         };
      }

      return 0.0f;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
}