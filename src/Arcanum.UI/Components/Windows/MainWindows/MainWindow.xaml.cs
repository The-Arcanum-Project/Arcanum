using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.Clipboard;
using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Settings.BaseClasses;
using Arcanum.Core.Settings.SmallSettingsObjects;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.PerformanceCounters;
using Arcanum.Core.Utils.ScreenManagement;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.Map;
using Arcanum.UI.Components.Views.MainWindow;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.Components.Windows.MainWindows.MainWindowsHelpers;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.HostUIServices.SettingsGUI;
using Arcanum.UI.NUI.Generator.SpecificGenerators;
using Arcanum.UI.Themes;
using Arcanum.UI.Util;
using Arcanum.UI.Util.WindowManagement;
using Common;
using Common.Logger;
using Common.UI;
using Common.UI.MBox;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using NUINavigation = Arcanum.UI.NUI.NUINavigation;

namespace Arcanum.UI.Components.Windows.MainWindows;

public sealed partial class MainWindow : IPerformanceMeasured, INotifyPropertyChanged
{
   private const string HTTPS_EU5_PARADOXWIKIS_COM_ARCANUM = "https://eu5.paradoxwikis.com/Arcanum";
   private const int DEFAULT_WIDTH = 1920;
   private const int DEFAULT_HEIGHT = 1080;

   private readonly ToolTipManager _toolTipManager = new();

   #region Properties

   public string RamUsage
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "RAM: [0 MB]";

   public string CpuUsage
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "CPU: [0%]";

   public string GpuUsage
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "GPU: [0%]";

   public string VramUsage
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "VRAM: [0 MB]";

   public string Fps
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "FPS: [0]";

   public string HoveredLocation
   {
      get;
      private set
      {
         if (value == field)
            return;

         field = value.PadLeft(30);
         OnPropertyChanged();
      }
   } = null!;

   public string RectangleBounds
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = null!;

   public ImageSource? CurrentActionIcon
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = null;

   public string CurrentActionText
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = null!;

   public string ClipboardText
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = null!;

   #endregion

   #region Fields

   private readonly CurrentActionVisualizer _cav;

   #endregion

   public MainWindow()
   {
      InitializeComponent();
#if !DEBUG
      DebugPanel.Visibility = Visibility.Collapsed;
      MainGrid.ColumnDefinitions.RemoveAt(6);
      MainGrid.ColumnDefinitions.RemoveAt(5);
      DebugPanelGridSplitter.Visibility = Visibility.Collapsed;

#else
      DebugPanel.Visibility = Visibility.Visible;
#endif
      MainWindowGen.Initialize(SpecializedEditorPresenter);

      PerformanceCountersHelper.Initialize(this);
      UIHandle.Instance.MapInterface = new MapInterfaceImpl { MapControl = MainMap };

      Title = AppData.ProductName;
      VersionNumber = $"v{AppData.AppVersion}";
      _cav = new(this);
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
      var screen = ScreenManager.MainScreen;
      if (screen.Bounds.Height <= DEFAULT_HEIGHT || screen.Bounds.Width <= DEFAULT_WIDTH)
      {
         Height = screen.WorkingArea.Height * 0.8;
         Width = screen.WorkingArea.Width * 0.8;
         WindowState = WindowState.Maximized;
      }

      this.SetScreen(screen);

      // Load map if data ready
      if (DescriptorDefinitions.MapTracingDescriptor.LoadingService[0] is not LocationMapTracing mapDataParser)
         throw new ApplicationException("Could not load location map tracing descriptor.");

      lock (mapDataParser)
         if (mapDataParser.FinishedTesselation)
         {
            Debug.Assert(mapDataParser.Polygons != null,
                         "Map data parser has finished tesselation but polygons are null.");
            _ = MainMap.SetupRenderer(mapDataParser.Polygons!, mapDataParser.MapSize);
            MapModeManager.IsMapReady = true;
         }

      // Eu5UiGen.GenerateAndSetView(new(Globals.Locations.First().Value, true, UiPresenter));

      SelectionManager.EditableObjects.CollectionChanged += EditableObjectsOnCollectionChanged;
      GenerateMapModeButtons();

      MapControl.OnMapLoaded += () =>
      {
         var size = ((LocationMapTracing)DescriptorDefinitions.MapTracingDescriptor
                                                              .LoadingService[0]).MapSize;
         Selection.MapManager.InitializeMapData(new(0, 0, size.Item1, size.Item2));

         SettingsEventManager.RegisterSettingsHandler(nameof(MapSettingsObj.FrozenSelectionColorOpacity), (_, _) => MainMap.RefreshAndRenderSelectionColors());
         SettingsEventManager.RegisterSettingsHandler(nameof(MapSettingsObj.SelectionColorOpacity), (_, _) => MainMap.RefreshAndRenderSelectionColors());
         SettingsEventManager.RegisterSettingsHandler(nameof(MapSettingsObj.PrviewOpacityFactor), (_, _) => MainMap.RefreshAndRenderSelectionColors());
      };

      Selection.RectangleSelectionUpdated += _ =>
      {
         var rect = Selection.DragArea;
         RectangleBounds = $"Rect: [X:{rect.X}, Y:{rect.Y}, W:{rect.Width}, H:{rect.Height}]";
      };

      GcWizard.ForceGc();

      Selection.LocationHovered += locations =>
      {
         switch (locations.Count)
         {
            case 0:
               HoveredLocation = "No Location";
               return;
            case 1:
               HoveredLocation = locations[0].UniqueId;
               return;
            default:
               HoveredLocation = $"{locations.Count} Locations";
               break;
         }
      };

      lock (mapDataParser)
         SetUpToolTip(MainMap);

      SelectionManager.PropertyChanged += SelectionManagerOnPropertyChanged;

      SetupMapModeLogic();
      SetupClipboardLogic();
   }

