using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Nexus.Core;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class NexusAccessor
{
   private Dictionary<string, Type> MasterSuggestionList { get; init; }
   private Dictionary<Type, IEu5Object> EmptyObjectsByType => GetEmptyObjectsDictionary();

   public static readonly DependencyProperty ValuePreviewProperty =
      DependencyProperty.Register(nameof(ValuePreview),
                                  typeof(string),
                                  typeof(NexusAccessor),
                                  new("null"));

   public string ValuePreview
   {
      get => (string)GetValue(ValuePreviewProperty);
      set => SetValue(ValuePreviewProperty, value);
   }

   public static readonly DependencyProperty IsDropDownOpenProperty =
      DependencyProperty.Register(nameof(IsDropDownOpen),
                                  typeof(bool),
                                  typeof(NexusAccessor),
                                  new(false));

   public bool IsDropDownOpen
   {
      get => (bool)GetValue(IsDropDownOpenProperty);
      set => SetValue(IsDropDownOpenProperty, value);
   }

   // Holds the filtered list of suggestions for the ListBox
   public static readonly DependencyProperty SuggestionsProperty =
      DependencyProperty.Register(nameof(Suggestions),
                                  typeof(ObservableCollection<string>),
                                  typeof(NexusAccessor),
                                  new(new ObservableCollection<string>()));

   public ObservableCollection<string> Suggestions
   {
      get => (ObservableCollection<string>)GetValue(SuggestionsProperty);
      set => SetValue(SuggestionsProperty, value);
   }

   public NexusAccessor()
   {
      InitializeComponent();
      Suggestions = [];
      MasterSuggestionList = Eu5ObjectsRegistry.Eu5Objects.ToDictionary(o => o.Name, o => o);
   }

   private void NexusTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
   {
      var dotWasJustTyped = e.Changes.Count == 1 &&
                            e.Changes.First().AddedLength == 1 &&
                            NexusTextBox.Text.EndsWith(".");

      if (dotWasJustTyped)
         // Force the dropdown to show all members for the new path.
         UpdateAndShowSuggestions(forceShowAll: true);
      else
         // Otherwise, just update normally as the user types.
         UpdateAndShowSuggestions(forceShowAll: false);
   }

   private void SuggestionsListBox_OnKeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Enter)
         SelectCurrentItem();
      else if (e.Key == Key.Escape)
         IsDropDownOpen = false;
   }

   private void SuggestionsListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
   {
      // This handler ensures that clicking an item in the listbox selects it.
      // We use PreviewMouseDown because the Popup might close before a normal Click event fires.
      if (e.Source is FrameworkElement { DataContext: string selectedItem })
      {
         NexusTextBox.Text = selectedItem;
         IsDropDownOpen = false;
         // Move caret to the end of the text
         NexusTextBox.CaretIndex = NexusTextBox.Text.Length;
         e.Handled = true; // Prevents other events from firing
      }
   }

   private void NexusTextBox_OnLostFocus(object sender, RoutedEventArgs e)
   {
      // A safety check to ensure the popup closes when the textbox loses focus
      // The StaysOpen=False property on the Popup handles most cases, but this is a good backup.
      if (!SuggestionsListBox.IsKeyboardFocusWithin)
         IsDropDownOpen = false;
   }

   private void NexusTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
   {
      // --- NEW LOGIC FOR CTRL + SPACE ---
      if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
      {
         // Force the dropdown to show all relevant suggestions
         UpdateAndShowSuggestions(forceShowAll: true);
         e.Handled = true; // This prevents a space from being typed into the textbox
         return;
      }
      // --- END OF NEW LOGIC ---

      if (e.Key == Key.Down && IsDropDownOpen && Suggestions.Any())
      {
         // Move focus to the ListBox
         SuggestionsListBox.Focus();
         SuggestionsListBox.SelectedIndex = 0;
         e.Handled = true; // Prevent the caret from moving in the textbox
      }
      else if (e.Key == Key.Escape)
      {
         IsDropDownOpen = false;
         e.Handled = true; // Prevent any other action for the Escape key
      }
   }

   private void SelectCurrentItem()
   {
      if (SuggestionsListBox.SelectedItem is string selectedItem)
      {
         NexusTextBox.Text = selectedItem;
         IsDropDownOpen = false;
         NexusTextBox.CaretIndex = NexusTextBox.Text.Length;
         NexusTextBox.Focus();
      }
   }

   private void UpdateAndShowSuggestions(bool forceShowAll = false)
   {
      var currentText = NexusTextBox.Text;

      if (!forceShowAll && string.IsNullOrWhiteSpace(currentText))
      {
         IsDropDownOpen = false;
         return;
      }

      List<string> suggestionSource;
      string filterText;

      var lastDotIndex = currentText.LastIndexOf('.');

      if (lastDotIndex == -1)
      {
         suggestionSource = MasterSuggestionList.Keys.ToList();
         filterText = currentText;
      }
      else
      {
         var path = currentText[..lastDotIndex];
         filterText = currentText[(lastDotIndex + 1)..];

         suggestionSource = GetMembersForPath(path);
      }

      var filteredSuggestions = suggestionSource
                               .Where(s => s.StartsWith(filterText, StringComparison.OrdinalIgnoreCase))
                               .ToList();

      Suggestions.Clear();
      foreach (var suggestion in filteredSuggestions)
         Suggestions.Add(suggestion);

      IsDropDownOpen = Suggestions.Any() || (forceShowAll && suggestionSource.Count != 0);

      if (IsDropDownOpen && forceShowAll && !Suggestions.Any())
         foreach (var suggestion in suggestionSource)
            Suggestions.Add(suggestion);
   }

   /// <summary>
   /// Resolves an object path (e.g., "Player.Position") and returns the names of its public instance properties.
   /// </summary>
   /// <param name="path">The object path string.</param>
   /// <returns>A list of member names, or an empty list if the path is invalid.</returns>
   private List<string> GetMembersForPath(string path)
   {
      if (string.IsNullOrWhiteSpace(path))
         return [];

      var parts = path.Split('.');

      if (!MasterSuggestionList.TryGetValue(parts[0], out var currentType))
         return [];

      if (!EmptyObjectsByType.TryGetValue(currentType, out var currentInstance))
         return [];

      for (var i = 1; i < parts.Length; i++)
      {
         var propertyName = parts[i];

         var properties = currentInstance.GetAllProperties();
         var propInfo = properties.FirstOrDefault(p => p.ToString() == propertyName);

         if (propInfo == null)
            return [];

         var propertyType = Nx.TypeOf(currentInstance, propInfo);

         if (!EmptyObjectsByType.TryGetValue(propertyType, out currentInstance))
         {
            // Check if we have an observable collection or HashSet
            if (!propertyType.IsGenericType)
               return [];

            var genericDef = propertyType.GetGenericTypeDefinition();
            if (genericDef != typeof(ObservableRangeCollection<>) && genericDef != typeof(HashSet<>))
               return [];

            var itemType = propertyType.GetGenericArguments()[0];
            if (EmptyObjectsByType.TryGetValue(itemType, out currentInstance))
               continue;

            return [];
         }
      }

      return currentInstance.GetAllProperties()
                            .Select(p => p.ToString())
                            .OrderBy(name => name)
                            .ToList();
   }

   public static Dictionary<Type, IEu5Object> GetEmptyObjectsDictionary()
   {
      var dict = new Dictionary<Type, IEu5Object>();
      foreach (var type in Eu5ObjectsRegistry.Eu5Objects)
      {
         if (type is null)
            continue;

         if (Activator.CreateInstance(type) is IEu5Object instance)
            dict[type] = instance;
      }

      return dict;
   }

   private void NexusTextBox_OnGotFocus(object sender, RoutedEventArgs e)
   {
      UpdateAndShowSuggestions();
   }
}