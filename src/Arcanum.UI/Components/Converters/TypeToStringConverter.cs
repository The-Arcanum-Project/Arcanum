using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Arcanum.UI.Components.UserControls.BaseControls;

namespace Arcanum.UI.Components.Converters;

public class TypeToStringConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      var valueType = value?.GetType();
      if (valueType == null)
         return "null";

      if (PropertyGrid.CustomTypeConverters.TryGetValue(valueType, out var customConverter))
         return customConverter.Invoke(value!);

      var method = valueType.GetMethod(nameof(ToString), BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes);
      return method?.DeclaringType != typeof(object) ? value!.ToString()! : valueType.Name;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();
}