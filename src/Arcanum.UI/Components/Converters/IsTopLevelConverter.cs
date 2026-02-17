using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Arcanum.UI.Components.Converters;

public class IsTopLevelConverter : IValueConverter
{
   public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not DependencyObject obj)
         return false;

      var parent = VisualTreeHelper.GetParent(obj);
      while (parent != null && parent is not TreeView && parent is not TreeViewItem)
         parent = VisualTreeHelper.GetParent(parent);

      return parent is TreeView;
   }

   public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
}