   private void SetupClipboardLogic()
   {
      ArcClipboard.OnCopyAction += payload =>
      {
         ClipboardText = payload.Property == null
                            ? $"Clipboard: {payload.Value.GetType().Name} ({((IEu5Object)payload.Value).UniqueId})"
                            : $"Clipboard: {payload.Property} of {payload.Value.GetType().Name} ({((IEu5Object)payload.Value).UniqueId})";
      };
   }

   private void SetupMapModeLogic()
   {
      MapModeManager.OnMapModeChanged += MapModeManagerOnOnMapModeChanged;
      EventDistributor.ObjectOfTypeModified += UpdateMap;

      MainMap.OnMapClick += MainMapOnOnMapClick;
   }

   private static void MainMapOnOnMapClick(MapClickEventArgs e)
   {
      if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
      {
         if (e.MouseButton == MouseButton.Right)
         {
            if (!Selection.GetLocation(e.ClickPosition, out var location))
               return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
               var objs = SelectionManager.GetInferredObjectsForLocations([location], MapModeManager.GetCurrent().DisplayTypes[0]);
               if (objs is { Count: > 0 })
                  ArcClipboard.Copy(objs[0]);
            }
            else
               ArcClipboard.Copy(location);
         }
      }
   }

   private void MapModeManagerOnOnMapModeChanged(MapModeManager.MapModeType _)
   {
      Debug.Assert(MainMap != null, "MainMap is null in MapModeManagerOnOnMapModeChanged");
      if (!MapModeManager.IsMapReady)
         return;

      // ReSharper disable twice InconsistentlySynchronizedField
      MapModeManager.RenderCurrent(MainMap!.CurrentBackgroundColors);
      MainMap.UpdateColors();
   }

   private void UpdateMap(Type arg1, Enum arg2, IEu5Object[] objects)
   {
      if (objects.Length == 0)
         return;

      if (MapModeManager.GetCurrent().DisplayTypes.Contains(objects[0].GetType()))
      {
         MapModeManager.RenderCurrent(MainMap!.CurrentBackgroundColors);
         MainMap.UpdateColors();
      }
   }

   private void SelectionManagerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if (e.PropertyName == nameof(SelectionManager.ObjectSelectionMode))
         SelectionModeBox.SelectedIndex = (int)SelectionManager.ObjectSelectionMode;
   }

   private void EditableObjectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      var items = SelectionManager.EditableObjects;
      if (items.Count == 0)
      {
         UiPresenter.Content = null;
         return;
      }

      MainWindowGen.GenerateAndSetView(new(items.ToList(), true, UiPresenter, true));
   }

   private void SetUpToolTip(MapControl mainMap)
   {
      _toolTipManager.SetUpToolTip(mainMap);
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

   private async void SaveAllModified_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      try
      {
         var splash = new SavingSplashScreen { Owner = this };
         splash.Show();

         try
         {
            await Task.Run(() =>
            {
               SaveMaster.SaveAll(splash.UpdateProgress);
               return Task.CompletedTask;
            });
         }
         finally
         {
            splash.MarkAsComplete();
         }
      }
      catch (Exception ex)
      {
         ArcLog.WriteLine("MW", LogLevel.ERR, "Error while saving all modified files: " + ex);
         MBox.Show("An error occurred while saving all modified files:\n" + ex.Message,
                   "Error",
                   MBoxButton.OK,
                   MessageBoxImage.Error);
      }
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

   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   private void OpenConsoleCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var console = LifecycleManager.Instance.PluginManager.Host.GetService<IConsoleService>();
      var consoleWindow = new ConsoleWindow(console);
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
      SearchWindow.ShowSearchWindow(MainMap);
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
      MainWindowGen.GenerateAndSetView(new(Globals.Locations.ToList()[Random.Shared.Next(0, Globals.Locations.Count)]
                                                  .Value,
                                           true,
                                           UiPresenter,
                                           true));
   }

   private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
   {
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

   #region MapMode Buttons

   public void GenerateMapModeButtons()
   {
      MapModeButtonGrid.Children.Clear();
      MapModeButtonGrid.ColumnDefinitions.Clear();
      MapModeButton.CommandToMapModeType.Clear();
      for (var i = CommandBindings.Count; i-- > 0;)
      {
         var binding = CommandBindings[i];
         if (MapModeButton.CommandToMapModeType.ContainsKey(binding.Command))
            CommandBindings.RemoveAt(i);
      }

      for (var i = 0; i < Config.Settings.MapModeConfig.NumOfMapModeButtons; i++)
      {
         MapModeButtonGrid.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
         var mapMode = MapModeManager.GetMapModeForButtonIndex(i);
         var routedCommand = GetMapModeButtonCommand(i);
         if (routedCommand != null && mapMode != null)
         {
            MapModeButton.CommandToMapModeType[routedCommand] = mapMode.Type;
            var commandBinding =
               new CommandBinding(routedCommand, OnMapModeCommandExecuted, OnMapModeCommandCanExecute);
            CommandBindings.Add(commandBinding);
         }

         var button = new MapModeButton
         {
            Margin = new(2),
            Padding = new(0),
            Command = routedCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 30,
            ToolTip = mapMode != null
                         ? mapMode.Description +
                           "\nRMB to assign a new map mode.\nShortcut: Ctrl + " +
                           (i != 9 ? i + 1 : 0)
                         : "No map mode assigned to this button.\nRMB to assign a new map mode.\nShortcut: Ctrl + " +
                           (i != 9 ? i + 1 : 0),
            MapModeType = mapMode?.Type ?? MapModeManager.MapModeType.Locations,
            BorderThickness = new(1),
            FontSize = 7,
         };
         button.SetValue(Grid.ColumnProperty, i);
         MapModeButtonGrid.Children.Add(button);
      }
   }

   private void OnMapModeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      if (MapModeButton.CommandToMapModeType.TryGetValue(e.Command, out var mapModeType))
         MapModeManager.SetMapMode(mapModeType);
   }

   private void OnMapModeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
   {
      e.CanExecute = MapModeButton.CommandToMapModeType.ContainsKey(e.Command);
   }

   private RoutedCommand? GetMapModeButtonCommand(int index)
   {
      return index switch
      {
         0 => MwCommands.MapModeButton1Command,
         1 => MwCommands.MapModeButton2Command,
         2 => MwCommands.MapModeButton3Command,
         3 => MwCommands.MapModeButton4Command,
         4 => MwCommands.MapModeButton5Command,
         5 => MwCommands.MapModeButton6Command,
         6 => MwCommands.MapModeButton7Command,
         7 => MwCommands.MapModeButton8Command,
         8 => MwCommands.MapModeButton9Command,
         9 => MwCommands.MapModeButton10Command,
         _ => null,
      };
   }

   #endregion

   private void CanGoToPreviousINUICommand_Executed(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = NUINavigation.Instance.CanBack;

   private void GoToPreviousINUICommand_Executed(object sender, ExecutedRoutedEventArgs e) => NUINavigation.Instance.Back();

   private void CanGoToNextINUICommand_Executed(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = NUINavigation.Instance.CanForward;

   private void GoToNextINUICommand_Executed(object sender, ExecutedRoutedEventArgs e) => NUINavigation.Instance.Forward();

   private void ViewINUIObjectsCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var nuiObjectView = new NUIObjectView();
      nuiObjectView.Show();
   }

   private void OpenHistoryWindow_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      WindowManager.OpenWindow<HistoryTreeView>();
   }

   private void OpenDebugPanel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      var debugPanel = new Debug_Panel();
      debugPanel.Show();
   }

   private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
   {
      if (SaveMaster.GetNeedsToBeSaveCount > 0)
      {
         var result = MBox.Show("There are unsaved changes. Do you really want to close without saving?",
                                "Unsaved Changes",
                                MBoxButton.OKCancel,
                                MessageBoxImage.Warning);
         if (result == MBoxResult.Cancel)
         {
            e.Cancel = true;
            return;
         }
      }

      UIHandle.Instance.LogWindowHandle.CloseWindow();
      Application.Current.Dispatcher.Invoke(() =>
      {
         foreach (Window window in Application.Current.Windows)
            if (window != this)
               window.Close();
      });

      PerformanceCountersHelper.Shutdown();
      Application.Current.Dispatcher.InvokeShutdown();
   }

   private void FreezeSelection_Command(object sender, ExecutedRoutedEventArgs e)
   {
      SelectionManager.ToggleFreeze();
   }

   private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      new ClipboardHistory().Show();
   }

   private void OpenArcanumWikiCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      ProcessHelper.OpenLink(HTTPS_EU5_PARADOXWIKIS_COM_ARCANUM);
   }

   private void SelectWastelands_CheckChanged(object sender, RoutedEventArgs e)
   {
      if (sender is not CheckBox selectWastelands)
         return;

      SelectionManager.SelectWasteland = selectWastelands.IsChecked ?? true;
   }

   private void SelectWater_CheckChanged(object sender, RoutedEventArgs e)
   {
      if (sender is not CheckBox selectWater)
         return;

      SelectionManager.SelectWater = selectWater.IsChecked ?? true;
   }
}