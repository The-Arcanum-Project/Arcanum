using System.Windows.Input;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Views.MainWindow;

public static class MwCommands
{
   public static readonly RoutedCommand CloseProjectFileCommand = new("CloseProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.CloseProject },
   };

   public static readonly RoutedCommand OpenProjectFileCommand = new("OpenProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenProject },
   };

   public static readonly RoutedCommand NewProjectFileCommand = new("NewProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.NewProject },
   };

   public static readonly RoutedCommand ExitArcanumCommand = new("ExitArcanumCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.ExitArcanum },
   };

   public static readonly RoutedCommand SaveAllModifiedObjectsCommand =
      new("SaveAllModifiedObjectsCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.SaveAllModifiedObjects },
      };

   public static readonly RoutedCommand OpenSaveSelectorCommand = new("OpenSaveSelectorCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenSaveSelector },
   };

   public static readonly RoutedCommand OpenSettingsCommand = new("OpenSettingsCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenSettings },
   };

   public static readonly RoutedCommand OpenPluginSettingsCommand =
      new("OpenPluginSettingsCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenPluginSettings },
      };

   // File Reloading
   public static readonly RoutedCommand OpenReloadFolderWindowCommand =
      new("OpenReloadFolderWindowCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenReloadFolderWindow },
      };

   public static readonly RoutedCommand OpenReloadFileWindowCommand =
      new("OpenReloadFileWindowCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenReloadFileWindow },
      };

   public static readonly RoutedCommand OpenConsoleCommand = new("OpenConsoleCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenConsoleCommand },
   };

   public static readonly RoutedCommand DebugParsingCommand = new("DebugParsingCommand", typeof(MainWindowView))
   {
      InputGestures = { new KeyGesture(Key.F12) },
   };

   // Wiki Commands
   public static readonly RoutedCommand OpenEffectWikiCommand = new("OpenEffectWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenEffectWiki },
   };

   public static readonly RoutedCommand OpenTriggerWikiCommand = new("OpenTriggerWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenTriggerWiki },
   };

   public static readonly RoutedCommand OpenModifierWikiCommand = new("OpenModifierWikiCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenModifierWiki },
   };

   // Search Commands
   public static readonly RoutedCommand OpenSearchWindowCommand = new("OpenSearchCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenSearchWindow },
   };

   // History Commands
   public static readonly RoutedCommand OpenHistoryWindowCommand = new("OpenHistoryWindowCommand",
                                                                       typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenHistoryWindow },
   };

   public static readonly RoutedCommand UndoCommand = new("UndoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.UndoCommand },
   };

   public static readonly RoutedCommand StepUndoCommand = new("StepUndoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.StepUndoCommand },
   };

   public static readonly RoutedCommand RedoCommand = new("RedoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.RedoCommand },
   };

   public static readonly RoutedCommand StepRedoCommand = new("StepRedoCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.StepRedoCommand },
   };

   // Error Log Commands
   public static readonly RoutedCommand OpenErrorLogWindowCommand =
      new("OpenErrorLogWindowCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenErrorLogWindow },
      };

   // UIElementsBrowserCommand
   public static readonly RoutedCommand OpenUIElementsBrowserCommand =
      new("OpenUIElementsBrowserCommand", typeof(MainWindowView))
      {
         InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenUIElementsBrowser },
      };

   // Loading Commands
   public static readonly RoutedCommand LoadLocationsCommand = new("OpenLoadingWindowCommand",
                                                                   typeof(MainWindowView))
   {
      InputGestures = { new KeyGesture(Key.L, ModifierKeys.Control) },
   };
}