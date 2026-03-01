using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Arcanum.UI.Components.Converters;

public class ResourceToGeometryConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not string resourceKey)
         return null;

      var resource = Application.Current.TryFindResource(resourceKey);
      return resource as Geometry;
   }

   public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
}