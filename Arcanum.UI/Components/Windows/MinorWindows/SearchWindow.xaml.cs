using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.PopUp;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SearchWindow
{
   private Queastor QueryQueastor { get; set; } = null!;

   private readonly Window _parent =
      Application.Current.MainWindow ?? throw new InvalidOperationException("MainWindow is not set.");

   public ICommand CloseCommand => new RelayCommand(Close);

   public SearchWindow()
   {
      InitializeComponent();
      SearchTextBox.RequestSearch = Search;
      SearchTextBox.SettingsOpened = OpenSettingsWindow;

      SearchResultsListBox.ItemsSource = new ObservableCollection<SearchResultItem>();

      Closing += (_, _) => AppData.SearchSettings = (SearchSettings)QueryQueastor.Settings;
      Deactivated += OnPopupDeactivated;
   }

   private bool _isClosing;

   private void OnPopupDeactivated(object? sender, EventArgs e)
   {
      if (_isClosing)
         return;

      _isClosing = true;
      Close();
      var mousePosition = Mouse.GetPosition(_parent);

      var parentRect = new Rect(_parent.Left,
                                _parent.Top,
                                _parent.Width,
                                _parent.Height);

      var popupRect = new Rect(Left,
                               Top,
                               Width,
                               Height);

      if (parentRect.Contains(mousePosition) && !popupRect.Contains(mousePosition))
         Close();

      _parent.Focus();
      Keyboard.Focus(_parent);
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
      queastor.Settings = AppData.SearchSettings;
      window.SetCategory(queastor.Settings.SearchCategory);

      window.Show();
      window.SearchTextBox.SearchInputTextBox.Focus();

      return window;
   }

   internal void Search(string query)
   {
      if (string.IsNullOrEmpty(query))
         SearchResultsListBox.ItemsSource = new ObservableCollection<ISearchResult>();
      else
         SearchResultsListBox.ItemsSource =
            new ObservableCollection<ISearchable>(QueryQueastor.Search(query));

      if (SearchResultsListBox.Items.Count == 0)
         NoResultsTextBlock.Visibility = Visibility.Visible;
      else
         NoResultsTextBlock.Visibility = Visibility.Collapsed;
   }

   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      if (e.Key == Key.Escape)
      {
         _isClosing = true;
         CloseCommand.Execute(null);
      }
      base.OnPreviewKeyDown(e);
   }

   private void SearchResultsListBox_OnMouseUp(object sender, MouseButtonEventArgs e)
   {
      var point = e.GetPosition(SearchResultsListBox);
      var element = SearchResultsListBox.InputHitTest(point) as DependencyObject;

      if (element is Run run)
         element = run.Parent;
      while (element != null && element is not ListBoxItem)
         element = VisualTreeHelper.GetParent(element);

      if (element is ListBoxItem { DataContext: ISearchable searchable })
      {
         searchable.OnSearchSelected();
         CloseCommand.Execute(null);
      }
   }

   public void SetCategory(ISearchSettings.Category category)
   {
      SettingsToggleButton.IsChecked = (category & ISearchSettings.Category.Settings) != 0;
      UiElementsToggleButton.IsChecked = (category & ISearchSettings.Category.UiElements) != 0;
      GameObjectsToggleButton.IsChecked = (category & ISearchSettings.Category.GameObjects) != 0;
      MapObjectsToggleButton.IsChecked = (category & ISearchSettings.Category.MapObjects) != 0;
      AllToggleButton.IsChecked = category == ISearchSettings.Category.All;
   }

   private void SetCategoryFromButtonButtons()
   {
      var category = ISearchSettings.Category.None;
      if (SettingsToggleButton.IsChecked == true)
         category |= ISearchSettings.Category.Settings;
      if (UiElementsToggleButton.IsChecked == true)
         category |= ISearchSettings.Category.UiElements;
      if (GameObjectsToggleButton.IsChecked == true)
         category |= ISearchSettings.Category.GameObjects;
      if (MapObjectsToggleButton.IsChecked == true)
         category |= ISearchSettings.Category.MapObjects;
      if (AllToggleButton.IsChecked == true)
         category = ISearchSettings.Category.All;

      // if all are set except AllToggleButton, we set AllToggleButton to true
      if ((category & ISearchSettings.Category.Settings) != 0 &&
          (category & ISearchSettings.Category.UiElements) != 0 &&
          (category & ISearchSettings.Category.GameObjects) != 0 &&
          (category & ISearchSettings.Category.MapObjects) != 0)
         AllToggleButton.IsChecked = true;

      QueryQueastor.Settings.SearchCategory = category;

      if (SearchTextBox.SearchInputTextBox.Text.Length > 0)
         Search(SearchTextBox.SearchInputTextBox.Text);
   }

   private void AllToggleButton_OnClick(object sender, RoutedEventArgs e)
   {
      var newState = AllToggleButton.IsChecked;
      if (newState == true)
      {
         SettingsToggleButton.IsChecked = newState;
         UiElementsToggleButton.IsChecked = newState;
         GameObjectsToggleButton.IsChecked = newState;
         MapObjectsToggleButton.IsChecked = newState;
      }

      SetCategoryFromButtonButtons();
   }

   private void SettingsToggleButton_OnClick(object sender, RoutedEventArgs e)
   {
      if (sender is not ToggleButton toggleButton)
         return;

      if (toggleButton.IsChecked == false)
         AllToggleButton.IsChecked = false;

      SetCategoryFromButtonButtons();
   }

   private void OpenSettingsWindow()
   {
      var settingsPropWindow =
         new PropertyGridWindow(QueryQueastor.Settings)
         {
            Title = "Search Settings", WindowStartupLocation = WindowStartupLocation.CenterScreen,
         };
      settingsPropWindow.ShowDialog();
   }
}