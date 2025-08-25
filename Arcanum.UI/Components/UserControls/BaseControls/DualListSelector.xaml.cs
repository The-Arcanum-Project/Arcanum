using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Converters;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class DualListSelector
{
   public ObservableCollection<object> SelectedItems { get; } = [];
   public ObservableCollection<object> AvailableItems { get; } = [];

   // Search
   private readonly ICollectionView _selectedView;
   private readonly ICollectionView _availableView;
   private readonly DisplayStringConverter _converter = new(); // For search logic

   public DualListSelector()
   {
      InitializeComponent();

      // Create the CollectionViews for filtering
      _selectedView = CollectionViewSource.GetDefaultView(SelectedItems);
      _availableView = CollectionViewSource.GetDefaultView(AvailableItems);

      // Bind the ListViews to the filtered views, not the raw collections
      SelectedListView.ItemsSource = _selectedView;
      AvailableListView.ItemsSource = _availableView;

      // Wire up events directly in the constructor. This is safer than using Loaded.
      MoveToSelectedButton.Click += MoveToSelected_Click;
      MoveToAvailableButton.Click += MoveToAvailable_Click;
      MoveUpButton.Click += MoveUp_Click;
      MoveDownButton.Click += MoveDown_Click;
   }

   #region Dependency Properties (The Public API)

   public IEnumerable AllItemsSource
   {
      get => (IEnumerable)GetValue(AllItemsSourceProperty);
      init => SetValue(AllItemsSourceProperty, value);
   }

   public static readonly DependencyProperty AllItemsSourceProperty =
      DependencyProperty.Register(nameof(AllItemsSource),
                                  typeof(IEnumerable),
                                  typeof(DualListSelector),
                                  new(null, OnSourcesChanged));

   // DP for the list of SELECTED items.
   public IList SelectedItemsSource
   {
      get => (IList)GetValue(SelectedItemsSourceProperty);
      init => SetValue(SelectedItemsSourceProperty, value);
   }

   public static readonly DependencyProperty SelectedItemsSourceProperty =
      DependencyProperty.Register(nameof(SelectedItemsSource),
                                  typeof(IList),
                                  typeof(DualListSelector),
                                  new(null, OnSourcesChanged));

   #endregion

   public static BaseWindow CreateWindow(IEnumerable allItems, IList selectedItems, string title)
   {
      return new()
      {
         Title = title,
         Width = 600,
         Height = 400,
         WindowStartupLocation = WindowStartupLocation.CenterOwner,
         ResizeMode = ResizeMode.CanResize,
         Content = new DualListSelector
         {
            AllItemsSource = allItems, SelectedItemsSource = selectedItems,
         },
         Background = (Brush)Application.Current.FindResource("DefaultBackColorBrush")!,
         HeaderBackGroundBrush = (Brush)Application.Current.FindResource("AccentGradientBrush")!,
      };
   }

   private static void OnSourcesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (DualListSelector)d;
      control.PopulateLists();
   }

   private void PopulateLists()
   {
      if (AllItemsSource == null! || SelectedItemsSource == null!)
         return;

      // Use a HashSet for efficient lookup of selected items.
      var selectedSet = new HashSet<object>(SelectedItemsSource.Cast<object>());

      SelectedItems.Clear();
      AvailableItems.Clear();

      foreach (var item in SelectedItemsSource)
         SelectedItems.Add(item);

      foreach (var item in AllItemsSource)
         if (!selectedSet.Contains(item))
            AvailableItems.Add(item);
   }

   #region Button Click Logic

   private bool CanItemsBeMoved(int count, string from, string to)
   {
      if (count > Config.Settings.NUIConfig.MaxItemsMovedWithoutWarning)
         return MBox.Show($"Do you really want to move {count} items from '{from}' to '{to}'?",
                          "Confirm Move",
                          MBoxButton.OKCancel,
                          MessageBoxImage.Question) ==
                MBoxResult.OK;
      return true;
   }

   private void MoveToSelected_Click(object sender, RoutedEventArgs e)
   {
      var itemsToMove = AvailableListView.SelectedItems.Cast<object>().ToList();
      if (!CanItemsBeMoved(itemsToMove.Count, "Available", "Selected"))
         return;
      foreach (var item in itemsToMove)
      {
         AvailableItems.Remove(item);
         SelectedItems.Add(item);
         SelectedItemsSource.Add(item);
      }
   }

   private void MoveToAvailable_Click(object sender, RoutedEventArgs e)
   {
      var itemsToMove = SelectedListView.SelectedItems.Cast<object>().ToList();
      if (!CanItemsBeMoved(itemsToMove.Count, "Selected", "Available"))
         return;
      foreach (var item in itemsToMove)
      {
         SelectedItems.Remove(item);
         AvailableItems.Add(item);
         SelectedItemsSource.Remove(item);
      }
   }

   private void MoveUp_Click(object sender, RoutedEventArgs e)
   {
      var selectedIndex = SelectedListView.SelectedIndex;
      if (selectedIndex > 0)
      {
         var item = SelectedItems[selectedIndex];
         SelectedItems.RemoveAt(selectedIndex);
         SelectedItems.Insert(selectedIndex - 1, item);
         SelectedListView.SelectedIndex = selectedIndex - 1;

         // Also update the external bound list if it's an IList<T>
         if (SelectedItemsSource is { } list)
         {
            list.RemoveAt(selectedIndex);
            list.Insert(selectedIndex - 1, item);
         }
      }
   }

   private void MoveDown_Click(object sender, RoutedEventArgs e)
   {
      var selectedIndex = SelectedListView.SelectedIndex;
      if (selectedIndex > -1 && selectedIndex < SelectedItems.Count - 1)
      {
         var item = SelectedItems[selectedIndex];
         SelectedItems.RemoveAt(selectedIndex);
         SelectedItems.Insert(selectedIndex + 1, item);
         SelectedListView.SelectedIndex = selectedIndex + 1;

         // Also update the external bound list
         if (SelectedItemsSource is { } list)
         {
            list.RemoveAt(selectedIndex);
            list.Insert(selectedIndex + 1, item);
         }
      }
   }

   #endregion

   #region Search Logic

   private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      var searchBox = (TextBox)sender;
      var searchText = searchBox.Text;
      var targetView = searchBox.Tag.ToString() == "Selected" ? _selectedView : _availableView;

      if (string.IsNullOrWhiteSpace(searchText))
         targetView.Filter = null;
      else
         targetView.Filter = item =>
         {
            var displayText = _converter.Convert(item, typeof(string), null, CultureInfo.InvariantCulture) as string;
            return displayText?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
         };
   }

   #endregion
}