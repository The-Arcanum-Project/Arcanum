using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Common.UI.MBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class PrimitiveTypeListView
{
   public class SelectableItem<T> : INotifyPropertyChanged
   {
      public T? Value { get; set; }
      public event PropertyChangedEventHandler? PropertyChanged;
   }

   // The collections that the ListBoxes will bind to.
   public ObservableCollection<SelectableItem<object>> AvailableItems { get; set; }
   public ObservableCollection<SelectableItem<object>> SelectedItems { get; set; }

   private readonly IList _originalSelectedList;
   private readonly Type _itemType;

   private PrimitiveTypeListView(IEnumerable allPossibleItems, IList listToEdit)
   {
      InitializeComponent();
      DataContext = this;

      _originalSelectedList = listToEdit;

      _itemType = listToEdit.GetType().IsGenericType
                     ? listToEdit.GetType().GetGenericArguments()[0]
                     : typeof(object);

      var allItemsWrapped = allPossibleItems.Cast<object>()
                                            .Select(item => new SelectableItem<object> { Value = item })
                                            .ToList();

      var selectedItemsWrapped = new List<SelectableItem<object>>();
      foreach (var item in listToEdit.Cast<object>())
      {
         var foundItem = allItemsWrapped.FirstOrDefault(w => Equals(w.Value, item));
         if (foundItem != null)
            selectedItemsWrapped.Add(foundItem);
      }

      AvailableItems = new(allItemsWrapped.Except(selectedItemsWrapped));
      SelectedItems = new(selectedItemsWrapped);
   }

   #region Button Click Handlers

   private void AddNewButton_Click(object sender, RoutedEventArgs e)
   {
      CreateAndAddNewItem();
   }

   private void NewItemTextBox_KeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Enter)
      {
         CreateAndAddNewItem();
         e.Handled = true;
      }
   }

   private void AddButton_Click(object sender, RoutedEventArgs e)
   {
      var itemsToAdd = AvailableItemsListBox.SelectedItems.Cast<SelectableItem<object>>().ToList();
      if (!itemsToAdd.Any())
         return;

      foreach (var item in itemsToAdd)
      {
         AvailableItems.Remove(item);
         SelectedItems.Add(item);
      }
   }

   private void RemoveButton_Click(object sender, RoutedEventArgs e)
   {
      var itemsToRemove = SelectedItemsListBox.SelectedItems.Cast<SelectableItem<object>>().ToList();
      if (!itemsToRemove.Any())
         return;

      foreach (var item in itemsToRemove)
      {
         SelectedItems.Remove(item);
         AvailableItems.Add(item);
      }
   }

   private void OkButton_Click(object sender, RoutedEventArgs e)
   {
      _originalSelectedList.Clear();
      foreach (var item in SelectedItems)
         _originalSelectedList.Add(item.Value);

      DialogResult = true;
   }

   #endregion

   #region Core Logic

   private void CreateAndAddNewItem()
   {
      string inputText = NewItemTextBox.Text.Trim();
      if (string.IsNullOrEmpty(inputText))
      {
         return;
      }

      object newItemValue;
      try
      {
         var converter = TypeDescriptor.GetConverter(_itemType);
         newItemValue = converter.ConvertFromString(inputText)!;
      }
      catch (Exception ex)
      {
         MBox.Show($"Could not convert '{inputText}' to the required type '{_itemType.Name}'.\n\nError: {ex.Message}",
                   "Invalid Input",
                   MBoxButton.OK,
                   MessageBoxImage.Error);
         return;
      }

      if (AvailableItems.Any(i => Equals(i.Value, newItemValue)) ||
          SelectedItems.Any(i => Equals(i.Value, newItemValue)))
      {
         MBox.Show($"The item '{inputText}' already exists.",
                   "Duplicate Item",
                   MBoxButton.OK,
                   MessageBoxImage.Warning);
         return;
      }

      var newItem = new SelectableItem<object> { Value = newItemValue };
      AvailableItems.Add(newItem);

      NewItemTextBox.Clear();
      NewItemTextBox.Focus();
   }

   #endregion

   #region Static Factory Method (How you will use this window)

   /// <summary>
   /// Creates and shows a modal collection editor window.
   /// </summary>
   /// <param name="allPossibleItems">The complete list of items that can be chosen from.</param>
   /// <param name="listToEdit">The list that will be displayed and modified.</param>
   /// <param name="title">The title for the window.</param>
   /// <returns>True if the user clicked OK; otherwise false.</returns>
   public static bool? ShowDialog(IEnumerable allPossibleItems, IList listToEdit, string title = "Collection Editor")
   {
      var window = new PrimitiveTypeListView(allPossibleItems, listToEdit) { Title = title };
      return window.ShowDialog();
   }

   #endregion

   public event PropertyChangedEventHandler? PropertyChanged;
}