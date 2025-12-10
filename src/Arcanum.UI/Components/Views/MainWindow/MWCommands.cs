using System.Windows.Input;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Views.MainWindow;

public static class MwCommands
{
   public static readonly RoutedCommand CloseProjectFileCommand = new("CloseProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.CloseProject },
   };

   public static readonly RoutedCommand OpenProjectFileCommand = new("OpenProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenProject },
   };

   public static readonly RoutedCommand NewProjectFileCommand = new("NewProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.NewProject },
   };

   public static readonly RoutedCommand ExitArcanumCommand = new("ExitArcanumCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.ExitArcanum },
   };

   public static readonly RoutedCommand SaveAllModifiedObjectsCommand =
      new("SaveAllModifiedObjectsCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.SaveAllModifiedObjects },
      };

   public static readonly RoutedCommand OpenSaveSelectorCommand = new("OpenSaveSelectorCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenSaveSelector },
   };

   public static readonly RoutedCommand OpenSettingsCommand = new("OpenSettingsCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenSettings },
   };

   public static readonly RoutedCommand OpenPluginSettingsCommand =
      new("OpenPluginSettingsCommand", typeof(MainWindowView)) { InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenPluginSettings }, };

   // File Reloading
   public static readonly RoutedCommand OpenReloadFolderWindowCommand =
      new("OpenReloadFolderWindowCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenReloadFolderWindow },
      };

   public static readonly RoutedCommand OpenReloadFileWindowCommand =
      new("OpenReloadFileWindowCommand", typeof(MainWindowView)) { InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenReloadFileWindow }, };

   public static readonly RoutedCommand OpenConsoleCommand = new("OpenConsoleCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenConsoleCommand },
   };

   public static readonly RoutedCommand LoadingStepRunnerCommand =
      new("LoadingStepRunnerCommand", typeof(MainWindowView)) { InputGestures = { new KeyGesture(Key.F12) }, };

   // Wiki Commands
   public static readonly RoutedCommand OpenEffectWikiCommand = new("OpenEffectWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenEffectWiki },
   };

   public static readonly RoutedCommand OpenTriggerWikiCommand = new("OpenTriggerWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenTriggerWiki },
   };

   public static readonly RoutedCommand OpenModifierWikiCommand = new("OpenModifierWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenModifierWiki },
   };

   // Search Commands
   public static readonly RoutedCommand OpenSearchWindowCommand = new("OpenSearchCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenSearchWindow },
   };

   // History Commands
   public static readonly RoutedCommand OpenHistoryWindowCommand = new("OpenHistoryWindowCommand",
                                                                       typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenHistoryWindow },
   };

   public static readonly RoutedCommand UndoCommand = new("UndoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.UndoCommand },
   };

   public static readonly RoutedCommand StepUndoCommand = new("StepUndoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.StepUndoCommand },
   };

   public static readonly RoutedCommand RedoCommand = new("RedoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.RedoCommand },
   };

   public static readonly RoutedCommand StepRedoCommand = new("StepRedoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.StepRedoCommand },
   };

   // Error Log Commands
   public static readonly RoutedCommand OpenErrorLogWindowCommand =
      new("OpenErrorLogWindowCommand", typeof(MainWindowView)) { InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenErrorLogWindow }, };

   // UIElementsBrowserCommand
   public static readonly RoutedCommand OpenUIElementsBrowserCommand =
      new("OpenUIElementsBrowserCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenUIElementsBrowser },
      };

   // Debug Commands
   public static readonly RoutedCommand ExecuteLexer = new("OpenLoadingWindowCommand",
                                                           typeof(MainWindowView)) { InputGestures = { new KeyGesture(Key.L, ModifierKeys.Control) }, };

   public static readonly RoutedCommand OpenDebugPanel = new("OpenDebugPanel",
                                                             typeof(MainWindowView))
   {
      InputGestures = { new KeyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift) },
   };

   public static readonly RoutedCommand OpenGlobalsBrowserCommand = new("OpenGlobalsBrowserCommand",
                                                                        typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenGlobalsBrowser },
   };

   public static readonly RoutedCommand GCCommand = new("OpenModMetadataCommand",
                                                        typeof(MainWindowView)) { InputGestures = { new KeyGesture(Key.G, ModifierKeys.Control) }, };

   public static readonly RoutedCommand OpenParsingStepBrowserCommand = new("OpenParsingStepBrowserCommand",
                                                                            typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenParsingStepBrowser },
   };

   public static readonly RoutedCommand TempTestingCommand = new("TempTestingCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.TempTestingCommand },
   };

   public static readonly RoutedCommand GoToPreviousINUICommand = new("GoToPreviousINUICommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.GoToPreviousINUI },
   };

   public static readonly RoutedCommand GoToNextINUICommand = new("GoToNextINUICommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.GoToNextINUI },
   };

   public static readonly RoutedCommand ViewINUIObjectsCommand = new("ViewINUIObjectsCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.ViewINUIObjects },
   };

   // Map Mode quick access buttons

   public static readonly RoutedCommand MapModeButton1Command = new("MapModeButton_1_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton1 },
   };

   public static readonly RoutedCommand MapModeButton2Command = new("MapModeButton_2_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton2 },
   };

   public static readonly RoutedCommand MapModeButton3Command = new("MapModeButton_3_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton3 },
   };

   public static readonly RoutedCommand MapModeButton4Command = new("MapModeButton_4_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton4 },
   };

   public static readonly RoutedCommand MapModeButton5Command = new("MapModeButton_5_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton5 },
   };

   public static readonly RoutedCommand MapModeButton6Command = new("MapModeButton_6_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton6 },
   };

   public static readonly RoutedCommand MapModeButton7Command = new("MapModeButton_7_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton7 },
   };

   public static readonly RoutedCommand MapModeButton8Command = new("MapModeButton_8_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton8 },
   };

   public static readonly RoutedCommand MapModeButton9Command = new("MapModeButton_9_Command", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton9 },
   };

   public static readonly RoutedCommand MapModeButton10Command =
      new("MapModeButton_10_Command", typeof(MainWindowView)) { InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.MapModeButton10 }, };

   public static readonly RoutedCommand FreezeSelectionCommand = new("FreezeSelectionCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.FreezeSelection },
   };

   // Clipboard Commands
   public static readonly RoutedCommand HistoryClipboardCommand = new("HistoryClipboard", typeof(MainWindowView))
   {
      InputGestures = { Config.Settings.UserKeyBinds.MainWindowKeyBinds.OpenClipboardHistory },
   };
}