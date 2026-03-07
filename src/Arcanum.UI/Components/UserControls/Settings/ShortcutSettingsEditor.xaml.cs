using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.Commands;
using Arcanum.UI.Commands.KeyMap;
using Arcanum.UI.Components.Converters;
using Arcanum.UI.Components.Views.Settings;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.UserControls.Settings;

public partial class ShortcutSettingsEditor
{
   public static readonly DependencyProperty RootItemsProperty =
      DependencyProperty.Register(nameof(RootItems), typeof(ObservableCollection<ShortcutTreeItem>), typeof(ShortcutSettingsEditor));

   private readonly GestureToTextConverter _gestureToTextConverter = new();
   private CancellationTokenSource? _searchCts;

   public ShortcutSettingsEditor()
   {
      InitializeComponent();
      // Set the DataContext to itself so the TreeView can find RootItems
      DataContext = this;

      // Initialize Command Bindings
      CommandBindings.Add(new(AddShortcutCommand, ExecuteAddShortcut));
      CommandBindings.Add(new(ResetCommand, ExecuteReset));
      CommandBindings.Add(new(RemoveShortcutCommand, ExecuteRemoveShortcut, (_, e) => e.CanExecute = e.Parameter is InputGesture));
      CommandBindings.Add(new(ReplaceShortcutCommand, ExecuteReplaceShortcut, (_, e) => e.CanExecute = e.Parameter is InputGesture));

      Loaded += async (_, _) => await InitializeTreeAsync();
   }

   public ObservableCollection<ShortcutTreeItem> RootItems
   {
      get => (ObservableCollection<ShortcutTreeItem>)GetValue(RootItemsProperty);
      set => SetValue(RootItemsProperty, value);
   }

   // Helper to track right-clicked item
   private ShortcutTreeItem? SelectedItem => TreeViewDisplay.SelectedItem as ShortcutTreeItem;

   public static RoutedCommand AddShortcutCommand { get; } = new("AddShortcut", typeof(ShortcutSettingsEditor));
   public static RoutedCommand ResetCommand { get; } = new("Reset", typeof(ShortcutSettingsEditor));
   public static RoutedCommand RemoveShortcutCommand { get; } = new("RemoveShortcut", typeof(ShortcutSettingsEditor));
   public static RoutedCommand ReplaceShortcutCommand { get; } = new("ReplaceShortcut", typeof(ShortcutSettingsEditor));

   private async Task InitializeTreeAsync()
   {
      var rootItems = await Task.Run(() =>
      {
         var root = new ShortcutTreeItem { Name = "Root" };
         var textInfo = CultureInfo.CurrentCulture.TextInfo;

         foreach (var cmd in CommandRegistry.AllCommands)
         {
            var parts = cmd.Id.Value.Split('.');
            var folderHierarchy = parts.Take(parts.Length - 1);
            var currentFolder = root;

            foreach (var part in folderHierarchy)
            {
               var folderName = textInfo.ToTitleCase(part);
               var folder = currentFolder.Children.FirstOrDefault(c => c.Name == folderName);

               if (folder == null)
               {
                  folder = new() { Name = folderName };
                  currentFolder.Children.Add(folder);
               }

               currentFolder = folder;
            }

            currentFolder.Children.Add(new()
            {
               Name = cmd.DisplayName, Command = cmd,
            });
         }

         return new ObservableCollection<ShortcutTreeItem>(root.Children);
      });

      RootItems = rootItems;
      LoadingSpinner.Visibility = Visibility.Collapsed;
      TreeViewDisplay.Visibility = Visibility.Visible;
   }

   private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      const int debounceDelayMs = 250;
      _searchCts?.Cancel();
      _searchCts = new();
      var token = _searchCts.Token;

      var query = SearchBox.Text;

