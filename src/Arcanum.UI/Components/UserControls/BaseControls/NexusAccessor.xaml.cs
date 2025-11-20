using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.PathAccessorEngine;
using Arcanum.Core.Registry;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class NexusAccessor
{
   private Dictionary<string, Type> MasterSuggestionList { get; init; }

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
                            NexusTextBox.Text.EndsWith('.');

      Suggestions = new(PathAccessor.GetSuggestions(NexusTextBox.Text, out var open, forceShowAll: dotWasJustTyped));
      IsDropDownOpen = open;
      var pa = new PathAccessor(NexusTextBox.Text);
      ValuePreview = pa.CalculateExampleValue(null);
   }

   private void SuggestionsListBox_OnKeyDown(object sender, KeyEventArgs e)
   {
      // --- CTRL + SPACE ---
      if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
      {
         Suggestions = new(PathAccessor.GetSuggestions(NexusTextBox.Text, out var open, forceShowAll: true));
         IsDropDownOpen = open;
         e.Handled = true;
         return;
      }

      if (!IsDropDownOpen || !Suggestions.Any())
         return;

      switch (e.Key)
      {
         case Key.Down:
            var newIndexDown = SuggestionsListBox.SelectedIndex + 1;
            if (newIndexDown < Suggestions.Count)
            {
               SuggestionsListBox.SelectedIndex = newIndexDown;
               SuggestionsListBox.ScrollIntoView(SuggestionsListBox.SelectedItem);
            }

            e.Handled = true;
            break;

         case Key.Up:
            var newIndexUp = SuggestionsListBox.SelectedIndex - 1;
            if (newIndexUp >= 0)
            {
               SuggestionsListBox.SelectedIndex = newIndexUp;
               SuggestionsListBox.ScrollIntoView(SuggestionsListBox.SelectedItem);
            }

            e.Handled = true;
            break;

         case Key.Enter:
         case Key.Tab:
            if (SuggestionsListBox.SelectedItem != null)
               SelectCurrentItem();

            IsDropDownOpen = false;
            e.Handled = true;
            break;

         case Key.Escape:
            IsDropDownOpen = false;
            e.Handled = true;
            break;

         case Key.Space:
            e.Handled = true;
            break;

         default:
            // Put the focus back to the TextBox and let it handle the key
            NexusTextBox.Focus();
            e.Handled = false;
            break;
      }
   }

   private void NexusTextBox_OnLostFocus(object sender, RoutedEventArgs e)
   {
      if (!SuggestionsListBox.IsKeyboardFocusWithin)
         IsDropDownOpen = false;
   }

   private void NexusTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
   {
      switch (e.Key)
      {
         // CTRL + SPACE ---
         case Key.Space when Keyboard.Modifiers == ModifierKeys.Control:
            Suggestions = new(PathAccessor.GetSuggestions(NexusTextBox.Text, out var open, forceShowAll: true));
            IsDropDownOpen = open;
            e.Handled = true;
            return;
         case Key.Down when IsDropDownOpen && Suggestions.Any():
         {
            SuggestionsListBox.Focus();
            SuggestionsListBox.SelectedIndex = 0;

            if (SuggestionsListBox.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem firstItem)
               firstItem.Focus();
            e.Handled = true;
            break;
         }
         case Key.Escape:
            IsDropDownOpen = false;
            e.Handled = true;
            break;
         case Key.Space:
            e.Handled = true;
            break;
      }
   }

   private void SelectCurrentItem()
   {
      if (SuggestionsListBox.SelectedItem is string selectedItem)
         AppendSuggestion(selectedItem);
   }

   private void AppendSuggestion(string suggestion)
   {
      if (string.IsNullOrWhiteSpace(suggestion))
         return;

      var lastDotIndex = NexusTextBox.Text.LastIndexOf('.');
      if (lastDotIndex == -1)
         NexusTextBox.Text = suggestion;
      else
         NexusTextBox.Text = NexusTextBox.Text[..(lastDotIndex + 1)] + suggestion;

      NexusTextBox.CaretIndex = NexusTextBox.Text.Length;
      NexusTextBox.Focus();
   }

   private void NexusTextBox_OnGotFocus(object sender, RoutedEventArgs e)
   {
      Suggestions = new(PathAccessor.GetSuggestions(NexusTextBox.Text, out var isOpen));
      IsDropDownOpen = isOpen;

      var pa = new PathAccessor(NexusTextBox.Text);
      ValuePreview = pa.CalculateExampleValue(null);
   }
}