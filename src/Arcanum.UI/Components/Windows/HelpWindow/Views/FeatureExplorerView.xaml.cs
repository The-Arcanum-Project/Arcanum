#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.Views;

public partial class FeatureExplorerView
{
   public FeatureExplorerView()
   {
      InitializeComponent();

      Loaded += FeatureExplorerView_Loaded;
      Unloaded += FeatureExplorerView_Unloaded;
   }

   private void FeatureExplorerView_Loaded(object sender, RoutedEventArgs e)
   {
      if (DataContext is FeatureExplorerViewModel { Features.Count: > 0 } vm)
         vm.SelectedItem = vm.Features[0];
   }

   private void FeatureExplorerView_Unloaded(object sender, RoutedEventArgs e)
   {
      Loaded -= FeatureExplorerView_Loaded;
      Unloaded -= FeatureExplorerView_Unloaded;
   }

   private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount != 2)
         return;

      if (sender is FrameworkElement { DataContext: IAppCommand cmd } && cmd.CanExecute(null))
      {
         cmd.Execute(null);
         Window.GetWindow(this)?.Close();
      }

      e.Handled = true;
   }

   private void ListView_OnSelectedItemChanged(object sender, RoutedEventArgs e)
   {
      if (sender is not ListView listView)
         return;

      if (listView.SelectedItem is FeatureItem selectedItem)
         (DataContext as FeatureExplorerViewModel)?.SelectedItem = selectedItem;
   }
}