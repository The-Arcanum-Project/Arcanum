using System.Windows;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class SuggestionPopup
{
   public SuggestionPopup()
   {
      InitializeComponent();
   }

   public void SetPlacementTarget(UIElement target)
   {
      MainPopup.PlacementTarget = target;
   }

   public void UpdateSuggestions(IEnumerable<string> suggestions)
   {
      var list = suggestions.ToList();

      if (list.Count == 0)
      {
         MainPopup.IsOpen = false;
         return;
      }

      SuggestionList.ItemsSource = list;
      SuggestionList.SelectedIndex = 0;
      MainPopup.IsOpen = true;

      if (MainPopup.PlacementTarget is FrameworkElement fe)
         MainPopup.Width = fe.ActualWidth;
   }

   public bool MoveSelection(int direction)
   {
      if (!MainPopup.IsOpen || SuggestionList.Items.Count == 0)
         return false;

      var newIndex = SuggestionList.SelectedIndex + direction;

      if (newIndex < 0)
         newIndex = SuggestionList.Items.Count - 1;
      if (newIndex >= SuggestionList.Items.Count)
         newIndex = 0;

      SuggestionList.SelectedIndex = newIndex;
      SuggestionList.ScrollIntoView(SuggestionList.SelectedItem);
      return true;
   }

   public string? GetSelectedSuggestion()
   {
      if (!MainPopup.IsOpen)
         return null;

      return SuggestionList.SelectedItem as string;
   }

   public void Close() => MainPopup.IsOpen = false;

   public bool IsOpen => MainPopup.IsOpen;
}