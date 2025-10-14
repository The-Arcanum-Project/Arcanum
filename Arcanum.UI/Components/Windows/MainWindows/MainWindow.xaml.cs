using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Arcanum.Core.CoreSystems.ConsoleServices;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.PerformanceCounters;
using Arcanum.UI.Components.UIHandles;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.HostUIServices.SettingsGUI;
using Arcanum.UI.NUI;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
using Common.UI;
using Application = System.Windows.Application;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class MainWindow : IPerformanceMeasured, INotifyPropertyChanged
{
   public const int DEFAULT_WIDTH = 1920;
   public const int DEFAULT_HEIGHT = 1080;

   private string _ramUsage = "RAM: [0 MB]";
   private string _cpuUsage = "CPU: [0%]";
   private string _gpuUsage = "GPU: [0%]";
   private string _vramUsage = "VRAM: [0 MB]";
   private string _fps = "FPS: [0]";

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

   public string GpuUsage
   {
      get => _gpuUsage;
      private set
      {
         if (value == _gpuUsage)
            return;

         _gpuUsage = value;
         OnPropertyChanged();
      }
   }

   public string VramUsage
   {
      get => _vramUsage;
      private set
      {
         if (value == _vramUsage)
            return;

         _vramUsage = value;
         OnPropertyChanged();
      }
   }

   public string Fps
   {
      get => _fps;
      private set
      {
         if (value == _fps)
            return;

         _fps = value;
         OnPropertyChanged();
      }
   }

   #endregion

   public MainWindow()
   {
      InitializeComponent();
      PerformanceCountersHelper.Initialize(this);
   }

   public void GoToArcanumMenuScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      UIHandle.Instance.MainWindowsHandle.TransferToMainMenuScreen(this, MainMenuScreen.MainMenuScreenView.Arcanum);
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

      // Load map if data ready
      if (DescriptorDefinitions.MapTracingDescriptor.LoadingService[0] is not LocationMapTracing mapDataParser)
         throw new ApplicationException("Could not load location map tracing descriptor.");

      lock (mapDataParser)
         if (mapDataParser.finishedTesselation)
            MainMap.SetupRenderingAsync(mapDataParser.polygons, mapDataParser.mapSize);

      Eu5UiGen.GenerateAndSetView(new(Globals.Locations.First().Value, true, UiPresenter));

      Selection.LocationSelectionChanged += SelectionOnLocationSelectionChanged;
   }

   private void SelectionOnLocationSelectionChanged(List<Location> locations)
   {
      Eu5UiGen.GenerateAndSetView(new([..locations], true, UiPresenter));
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

   public void SetGpuUsage(string gpuUsage)
   {
      if (string.Equals(gpuUsage, GpuUsage, StringComparison.Ordinal))
         return;

      GpuUsage = gpuUsage;
   }

   public void SetVramUsage(string vramUsage)
   {
      if (string.Equals(vramUsage, VramUsage, StringComparison.Ordinal))
         return;

      VramUsage = vramUsage;
   }

   public void SetFps(string fps)
   {
      if (string.Equals(fps, Fps, StringComparison.Ordinal))
         return;

      Fps = fps;
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

   private void ExecuteLexer(object sender, ExecutedRoutedEventArgs e)
   {
      var lexerWindow = new ParserTest();
      lexerWindow.ShowDialog();
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
      Eu5UiGen.GenerateAndSetView(new(Globals.Locations.ToList()[Random.Shared.Next(0, Globals.Locations.Count)].Value,
                                      true,
                                      UiPresenter));
   }

   private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
   {
      // Check which button was pressed
      if (e.ChangedButton == MouseButton.XButton1)
      {
         NUINavigation.Instance.Back();
         e.Handled = true;
      }
      else if (e.ChangedButton == MouseButton.XButton2)
      {
         NUINavigation.Instance.Forward();
         e.Handled = true;
      }
   }

   private void CanGoToPreviousINUICommand_Executed(object sender, CanExecuteRoutedEventArgs e)
      => e.CanExecute = NUINavigation.Instance.CanBack;

   private void GoToPreviousINUICommand_Executed(object sender, ExecutedRoutedEventArgs e)
      => NUINavigation.Instance.Back();

   private void CanGoToNextINUICommand_Executed(object sender, CanExecuteRoutedEventArgs e)
      => e.CanExecute = NUINavigation.Instance.CanForward;

   private void GoToNextINUICommand_Executed(object sender, ExecutedRoutedEventArgs e)
      => NUINavigation.Instance.Forward();

   private void ViewINUIObjectsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var nuiObjectView = new NUIObjectView();
      nuiObjectView.Show();
   }

   private void OpenDebugPanel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var debugPanel = new Debug_Panel();
      debugPanel.Show();
   }
}