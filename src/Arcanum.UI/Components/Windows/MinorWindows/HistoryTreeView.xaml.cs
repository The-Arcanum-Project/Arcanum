using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class HistoryTreeView
{
   public ObservableCollection<HistoryNode> Nodes { get; set; } = [];

   public HistoryTreeView()
   {
      InitializeComponent();
      // TODO: @Melco Make this cleaner in the future
      AppData.HistoryManager.NodeSwitched += (_) => MarkTreeViewItems();
      DataContext = this;
      Loaded += (_, _) =>
      {
         Nodes.Clear();
         Nodes.Add(AppData.HistoryManager.Root);
         ExpandAll(NodesTreeView);
      };
      Nodes.CollectionChanged += (_, _) => ExpandAll(NodesTreeView);
      MarkTreeViewItems();
   }
   
   private void MarkTreeViewItems()
   {
      foreach (var item in NodesTreeView.Items)
      {
         if (NodesTreeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
         {
            MarkTreeViewItemRecursive(tvi);
         }
      }
   }

   private void MarkTreeViewItemRecursive(TreeViewItem item)
   {
      if (item.DataContext is HistoryNode node && node.Equals(AppData.HistoryManager.Current))
      {
         item.Background = (Brush)Application.Current.FindResource("LightAccentBackColorBrush")!;
      }
      else
      {
         item.ClearValue(BackgroundProperty);
      }

      item.UpdateLayout(); // ensure children containers are generated

      foreach (var child in item.Items)
      {
         if (item.ItemContainerGenerator.ContainerFromItem(child) is TreeViewItem childTvi)
            MarkTreeViewItemRecursive(childTvi);
         else
         {
            // hook to generate container if not yet ready
            item.ItemContainerGenerator.StatusChanged += (_, _) =>
            {
               if (item.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

               if (item.ItemContainerGenerator.ContainerFromItem(child) is TreeViewItem generated)
                  MarkTreeViewItemRecursive(generated);
            };
         }
      }
   }
   
   public static void ExpandAll(TreeView treeView)
   {
      foreach (var item in treeView.Items)
         if (treeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
            ExpandAllRecursive(tvi);
   }

   private static void ExpandAllRecursive(TreeViewItem item)
   {
      item.IsExpanded = true;
      item.UpdateLayout();

      foreach (var subItem in item.Items)
         if (item.ItemContainerGenerator.ContainerFromItem(subItem) is not TreeViewItem child)
            item.ItemContainerGenerator.StatusChanged += (_, _) =>
            {
               if (item.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                  return;

               if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem generated)
                  ExpandAllRecursive(generated);
            };
         else
            ExpandAllRecursive(child);
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (NodesTreeView.SelectedItem is HistoryNode selectedNode)
         AppData.HistoryManager.RevertTo(selectedNode.Id);
   }
}