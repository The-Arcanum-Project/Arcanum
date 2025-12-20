using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.PopUp;
using Common;
using Common.Logger;
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

   public string ErrorName
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "Error Name";

   public string ErrorMessage
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "Error Message";

   public string ErrorDescription
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "Error Description";

   public string FileProbe
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = string.Empty;

   public DiagnosticSeverity SelectedSeverity
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = DiagnosticSeverity.Error;

   public string SelectedPath
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = string.Empty;

   public bool DontProbeForFiles
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public int ModErrorCount
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public int VanillaErrorCount
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

   public int BaseModErrorCount
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   }

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
               Title = "Search AgsSettings", WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
         settingsPropWindow.ShowDialog();
         QuerySearch(SearchTextBox.SearchInputTextBox.Text);
      };

      DontProbeForFiles = !Config.Settings.ErrorLogOptions.ProbeFiles;
      _isFullyLoaded = true;

      var modErrors = 0;
      var vanillaErrors = 0;
      var baseModErrors = 0;

      foreach (var diagnostic in ErrorManager.Diagnostics)
         if (diagnostic.Context.FilePath.StartsWith(FileManager.GetVanillaPath()))
            vanillaErrors++;
         else if (diagnostic.Context.FilePath.StartsWith(FileManager.GetModPath()))
            modErrors++;
         else
            baseModErrors++;

      ModErrorCount = modErrors;
      VanillaErrorCount = vanillaErrors;
      BaseModErrorCount = baseModErrors;
   }

   private void QuerySearch(string query)
   {
      if (!_isFullyLoaded)
         return;

      if (ErrorLogDataGrid.ItemsSource is not ListCollectionView lcv)
         throw new InvalidOperationException("DataContext is not a CollectionView.");

      lcv.SortDescriptions.Clear();

      var baseFilter = SimpleCollectionViewFilterProvider.GenerateFilter(SearchSettings,
                                                                         query,
                                                                         FilterPropertyPath ?? string.Empty);

      var vanillaPath = FileManager.GetVanillaPath();
      var showVanilla = VanillaErrors.IsChecked == true;

      lcv.Filter = (item) =>
      {
         if (baseFilter != null! && !baseFilter(item))
            return false;

         if (item is Diagnostic diag)
         {
            var isVanillaFile = diag.Context.FilePath.StartsWith(vanillaPath, StringComparison.OrdinalIgnoreCase);

            if (isVanillaFile && !showVanilla)
               return false;
         }

         return true;
      };

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
         FileProbe = diagnostic.Descriptor.Resolution;
         SelectedPath = FileManager.SanitizePath(diagnostic.Context.FilePath);
         UpdateProbe(diagnostic);
      }
      else
      {
         ErrorName = "Error Name";
         ErrorMessage = "Error Message";
         ErrorDescription = "Error Description";
         FileProbe = string.Empty;
         SelectedPath = string.Empty;
      }

      if (ErrorLogDataGrid.SelectedItem is not Diagnostic selectedDiagnostic)
         return;

      SelectedSeverity = selectedDiagnostic.Severity;
   }

   public static Task<string[]> ReadLineRange(Diagnostic diagnostic)
   {
      return IO.ReadLineRange(diagnostic.Context.FilePath,
                              diagnostic.Context.LineNumber - 2,
                              diagnostic.Context.LineNumber + 2);
   }

   private async void UpdateProbe(Diagnostic diagnostic)
   {
      if (DontProbeForFiles)
         return;

      try
      {
         var lines = await ReadLineRange(diagnostic);

         for (var i = 0; i < lines.Length; i++)
            lines[i] = lines[i].Trim() + '\n';

         ProbeTextBlock.Inlines.Clear();

         var line1Run = new Run(lines[0]);
         var line2Run = new Run(lines[1]);
         var line3Run = new Run(lines[2]);
         line3Run.TextDecorations.Add(GetRedUnderlineDecoration());
         var line4Run = new Run(lines[3]);
         var line5Run = new Run(lines[4]);

         ProbeTextBlock.Inlines.Add(line1Run);
         ProbeTextBlock.Inlines.Add(line2Run);
         ProbeTextBlock.Inlines.Add(line3Run);
         ProbeTextBlock.Inlines.Add(line4Run);
         ProbeTextBlock.Inlines.Add(line5Run);
      }
      catch (Exception e)
      {
         ArcLog.Write("ERL", LogLevel.CRT, "Exception during probing of error file: {0}", e);
      }
   }

   private static TextDecorationCollection GetRedUnderlineDecoration()
   {
      var redPen = new Pen(Brushes.Red, 1);
      redPen.Freeze();
      var customUnderline = new TextDecoration
      {
         Location = TextDecorationLocation.Underline, Pen = redPen,
      };
      customUnderline.Freeze();

      var customDecorations = new TextDecorationCollection { customUnderline };
      customDecorations.Freeze();
      return customDecorations;
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

   private void ExportToCsv_OnClick(object sender, MouseButtonEventArgs e)
   {
      if (ErrorLogDataGrid.ItemsSource is not ListCollectionView view)
         return;

      var csvContent = new StringBuilder();

      var cols = Config.Settings.ErrorLogOptions.ColumnsToExport.Split(',')
                       .Select(x => x.Equals("*"))
                       .ToArray();

      // Add the header row based on selected columns
      if (cols[0])
         csvContent.Append("Severity,");
      if (cols[1])
         csvContent.Append("Message,");
      if (cols[2])
         csvContent.Append("Action,");
      if (cols[3])
         csvContent.Append("DescriptionFilePath/LineNumber/ColumnNumber,");
      csvContent.AppendLine();

      foreach (Diagnostic d in view)
      {
         if (cols[0])
            csvContent.Append($"{d.Severity},");
         if (cols[1])
            csvContent.Append($"{d.Message.Replace('\n', ';')},");
         if (cols[2])
            csvContent.Append($"{d.Action.Replace('\n', ';')},");
         if (cols[3])
            csvContent.Append($"{d.Description.Replace('\n', ';')},");
         csvContent.AppendLine();
      }

      var folder = string.IsNullOrWhiteSpace(Config.Settings.ErrorLogOptions.ExportFilePath)
                      ? IO.GetArcanumDataPath
                      : Config.Settings.ErrorLogOptions.ExportFilePath;
      var filePath = Path.Combine(folder, Config.Settings.ErrorLogOptions.ExportFileName);

      IO.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);

      if (e.ChangedButton == MouseButton.Right)
         ProcessHelper.OpenFile(filePath);
   }

   private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ChangedButton != MouseButton.Right)
         return;

      ExportToPdx();
   }

   private static void ExportToPdx()
   {
      Dictionary<string, int> diagnosticsPerFile = [];
      foreach (var diag in ErrorManager.Diagnostics)
      {
         var sanitizedPath = FileManager.SanitizePath(diag.Context.FilePath, '\\');
         foreach (var fileDescriptor in DescriptorDefinitions.FileDescriptors)
         {
            if (!sanitizedPath.StartsWith(fileDescriptor.FilePath))
               continue;

            if (!diagnosticsPerFile.TryAdd(fileDescriptor.FilePath, 1))
               diagnosticsPerFile[fileDescriptor.FilePath]++;
         }
      }

      var sb = new StringBuilder();
      sb.AppendLine("# Arcanum Exported Diagnostics");
      sb.AppendLine();
      sb.AppendLine($"# Exported on: {DateTime.Now}");
      sb.AppendLine($"# Total Diagnostics: {ErrorManager.Diagnostics.Count}");
      sb.AppendLine();
      // Parsed files and folders
      sb.AppendLine("# Parsed Files and Folders:");
      var fileDescriptors = DescriptorDefinitions.FileDescriptors.OrderBy(x => x.FilePath).ToList();
      var maxErrorsInFileIntLength = diagnosticsPerFile.Values.Count == 0
                                        ? 1
                                        : diagnosticsPerFile.Values.Max().ToString().Length;
      foreach (var descriptor in fileDescriptors)
      {
         var sanitizedPath = FileManager.SanitizePath(descriptor.FilePath);
         diagnosticsPerFile.TryGetValue(sanitizedPath, out var errorCount);
         sb.AppendLine($"- ({errorCount.ToString().PadLeft(maxErrorsInFileIntLength)}) | {sanitizedPath}");
      }

      sb.AppendLine();
      sb.AppendLine("Format:");
      sb.AppendLine("# Error Type: Error Name, ID: Error ID, Occurrences: Count, Severity: Severity");
      sb.AppendLine("--> Description: Error Description (replace the {x} with the 0 indexed argument from the line below to get the full error message)");
      sb.AppendLine("- FilePath (Line Number, Column Number) || Argument1 -|- Argument2 -|- ...");
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine();

      var diagnosticsByType = ErrorManager.Diagnostics
                                          .GroupBy(d => d.Descriptor.Name)
                                          .ToDictionary(g => g.Key, g => g.ToList());

      foreach (var kvp in diagnosticsByType)
      {
         sb.AppendLine($"# Error Type: {kvp.Key}, ID: {kvp.Value.First().Descriptor.Id}, Occurrences: {kvp.Value.Count}, Severity: {kvp.Value.First().Severity}");
         sb.AppendLine($"--> Description: {kvp.Value.First().Descriptor.Description.Replace("\n", "\n    ")}");
         sb.AppendLine();
         foreach (var diagnostic in kvp.Value)
            sb.AppendLine($"- {FileManager.SanitizePath(diagnostic.Context.FilePath)} (Line {diagnostic.Context.LineNumber}, Column {diagnostic.Context.ColumnNumber}) || {string.Join(" -|- ", diagnostic.Arguments)}");
         sb.AppendLine();
      }

      var folder = string.IsNullOrWhiteSpace(Config.Settings.ErrorLogOptions.ExportFilePath)
                      ? IO.GetArcanumDataPath
                      : Config.Settings.ErrorLogOptions.ExportFilePath;
      var filePath = Path.Combine(folder, "ExportedDiagnosticsPdx.txt");

      IO.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

      if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
         ProcessHelper.OpenFile(filePath);
      ErrorManager.ExportToFile();
   }

   private void VanillaFilter_OnToggle(object sender, RoutedEventArgs e)
   {
      // Simply re-run the search logic with the current text
      QuerySearch(SearchTextBox?.SearchInputTextBox?.Text ?? string.Empty);
   }

   private void ExportToCsv_LeftOnClick(object sender, RoutedEventArgs e)
   {
      ExportToCsv_OnClick(sender, new(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent, });
   }
}

public class SimpleSearchSettings : ISearchSettings
{
   public ISearchSettings.SearchModes SearchMode { get; set; } = ISearchSettings.SearchModes.Default;
   public ISearchSettings.SortingOptions SortingOption { get; set; } = ISearchSettings.SortingOptions.Acending;
   public int MaxLevinsteinDistance { get; set; } = 2;
}