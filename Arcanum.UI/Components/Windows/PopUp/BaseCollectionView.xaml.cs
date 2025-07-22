using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.Utils.DelayedEvents;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class BaseCollectionView
{
   public BaseCollectionView(IEnumerable items)
   {
      InitializeComponent();
      ListItems.ItemsSource = items;
   }

   private void ListItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (ListItems.SelectedItem == null)
         return;

      var type = ListItems.SelectedItem.GetType();
      var genType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

      if (type.IsPrimitive ||
          type == typeof(string) ||
          type == typeof(decimal) ||
          type.IsArray ||
          genType == typeof(List<>) ||
          genType == typeof(ICollection<>) ||
          genType == typeof(IEnumerable<>))
      {
         PropertyGrid.IsEnabled = false;
      }
      else
      {
         PropertyGrid.IsEnabled = true;
         PropertyGrid.SelectedObject = ListItems.SelectedItem;
      }
   }

   private void ViewCollection_Button_Click(object sender, RoutedEventArgs e)
   {
      IEnumerable? collection = null;

      if (sender is BaseButton button)
      {
         // Try direct DataContext
         if (button.DataContext is PropertyItem item)
         {
            if (item.Value is IEnumerable valueEnum)
               collection = valueEnum;
         }
         else
         {
            // Walk visual tree to ListBoxItem
            DependencyObject current = button;
            while (current != null! && current is not ListBoxItem)
               current = VisualTreeHelper.GetParent(current)!;

            if (current is ListBoxItem { DataContext: IEnumerable coll })
               collection = coll;
         }
      }

      if (collection == null)
         return;

      new BaseCollectionView(collection).ShowDialog();
   }
}