using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.CommandSystem;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.Components.Converters;

public class ObjectToGraphButtonVisibilityConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is IEu5Object eu5Object)
         // The button should be visible only if the object has graphable properties.
         return Nx.GetGraphableProperties(eu5Object).Length > 0
                   ? Visibility.Visible
                   : Visibility.Collapsed;

      return Visibility.Collapsed;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}