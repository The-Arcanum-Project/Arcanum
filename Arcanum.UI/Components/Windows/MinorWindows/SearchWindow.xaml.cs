using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.UI.Components.StyleClasses;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SearchWindow
{
   private Queastor QueryQueastor { get; set; } = null!;

   public ICommand CloseCommand => new RelayCommand(Close);

   public SearchWindow()
   {
      InitializeComponent();
      SearchTextBox.RequestSearch = Search;

      SearchResultsListBox.ItemsSource = new ObservableCollection<SearchResultItem>();
   }

   /// <summary>
   /// Shows the search window with the given query and the Queastor.GlobalInstance as the Queastor.
   /// </summary>
   /// <param name="query"></param>
   /// <param name="alwaysOnTop"></param>
   /// <returns></returns>
   public static SearchWindow ShowSearchWindow(string query = "", bool alwaysOnTop = false)
   {
      return ShowSearchWindow(query, alwaysOnTop, Queastor.GlobalInstance);
   }

   public static SearchWindow ShowSearchWindow(string query, bool alwaysOnTop, Queastor queastor)
   {
      var window = new SearchWindow { Topmost = alwaysOnTop, QueryQueastor = queastor };
      window.SearchTextBox.SearchInputTextBox.Text = query;
      window.Search(query);

      window.Show();
      window.SearchTextBox.SearchInputTextBox.Focus();

      return window;
   }

   internal void Search(string query)
   {
      if (string.IsNullOrEmpty(query))
      {
         SearchResultsListBox.ItemsSource = new ObservableCollection<ISearchResult>();
         return;
      }

      SearchResultsListBox.ItemsSource =
         new ObservableCollection<ISearchable>(QueryQueastor.Search(query));
   }

   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      if (e.Key == Key.Escape)
         CloseCommand.Execute(null);
      base.OnPreviewKeyDown(e);
   }

   private void SearchResultsListBox_OnMouseUp(object sender, MouseButtonEventArgs e)
   {
      var point = e.GetPosition(SearchResultsListBox);
      var element = SearchResultsListBox.InputHitTest(point) as DependencyObject;

      while (element != null && element is not ListBoxItem)
         element = VisualTreeHelper.GetParent(element);

      if (element is ListBoxItem { DataContext: ISearchable searchable })
      {
         searchable.OnSearchSelected();
         CloseCommand.Execute(null);
      }
   }
}