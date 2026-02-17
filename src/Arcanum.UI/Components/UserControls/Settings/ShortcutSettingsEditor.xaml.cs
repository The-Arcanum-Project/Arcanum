using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.Commands;
using Arcanum.UI.Commands.KeyMap;
using Arcanum.UI.Components.Views.Settings;

namespace Arcanum.UI.Components.UserControls.Settings;

public partial class ShortcutSettingsEditor
{
   public static readonly DependencyProperty RootItemsProperty =
      DependencyProperty.Register(nameof(RootItems), typeof(ObservableCollection<ShortcutTreeItem>), typeof(ShortcutSettingsEditor));

   public ShortcutSettingsEditor()
   {
      InitializeComponent();
      // Set the DataContext to itself so the TreeView can find RootItems
      DataContext = this;
      BuildTree();

      // Initialize Command Bindings
      CommandBindings.Add(new(AddShortcutCommand, ExecuteAddShortcut));
      CommandBindings.Add(new(ResetCommand, ExecuteReset));
      CommandBindings.Add(new(RemoveShortcutCommand, ExecuteRemoveShortcut));
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

   private void BuildTree()
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

      RootItems = new(root.Children);
   }

   private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      var query = SearchBox.Text;
      foreach (var item in RootItems)
         FilterItem(item, query);
   }

   private static bool FilterItem(ShortcutTreeItem item, string query)
   {
      if (string.IsNullOrWhiteSpace(query))
      {
         item.IsVisible = true;
         foreach (var child in item.Children)
            FilterItem(child, query);
         return true;
      }

      var nameMatches = item.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
      var anyChildMatches = false;

      foreach (var child in item.Children)
         if (FilterItem(child, query))
            anyChildMatches = true;

      var finalVisibility = nameMatches || anyChildMatches;
      item.IsVisible = finalVisibility;

      // Expand folders that contain matches
      if (!item.IsCommand && finalVisibility)
         item.IsExpanded = true;

      return finalVisibility;
   }

   // --- Command Implementations ---

   private void ExecuteAddShortcut(object sender, ExecutedRoutedEventArgs e)
   {
      if (e.Parameter is ShortcutTreeItem item && item.Command != null)
      {
         var recorder = new ShortcutRecorderWindow { Owner = Window.GetWindow(this) };

         if (recorder.ShowDialog() == true && recorder.Result != null)
         {
            var chord = recorder.Result;

            // Convert the strings back to WPF Enums
            var k1 = Enum.Parse<Key>(chord.FirstStroke.Key);
            var m1 = Enum.Parse<ModifierKeys>(chord.FirstStroke.Modifiers);

            if (chord.IsChord && chord.SecondStroke != null)
            {
               var k2 = Enum.Parse<Key>(chord.SecondStroke.Key);
               var m2 = Enum.Parse<ModifierKeys>(chord.SecondStroke.Modifiers);

               // Use your MultiKeyGesture constructor
               item.Command.Gestures.Add(new MultiKeyGesture(k1, m1, k2, m2));
            }
            else
            {
               // Standard WPF KeyGesture
               item.Command.Gestures.Add(new KeyGesture(k1, m1));
            }
         }
      }
   }

   private void ExecuteReset(object sender, ExecutedRoutedEventArgs e)
   {
      if (e.Parameter is ShortcutTreeItem item && item.Command != null)
      {
         // Implementation depends on your CommandLibrary storing defaults
         // For now, clear and add back one common default
         item.Command.Gestures.Clear();
         MessageBox.Show($"Resetting: {item.Name}");
      }
   }

   private void ExecuteRemoveShortcut(object sender, ExecutedRoutedEventArgs e)
   {
      // If you want to remove a specific gesture, you'd pass the gesture as a parameter
      if (e.Parameter is InputGesture gesture && SelectedItem?.Command != null)
         SelectedItem.Command.Gestures.Remove(gesture);
   }
}