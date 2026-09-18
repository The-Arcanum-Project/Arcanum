#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public partial class SingleLocationView
{
   public SingleLocationView()
   {
      InitializeComponent();
   }

   private void PopupContent_PreviewKeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key != Key.Escape)
         return;

      if (sender is FrameworkElement element)
      {
         var parent = element.Parent;
         while (parent != null && parent is not Popup)
            parent = LogicalTreeHelper.GetParent(parent);

         if (parent is Popup popup)
         {
            popup.IsOpen = false;
            e.Handled = true;
         }
      }
   }

   private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is ComboBox selector)
      {
         var be = selector.GetBindingExpression(Selector.SelectedItemProperty);
         be?.UpdateSource();
      }
   }
}