      Task.Delay(debounceDelayMs, token)
          .ContinueWith(t =>
                        {
                           if (t.IsCanceled)
                              return;

                           Dispatcher.Invoke(() =>
                           {
                              if (RootItems == null!)
                                 return;

                              if (string.IsNullOrWhiteSpace(query))
                              {
                                 foreach (var item in RootItems)
                                    ResetTreeStateRecursive(item);
                                 return;
                              }

                              foreach (var item in RootItems)
                                 FilterItem(item, query);
                           });
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
   }

   private static void ResetTreeStateRecursive(ShortcutTreeItem item)
   {
      item.IsVisible = true;
      item.IsExpanded = false;

      foreach (var child in item.Children)
         ResetTreeStateRecursive(child);
   }

   private void SearchMode_Click(object sender, RoutedEventArgs e)
   {
      var query = SearchBox.Text;
      foreach (var item in RootItems)
         FilterItem(item, query);
   }

   private bool FilterItem(ShortcutTreeItem item, string query)
   {
      if (string.IsNullOrWhiteSpace(query))
      {
         item.IsVisible = true;
         item.IsExpanded = false;
         foreach (var child in item.Children)
            FilterItem(child, query);
         return true;
      }

      // Check Name
      var matches = item.Name.Contains(query, StringComparison.OrdinalIgnoreCase) && ToggleSearchKeys.IsChecked == false && ToggleSearchIds.IsChecked == false;

      // Check Command Specifics
      if (item is { IsCommand: true, Command: not null })
      {
         // Check Command ID 
         if (ToggleSearchIds.IsChecked == true)
            if (item.Command.Id.Value.Contains(query, StringComparison.OrdinalIgnoreCase))
               matches = true;

         // Check Gestures 
         if (ToggleSearchKeys.IsChecked == true)
            if (item.Command.Gestures.Any(g => (_gestureToTextConverter.Convert(g, null!, null, CultureInfo.InvariantCulture) as string)!
                                            .Contains(query, StringComparison.OrdinalIgnoreCase)))
               matches = true;

         // Check Description 
         if (ToggleSearchIds.IsChecked == false && ToggleSearchKeys.IsChecked == false)
            if (item.Command.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
               matches = true;
      }

      var anyChildMatches = false;
      foreach (var child in item.Children)
         if (FilterItem(child, query))
            anyChildMatches = true;

      var finalVisibility = matches || anyChildMatches;
      item.IsVisible = finalVisibility;

      // Expand folders automatically if they contain a search match
      if (!item.IsCommand)
         item.IsExpanded = finalVisibility && anyChildMatches;

      return finalVisibility;
   }

   // --- Command Implementations ---

   private void ExecuteAddShortcut(object sender, ExecutedRoutedEventArgs e)
   {
      if (e.Parameter is ShortcutTreeItem item && item.Command != null)
      {
         var recorder = new ShortcutRecorderWindow(SelectedItem?.Command!) { Owner = Window.GetWindow(this) };
         if (recorder.ShowDialog() == true && recorder.Result != null)
            try
            {
               var chord = recorder.Result;
               var k1 = Enum.Parse<Key>(chord.FirstStroke.Key);
               var m1 = Enum.Parse<ModifierKeys>(chord.FirstStroke.Modifiers);

               if (chord.IsChord && chord.SecondStroke != null)
               {
                  var k2 = Enum.Parse<Key>(chord.SecondStroke.Key);
                  var m2 = Enum.Parse<ModifierKeys>(chord.SecondStroke.Modifiers);
                  item.Command.Gestures.Add(new MultiKeyGesture(k1, m1, k2, m2));
               }
               else
                  item.Command.Gestures.Add(new KeyGesture(k1, m1));
            }
            catch (NotSupportedException)
            {
               MBox.Show("This key combination is not supported by the system.", "Invalid Shortcut", MBoxButton.OK, MessageBoxImage.Error);
            }
      }
   }

   private static void ExecuteReset(object sender, ExecutedRoutedEventArgs e)
   {
      if (e.Parameter is ShortcutTreeItem { Command: not null } item)
         item.Command.ResetToDefault();
   }

   private void ExecuteRemoveShortcut(object sender, ExecutedRoutedEventArgs e)
   {
      // If you want to remove a specific gesture, you'd pass the gesture as a parameter
      if (e.Parameter is InputGesture gesture && SelectedItem?.Command != null)
         SelectedItem.Command.Gestures.Remove(gesture);
   }

   private void ExecuteReplaceShortcut(object sender, ExecutedRoutedEventArgs e)
   {
      if (e.Parameter is InputGesture oldGesture && SelectedItem?.Command != null)
      {
         var recorder = new ShortcutRecorderWindow(SelectedItem?.Command!) { Owner = Window.GetWindow(this) };
         if (recorder.ShowDialog() == true && recorder.Result != null)
            try
            {
               var chord = recorder.Result;
               var k1 = Enum.Parse<Key>(chord.FirstStroke.Key);
               var m1 = Enum.Parse<ModifierKeys>(chord.FirstStroke.Modifiers);

               InputGesture newGesture;
               if (chord is { IsChord: true, SecondStroke: not null })
               {
                  var k2 = Enum.Parse<Key>(chord.SecondStroke.Key);
                  var m2 = Enum.Parse<ModifierKeys>(chord.SecondStroke.Modifiers);
                  newGesture = new MultiKeyGesture(k1, m1, k2, m2);
               }
               else
                  newGesture = new KeyGesture(k1, m1);

               if (SelectedItem == null)
                  return;

               var index = SelectedItem.Command.Gestures.IndexOf(oldGesture);
               if (index != -1)
               {
                  SelectedItem.Command.Gestures.RemoveAt(index);
                  SelectedItem.Command.Gestures.Insert(index, newGesture);
               }
            }
            catch (NotSupportedException)
            {
               MBox.Show("This key combination is not supported by the system.", "Invalid Shortcut", MBoxButton.OK, MessageBoxImage.Error);
            }
      }
   }

   private void ExpandAll_Click(object sender, RoutedEventArgs e)
   {
      foreach (var item in RootItems)
         SetExpansion(item, true);
   }

   private void CollapseAll_Click(object sender, RoutedEventArgs e)
   {
      foreach (var item in RootItems)
         SetExpansion(item, false);
   }

   private static void SetExpansion(ShortcutTreeItem item, bool isExpanded)
   {
      item.IsExpanded = isExpanded;
      foreach (var child in item.Children)
         SetExpansion(child, isExpanded);
   }

   private void OnTreeViewItemMouseRightButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is TreeViewItem treeViewItem)
      {
         treeViewItem.IsSelected = true;
         treeViewItem.Focus();
         e.Handled = true;
      }
   }

   private void ResetAll_Click(object sender, RoutedEventArgs e)
   {
      if (MBox.Show("Are you sure you want to reset ALL shortcuts to their default values?",
                    "Confirm Reset All",
                    MBoxButton.OKCancel,
                    MessageBoxImage.Warning) ==
          MBoxResult.OK)
         CommandRegistry.ResetAllToDefault();
   }
}