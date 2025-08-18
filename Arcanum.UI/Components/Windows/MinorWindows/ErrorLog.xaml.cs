using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.Utils;
using Arcanum.UI.Components.Windows.PopUp;
using Path = System.IO.Path;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ErrorLog : INotifyPropertyChanged
{
   public enum FilterType
   {
      None,
      Severity,
      Name,
      Id,
      Message,
      Description,
      ErrorAction,
      Resolution,
   }

   private string _errorName = "Error Name";
   public string ErrorName
   {
      get => _errorName;
      set
      {
         if (_errorName == value)
            return;

         _errorName = value;
         OnPropertyChanged();
      }
   }

   private string _errorMessage = "Error Message";
   public string ErrorMessage
   {
      get => _errorMessage;
      set
      {
         if (_errorMessage == value)
            return;

         _errorMessage = value;
         OnPropertyChanged();
      }
   }

   private string _errorDescription = "Error Description";
   public string ErrorDescription
   {
      get => _errorDescription;
      set
      {
         if (_errorDescription == value)
            return;

         _errorDescription = value;
         OnPropertyChanged();
      }
   }

   private string _errorResolution = string.Empty;
   public string ErrorResolution
   {
      get => _errorResolution;
      set
      {
         if (_errorResolution == value)
            return;

         _errorResolution = value;
         OnPropertyChanged();
      }
   }

   private DiagnosticSeverity _selectedSeverity = DiagnosticSeverity.Error;
   public DiagnosticSeverity SelectedSeverity
   {
      get => _selectedSeverity;
      set
      {
         if (_selectedSeverity == value)
            return;

         _selectedSeverity = value;
         OnPropertyChanged();
      }
   }
   
   private string _selectedPath = string.Empty;
   public string SelectedPath
   {
      get => _selectedPath;
      set
      {
         if (_selectedPath == value)
            return;
         _selectedPath = value;
         OnPropertyChanged();
      }
   }

   private readonly SimpleCollectionViewFilterProvider _filterProvider = new();
   private SimpleSearchSettings SearchSettings { get; set; } = new();
   private bool _isFullyLoaded;

   private string? FilterPropertyPath => FilterComboBox.SelectedItem is not FilterType filterType
                                            ? null
                                            : filterType switch
                                            {
                                               FilterType.Severity => nameof(Diagnostic.Severity),
                                               FilterType.Name => "Descriptor.Name",
                                               FilterType.Id => "Descriptor.Id",
                                               FilterType.Message => "Descriptor.Message",
                                               FilterType.Description => "Descriptor.Description",
                                               FilterType.ErrorAction => nameof(Diagnostic.Action),
                                               FilterType.Resolution => "Descriptor.Resolution",
                                               _ => null,
                                            };

   public ErrorLog()
   {
      InitializeComponent();

      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError1,
                                       new(0, 0, "TestFile.txt"),
                                       DiagnosticSeverity.Error,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));

      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError1,
                                       new(0, 0, "TestFile.txt"),
                                       DiagnosticSeverity.Warning,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));
      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError2,
                                       new(0, 0, "TestFile.txt"),
                                       DiagnosticSeverity.Information,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));
      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.UnknownError,
                                       new(0, 0, "TestFile.txt"),
                                       DiagnosticSeverity.Error,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));

      Loaded += OnLoaded;
   }

   private void OnLoaded(object sender, RoutedEventArgs e)
   {
      FilterComboBox.SelectedIndex = 0;
      FilterComboBox.SelectionChanged += (_, _) => QuerySearch(SearchTextBox.SearchInputTextBox.Text);

      FilterComboBox.ItemsSource = Enum.GetValues(typeof(FilterType));
      ErrorLogDataGrid.ItemsSource = new ListCollectionView(ErrorManager.Diagnostics);

      SearchTextBox.RequestSearch = QuerySearch;
      SearchTextBox.SearchInputTextBox.TextChanged += (_, _) =>
      {
         if (FilterComboBox.SelectedItem is not FilterType.Severity)
            return;

         var text = SearchTextBox.SearchInputTextBox.Text.Trim();

         ErrorCheckBox.IsChecked = text.Contains(nameof(DiagnosticSeverity.Error),
                                                 StringComparison.OrdinalIgnoreCase);
         WarningCheckBox.IsChecked = text.Contains(nameof(DiagnosticSeverity.Warning),
                                                   StringComparison.OrdinalIgnoreCase);
         InformationCheckBox.IsChecked = text.Contains(nameof(DiagnosticSeverity.Information),
                                                       StringComparison.OrdinalIgnoreCase);
      };

      SearchTextBox.SettingsOpened = () =>
      {
         var settingsPropWindow =
            new PropertyGridWindow(SearchSettings)
            {
               Title = "Search Settings", WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
         settingsPropWindow.ShowDialog();
         QuerySearch(SearchTextBox.SearchInputTextBox.Text);
      };
      
      _isFullyLoaded = true;
   }

   private void QuerySearch(string query)
   {
      if (!_isFullyLoaded)
         return;

      if (ErrorLogDataGrid.ItemsSource is not ListCollectionView lcv)
         throw new InvalidOperationException("DataContext is not a CollectionView.");

      lcv.SortDescriptions.Clear();

      lcv.Filter =
         SimpleCollectionViewFilterProvider.GenerateFilter(SearchSettings,
                                                           query,
                                                           FilterPropertyPath ?? string.Empty);

      // check if we have items to sort
      if (lcv.Count < 2)
         lcv.SortDescriptions.Add(new(FilterPropertyPath ?? string.Empty,
                                      SearchSettings.SortingOption == ISearchSettings.SortingOptions.Acending
                                         ? ListSortDirection.Ascending
                                         : ListSortDirection.Descending));
      lcv.Refresh();
   }

   private void ErrorLogListView_OnSelectionChanged(object sender, SelectedCellsChangedEventArgs s)
   {
      if (ErrorLogDataGrid.SelectedItem is Diagnostic diagnostic)
      {
         ErrorName = diagnostic.Descriptor.Name;
         ErrorMessage = diagnostic.Message;
         ErrorDescription = diagnostic.Description;
         ErrorResolution = diagnostic.Descriptor.Resolution;
         SelectedPath = FileManager.SanitizePath(diagnostic.Context.FilePath);
      }
      else
      {
         ErrorName = "Error Name";
         ErrorMessage = "Error Message";
         ErrorDescription = "Error Description";
         ErrorResolution = string.Empty;
         SelectedPath = string.Empty;
      }
      
      if (ErrorLogDataGrid.SelectedItem is not Diagnostic selectedDiagnostic)
         return;
      
      SelectedSeverity = selectedDiagnostic.Severity;
      
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

   private void CheckBox_OnClick(object sender, RoutedEventArgs e)
   {
      if (sender is not CheckBox checkBox)
         return;

      if (SearchTextBox.SearchInputTextBox.Text.Contains(checkBox.Content.ToString() ?? string.Empty,
                                                         StringComparison.OrdinalIgnoreCase))
         return;

      FilterComboBox.SelectedItem = FilterType.Severity;
      if (OnlySeverityFiltered())
         SearchTextBox.SearchInputTextBox.Text += ' ' + checkBox.Content.ToString();
      else
         SearchTextBox.SearchInputTextBox.Text = checkBox.Content.ToString() ?? string.Empty;
   }

   private void CheckBoxUnCheck_OnClick(object sender, RoutedEventArgs e)
   {
      if (sender is not CheckBox checkBox)
         return;

      if (!SearchTextBox.SearchInputTextBox.Text.Contains(checkBox.Content.ToString() ?? string.Empty,
                                                          StringComparison.OrdinalIgnoreCase))
         return;

      FilterComboBox.SelectedItem = FilterType.None;
      if (!OnlySeverityFiltered())
         SearchTextBox.SearchInputTextBox.Text = string.Empty;
      else
      {
         var indexOf = SearchTextBox.SearchInputTextBox.Text.IndexOf(checkBox.Content.ToString() ?? string.Empty,
                                                                     StringComparison.OrdinalIgnoreCase);

         if (indexOf < 0)
            return;

         var length = checkBox.Content.ToString()?.Length ?? 0;

         if (indexOf != 0)
         {
            indexOf -= 1;
            length += 1;
         }

         SearchTextBox.SearchInputTextBox.Text = SearchTextBox.SearchInputTextBox.Text.Remove(indexOf, length);
      }
   }

   private bool OnlySeverityFiltered()
   {
      var currentSearchString = SearchTextBox.SearchInputTextBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

      var enumValues = Enum.GetNames(typeof(DiagnosticSeverity));

      return currentSearchString.All(x => enumValues.Any(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      if (ErrorLogDataGrid.SelectedItem is not Diagnostic selectedDiagnostic)
         return;

      ProcessHelper.OpenFolder(selectedDiagnostic.Context.FilePath);
   }

   private void OpenFileAtPos_OnClick(object sender, RoutedEventArgs e)
   {
      if (ErrorLogDataGrid.SelectedItem is not Diagnostic selectedDiagnostic)
         return;

      ProcessHelper.OpenFileAtLine(selectedDiagnostic.Context.FilePath,
                                   selectedDiagnostic.Context.LineNumber,
                                   selectedDiagnostic.Context.ColumnNumber,
                                   PreferredEditor.VsCode);
   }

   private void ExportToCsv_OnClick(object sender, RoutedEventArgs e)
   {
      if (ErrorLogDataGrid.ItemsSource is not List<Diagnostic> diagnostics)
         return;

      var csvContent = new StringBuilder();
      csvContent.AppendLine("Severity,Message,Action,DescriptionFilePath/LineNumber/ColumnNumber");

      foreach (var diagnostic in diagnostics)
         csvContent.AppendLine($"{diagnostic.Severity},{diagnostic.Descriptor.Message.Replace('\n', ';')}," +
                               $"{diagnostic.Action.Replace('\n', ';')},{diagnostic.Descriptor.Description.Replace('\n', ';')},{diagnostic.Context.ToErrorString.Replace('\n', ';')}");

      IO.WriteAllText(Path.Combine(IO.GetArcanumDataPath, "ErrorLog.csv"), csvContent.ToString(), Encoding.UTF8);
   }
}

public class SimpleSearchSettings : ISearchSettings
{
   public ISearchSettings.SearchModes SearchMode { get; set; } = ISearchSettings.SearchModes.Default;
   public ISearchSettings.SortingOptions SortingOption { get; set; } = ISearchSettings.SortingOptions.Acending;
   public int MaxLevinsteinDistance { get; set; } = 2;
}