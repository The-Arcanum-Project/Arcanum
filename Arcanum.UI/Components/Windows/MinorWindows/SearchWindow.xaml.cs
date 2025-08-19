using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.PopUp;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SearchWindow : INotifyPropertyChanged
{
   private Queastor QueryQueastor { get; set; } = null!;
   private static string _lastSearchQuery = string.Empty;

   // private readonly Window _parent =
   //    Application.Current.MainWindow ?? throw new InvalidOperationException("MainWindow is not set.");

   public static readonly DependencyProperty ShowCountProperty =
      DependencyProperty.Register(nameof(ShowCount),
                                  typeof(bool),
                                  typeof(SearchWindow),
                                  new(true));

   private int _searchResultCount;

   public bool ShowCount
   {
      get => (bool)GetValue(ShowCountProperty);
      set => SetValue(ShowCountProperty, value);
   }

   public int SearchResultCount
   {
      get => _searchResultCount;
      private set
      {
         if (value == _searchResultCount)
            return;

         _searchResultCount = value;
         OnPropertyChanged();
      }
   }

   public ICommand CloseCommand => new RelayCommand(Close);

   public SearchWindow()
   {
      InitializeComponent();
      SearchTextBox.RequestSearch = Search;
      SearchTextBox.SettingsOpened = OpenSettingsWindow;

      SearchResultsListBox.ItemsSource = new ObservableCollection<SearchResultItem>();

      Closing += (_, _) => AppData.QueastorSearchSettings = (QueastorSearchSettings)QueryQueastor.Settings;
      Deactivated += OnPopupDeactivated;

      SearchTextBox.SearchInputTextBox.TextChanged += (sender, _) =>
      {
         if (sender is TextBox textBox)
            _lastSearchQuery = textBox.Text;
      };

      Loaded += (_, _) =>
      {
         SearchTextBox.SearchInputTextBox.Text = _lastSearchQuery;
         SearchTextBox.SearchInputTextBox.CaretIndex = _lastSearchQuery.Length;
      };
   }

   // private bool _isClosing;

   private void OnPopupDeactivated(object? sender, EventArgs e)
   {
      // if (_isClosing)
      //    return;
      //
      // _isClosing = true;
      // Close();
      // var mousePosition = Mouse.GetPosition(_parent);
      //
      // var parentRect = new Rect(_parent.Left,
      //                           _parent.Top,
      //                           _parent.Width,
      //                           _parent.Height);
      //
      // var popupRect = new Rect(Left,
      //                          Top,
      //                          Width,
      //                          Height);
      //
      // if (parentRect.Contains(mousePosition) && !popupRect.Contains(mousePosition))
      //    Close();
      //
      // _parent.Focus();
      // Keyboard.Focus(_parent);
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
      queastor.Settings = AppData.QueastorSearchSettings;
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

      SearchResultCount = SearchResultsListBox.Items.Count;
   }

   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      if (e.Key == Key.Escape)
      {
         // _isClosing = true;
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
         ExecuteOnSelected(searchable);
   }

   private void ExecuteOnSelected(ISearchable searchable)
   {
      searchable.OnSearchSelected();
      CloseCommand.Execute(null);
   }

   public void SetCategory(IQueastorSearchSettings.Category category)
   {
      SettingsToggleButton.IsChecked = (category & IQueastorSearchSettings.Category.Settings) != 0;
      UiElementsToggleButton.IsChecked = (category & IQueastorSearchSettings.Category.UiElements) != 0;
      GameObjectsToggleButton.IsChecked = (category & IQueastorSearchSettings.Category.GameObjects) != 0;
      MapObjectsToggleButton.IsChecked = (category & IQueastorSearchSettings.Category.MapObjects) != 0;
      AllToggleButton.IsChecked = category == IQueastorSearchSettings.Category.All;
   }

   private void SetCategoryFromButtonButtons()
   {
      var category = IQueastorSearchSettings.Category.None;
      if (SettingsToggleButton.IsChecked == true)
         category |= IQueastorSearchSettings.Category.Settings;
      if (UiElementsToggleButton.IsChecked == true)
         category |= IQueastorSearchSettings.Category.UiElements;
      if (GameObjectsToggleButton.IsChecked == true)
         category |= IQueastorSearchSettings.Category.GameObjects;
      if (MapObjectsToggleButton.IsChecked == true)
         category |= IQueastorSearchSettings.Category.MapObjects;
      if (AllToggleButton.IsChecked == true)
         category = IQueastorSearchSettings.Category.All;

      // if all are set except AllToggleButton, we set AllToggleButton to true
      if ((category & IQueastorSearchSettings.Category.Settings) != 0 &&
          (category & IQueastorSearchSettings.Category.UiElements) != 0 &&
          (category & IQueastorSearchSettings.Category.GameObjects) != 0 &&
          (category & IQueastorSearchSettings.Category.MapObjects) != 0)
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

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   private void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
   {
      switch (e.Key)
      {
         case Key.Enter:
            var listViewItem = SearchResultsListBox.SelectedItem;
            if (listViewItem is ISearchable searchable)
               ExecuteOnSelected(searchable);
            e.Handled = true;
            break;
         case Key.Down:
            if (SearchResultsListBox.Items.Count > 0)
            {
               if (SearchResultsListBox.SelectedIndex < SearchResultsListBox.Items.Count - 1)
                  SearchResultsListBox.SelectedIndex++;
               else
                  SearchResultsListBox.SelectedIndex = 0;

               SearchResultsListBox.ScrollIntoView(SearchResultsListBox.SelectedItem);
            }
            e.Handled = true;
            break;
         case Key.Up:
            if (SearchResultsListBox.Items.Count > 0)
            {
               if (SearchResultsListBox.SelectedIndex > 0)
                  SearchResultsListBox.SelectedIndex--;
               else
                  SearchResultsListBox.SelectedIndex = SearchResultsListBox.Items.Count - 1;

               SearchResultsListBox.ScrollIntoView(SearchResultsListBox.SelectedItem);
            }
            e.Handled = true;
            break;
      }
   }
}