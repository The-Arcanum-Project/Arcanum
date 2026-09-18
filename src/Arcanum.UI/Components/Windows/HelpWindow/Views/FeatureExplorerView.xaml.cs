#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.Views;

public partial class FeatureExplorerView
{
   public FeatureExplorerView()
   {
      InitializeComponent();

      Loaded += FeatureExplorerView_Loaded;
      Unloaded += FeatureExplorerView_Unloaded;
   }

   private void FeatureExplorerView_Loaded(object sender, RoutedEventArgs e)
   {
      if (DataContext is FeatureExplorerViewModel { Features.Count: > 0 } vm)
         vm.SelectedItem = vm.Features[0];
   }

   private void FeatureExplorerView_Unloaded(object sender, RoutedEventArgs e)
   {
      Loaded -= FeatureExplorerView_Loaded;
      Unloaded -= FeatureExplorerView_Unloaded;
   }

   private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount != 2)
         return;

      if (sender is FrameworkElement { DataContext: IAppCommand cmd } && cmd.CanExecute(null))
      {
         cmd.Execute(null);
         Window.GetWindow(this)?.Close();
      }

      e.Handled = true;
   }

   private void ListView_OnSelectedItemChanged(object sender, RoutedEventArgs e)
   {
      if (sender is not ListView listView)
         return;

      if (listView.SelectedItem is FeatureItem selectedItem)
         (DataContext as FeatureExplorerViewModel)?.SelectedItem = selectedItem;
   }

   private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
   {
      var vm = DataContext as FeatureExplorerViewModel;
      if (vm == null || !vm.IsSuggesting)
         return;

      if (e.Key == Key.Down)
      {
         vm.SuggestionIndex = (vm.SuggestionIndex + 1) % vm.SearchSuggestions.Count;
         e.Handled = true;
      }
      else if (e.Key == Key.Up)
      {
         vm.SuggestionIndex = vm.SuggestionIndex <= 0 ? vm.SearchSuggestions.Count - 1 : vm.SuggestionIndex - 1;
         e.Handled = true;
      }
      else if (e.Key is Key.Enter or Key.Tab)
      {
         // TAB and ENTER now both trigger completion
         if (vm.SuggestionIndex >= 0 && vm.SuggestionIndex < vm.SearchSuggestions.Count)
         {
            ApplySuggestion(vm.SearchSuggestions[vm.SuggestionIndex]);
            e.Handled = true; // This prevents Tab from moving focus
         }
         else if (e.Key == Key.Tab && vm.SearchSuggestions.Count > 0)
         {
            // If nothing is highlighted but Tab is pressed, take the first one
            ApplySuggestion(vm.SearchSuggestions[0]);
            e.Handled = true;
         }
      }
      else if (e.Key == Key.Escape)
      {
         vm.IsSuggesting = false;
         e.Handled = true;
      }
   }

   private void ApplySuggestion(string suggestion)
   {
      if (DataContext is FeatureExplorerViewModel vm)
      {
         vm.CompleteSearch(suggestion);

         SearchBox.Focus();
         SearchBox.CaretIndex = SearchBox.Text.Length;
      }
   }

   private void Suggestion_MouseClick(object sender, MouseButtonEventArgs e)
   {
      if (sender is FrameworkElement { DataContext: string suggestion })
         ApplySuggestion(suggestion);
   }

   private void FeatureExplorerView_KeyUp(object sender, KeyEventArgs e)
   {
      if (DataContext is not FeatureExplorerViewModel vm)
         return;

      if (e.Key == Key.Escape && vm.IsSuggesting)
      {
         vm.IsSuggesting = false;
         e.Handled = true;
      }
   }
}