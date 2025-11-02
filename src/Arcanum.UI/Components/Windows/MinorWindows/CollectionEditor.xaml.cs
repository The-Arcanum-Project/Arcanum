using System.Collections.ObjectModel;
using System.Windows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class CollectionEditor
{
   public ObservableCollection<string> SelectedItems { get; set; }
   public ObservableCollection<string> AvailableItems { get; set; }

   public int MinSelectedItems { get; set; } = 0;
   public int MaxSelectedItems { get; set; } = int.MaxValue;

   public CollectionEditor(ICollection<string> availableItems, ICollection<string> selectedItems)
   {
      InitializeComponent();

      AvailableItems = new(availableItems);
      SelectedItems = new(selectedItems);

      for (var i = AvailableItems.Count - 1; i >= 0; i--)
         if (SelectedItems.Contains(AvailableItems[i]))
            AvailableItems.RemoveAt(i);
   }

   private void MoveLeftButton_OnClick(object sender, RoutedEventArgs e)
   {
      var selectedAvailableItems = AvailableItemsView.SelectedItems;
      if (selectedAvailableItems.Count == 0)
         return;

      if (MaxSelectedItems > SelectedItems.Count + selectedAvailableItems.Count)
         for (var i = selectedAvailableItems.Count - 1; i >= 0; i--)
         {
            if (selectedAvailableItems[i] is null)
               continue;

            SelectedItems.Add(selectedAvailableItems[i]!.ToString()!);
            AvailableItems.Remove(selectedAvailableItems[i]!.ToString()!);
         }
      else
         MBox.Show($"You cannot select more than {MaxSelectedItems} items.",
                   "Selection Limit Reached",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
   }

   private void MoveRightButton_OnClick(object sender, RoutedEventArgs e)
   {
      var selectedSelectedItems = SelectedItemsView.SelectedItems;
      if (selectedSelectedItems.Count == 0)
         return;

      if (MinSelectedItems < SelectedItems.Count - selectedSelectedItems.Count)
         for (var i = selectedSelectedItems.Count - 1; i >= 0; i--)
         {
            if (selectedSelectedItems[i] is null)
               continue;

            AvailableItems.Add(selectedSelectedItems[i]!.ToString()!);
            SelectedItems.Remove(selectedSelectedItems[i]!.ToString()!);
         }
      else
         MBox.Show($"You must select at least {MinSelectedItems} items.",
                   "Selection Limit Reached",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
   }
}