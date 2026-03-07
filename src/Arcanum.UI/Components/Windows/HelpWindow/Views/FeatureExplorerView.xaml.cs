using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

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
      if (DataContext is FeatureExplorerViewModel vm)
      {
         vm.RequestSelectionUpdate += OnSelectionUpdateRequested;

         if (vm.SelectedItem != null)
            OnSelectionUpdateRequested(vm.SelectedItem);
         else if (vm.FeatureTree.Count > 0)
            vm.SelectedItem = vm.FeatureTree[0];
      }
   }

   private void OnSelectionUpdateRequested(FeatureTreeItem? item)
   {
      if (item == null)
         return;

      Dispatcher.BeginInvoke(DispatcherPriority.Input,
                             () =>
                             {
                                var container = FindTreeViewItem(FeatureTreeView, item);
                                container?.BringIntoView();
                                container?.Focus();
                             });
   }

   private void FeatureExplorerView_Unloaded(object sender, RoutedEventArgs e)
   {
      // Cleanup to prevent memory leaks
      if (DataContext is FeatureExplorerViewModel vm)
         vm.RequestSelectionUpdate -= OnSelectionUpdateRequested;
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

   private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
   {
      if (e.NewValue is FeatureTreeItem item && DataContext is FeatureExplorerViewModel vm)
         vm.SelectedItem = item;
   }

   private static TreeViewItem? FindTreeViewItem(ItemsControl parent, object dataItem)
   {
      if (parent.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
         return null!;

      if (parent.ItemContainerGenerator.ContainerFromItem(dataItem) is TreeViewItem container)
         return container;

      foreach (var childItem in parent.Items)
         if (parent.ItemContainerGenerator.ContainerFromItem(childItem) is TreeViewItem childContainer)
         {
            var found = FindTreeViewItem(childContainer, dataItem);
            if (found != null!)
               return found;
         }

      return null;
   }
}