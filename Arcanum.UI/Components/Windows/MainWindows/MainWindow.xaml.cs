using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Arcanum.API.UI;
using Arcanum.Core.CoreSystems.ConsoleServices;
using Arcanum.Core.CoreSystems.Parsing.Steps;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Settings;
using Arcanum.Core.Settings.SmallSettingsObjects;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.PropertyHelpers;
using Arcanum.UI.Components.Views.MainWindow;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.HostUIServices.SettingsGUI;
using Common.UI;
using Common.Utils.PropertyUtils;
using Application = System.Windows.Application;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class MainWindow : IPerformanceMeasured, INotifyPropertyChanged
{
   public const int DEFAULT_WIDTH = 1920;
   public const int DEFAULT_HEIGHT = 1080;

   private readonly MainWindowView _view;
   private string _ramUsage = "RAM: [0 MB]";
   private string _cpuUsage = "CPU: [0%]";

   #region Properties

   public string RamUsage
   {
      get => _ramUsage;
      private set
      {
         if (value == _ramUsage)
            return;

         _ramUsage = value;
         OnPropertyChanged();
      }
   }
   public string CpuUsage
   {
      get => _cpuUsage;
      private set
      {
         if (value == _cpuUsage)
            return;

         _cpuUsage = value;
         OnPropertyChanged();
      }
   }

   #endregion

   public MainWindow()
   {
      InitializeComponent();
      PerformanceCountersHelper.Initialize(this);

      _view = DataContext as MainWindowView ??
              throw new InvalidOperationException("DataContext is not set or is not of type MainWindowView.");
   }

   public void GoToArcanumMenuScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      var mw = new MainMenuScreen { MainMenuViewModel = { TargetedView = MainMenuScreen.MainMenuScreenView.Arcanum } };
      Application.Current.MainWindow = mw;
      Application.Current.MainWindow.Show();
      mw.Activate();
      Close();
   }

   private void CommandCanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

   private void ExitArcanum_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      LifecycleManager.Instance.RunShutdownSequence();
      Application.Current.Shutdown();
   }

   private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
   {
      var interop = new WindowInteropHelper(this);
      var screen = Screen.FromHandle(interop.Handle);
      if (screen.Bounds.Height <= DEFAULT_HEIGHT || screen.Bounds.Width <= DEFAULT_WIDTH)
      {
         Height = screen.WorkingArea.Height * 0.8;
         Width = screen.WorkingArea.Width * 0.8;
         WindowState = WindowState.Maximized;
      }
   }

   private void OpenPluginSettingsWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var settingsWindow = new PluginSettingsWindow();
      settingsWindow.ShowDialog();
   }

   private void OpenSettingsWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      SettingsWindow.ShowSettingsWindow();
   }

   private void SaveAllModified_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual save logic
   }

   private void OpenSaveSelector_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual save selector logic
   }

   private void OpenReloadFileWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual reload file logic
   }

   private void OpenReloadFolderWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      // TODO @MelCo link this to the actual reload folder logic
   }

   public void SetCpuUsage(string cpuUsage)
   {
      if (string.Equals(cpuUsage, CpuUsage, StringComparison.Ordinal))
         return;

      CpuUsage = cpuUsage;
   }

   public void SetMemoryUsage(string memoryUsage)
   {
      if (string.Equals(memoryUsage, RamUsage, StringComparison.Ordinal))
         return;

      RamUsage = memoryUsage;
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

   private void OpenConsoleCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var consoleWindow = new ConsoleWindow(new ConsoleServiceImpl(LifecycleManager.Instance.PluginManager.Host,
                                                                   "DebugConsole",
                                                                   category: DefaultCommands.CommandCategory.All));
      consoleWindow.Show();
   }

   private void LoadingStepRunnerCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      new RunLoadingStep().ShowDialog();
   }

   private void OpenEffectWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var browser = DocsObjBrowser.ShowDocsObjBrowser(DocsObjBrowser.DocsObjBrowserType.Effects);
      browser.Title = "Effects Browser";
      browser.ShowDialog();
   }

   private void OpenModifierWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var browser = ModifierBrowser.ShowModifierBrowser();
      browser.Title = "Modifier Browser";
      browser.ShowDialog();
   }

   private void OpenTriggerWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var browser = DocsObjBrowser.ShowDocsObjBrowser(DocsObjBrowser.DocsObjBrowserType.Triggers);
      browser.Title = "Triggers Browser";
      browser.ShowDialog();
   }

   private void MenuItem_OnClick(object sender, RoutedEventArgs e)
   {
      var metadataPropGrid = new PropertyGridWindow(CoreData.ModMetadata);
      metadataPropGrid.ShowDialog();
   }

   private void OpenSearchWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      SearchWindow.ShowSearchWindow();
   }

   private void OpenHistoryWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var historyWindow = new HistoryTreeView();
      historyWindow.ShowDialog();
   }

   private void StepRedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      AppData.HistoryManager.Redo(true);
   }

   private void StepUndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      AppData.HistoryManager.Undo(true);
   }

   private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      AppData.HistoryManager.Redo(false);
   }

   private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      AppData.HistoryManager.Undo(false);
   }

   private void CommandCanStrepRedoExecute(object sender, CanExecuteRoutedEventArgs e)
   {
      e.CanExecute = AppData.HistoryManager.CanStepRedo;
   }

   private void CommandCanStepUndoExecute(object sender, CanExecuteRoutedEventArgs e)
   {
      e.CanExecute = AppData.HistoryManager.CanStepUndo;
   }

   private void OpenErrorLogWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var errorLogWindow = new ErrorLog();
      errorLogWindow.ShowDialog();
   }

   private void UIElementsBrowserCommandCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      new UIElementsBrowser().ShowDialog();
   }

   private void LoadLocationsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
   }

   private void GlobalsBrowserCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var globalsBrowser = new GameObjectBrowser();
      globalsBrowser.ShowDialog();
   }

   private void GCCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      GC.Collect();
      GC.WaitForPendingFinalizers();
   }

   private void OpenParsingStepBrowserCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var parsingStepBrowser = new ParsingViewer();
      parsingStepBrowser.ShowDialog();
   }

   private void TempTestingCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var path =
         PropertyPathBuilder.GetPathToProperty(Config.Settings,
                                               typeof(MainSettingsObj).GetProperty(nameof(ErrorLogExportOptions))!
                                                                      .PropertyType
                                                                      .GetProperty(nameof(ErrorLogExportOptions
                                                                             .ExportFilePath))!);
      UIHandle.Instance.PopUpHandle.NavigateToSetting(path);
   }
}