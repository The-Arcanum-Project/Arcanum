using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.Globals;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class HistoryTreeView
{
   public ObservableCollection<HistoryNode> Nodes { get; set; } = [];

   public HistoryTreeView()
   {
      InitializeComponent();
      DataContext = this;
      Loaded += (_, _) =>
      {
         Nodes.Clear();
         Nodes.Add(Globals.HistoryManager.Root);
         var node1 = new HistoryNode(1, new CInitial(), HistoryEntryType.Normal, Globals.HistoryManager.Root);
         Globals.HistoryManager.Root.Children.Add(node1);
         var node2 = new HistoryNode(2, new CInitial(), HistoryEntryType.Normal, node1);
         node1.Children.Add(node2);
         var node3 = new HistoryNode(3, new CInitial(), HistoryEntryType.Normal, node2);
         node2.Children.Add(node3);
         var node4 = new HistoryNode(4, new CInitial(), HistoryEntryType.Normal, node3);
         node3.Children.Add(node4);
         var node5 = new HistoryNode(5, new CInitial(), HistoryEntryType.Normal, node4);
         node4.Children.Add(node5);
         var node6 = new HistoryNode(6, new CInitial(), HistoryEntryType.Normal, node5);
         node5.Children.Add(node6);

         var compNode = new CompactHistoryNode(20, [node1, node2, node3, node4]);
         compNode.InsertInTree();

         ExpandAll(NodesTreeView);
      };
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
               if (item.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
               {
                  var generated = item.ItemContainerGenerator.ContainerFromItem(subItem) as TreeViewItem;
                  if (generated != null)
                     ExpandAllRecursive(generated);
               }
            };
         else
            ExpandAllRecursive(child);
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (NodesTreeView.SelectedItem is HistoryNode selectedNode)
         Globals.HistoryManager.RevertTo(selectedNode.Id);
   }
}