using System.Windows.Input;
using Arcanum.Core.Globals;

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
   public static readonly RoutedCommand SaveAllModifiedObjectsCommand = new("SaveAllModifiedObjectsCommand", typeof(MainWindowView))
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
   
   public static readonly RoutedCommand OpenPluginSettingsCommand = new("OpenPluginSettingsCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenPluginSettings },
   };
   // File Reloading
   public static readonly RoutedCommand OpenReloadFolderWindowCommand = new("OpenReloadFolderWindowCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenReloadFolderWindow },
   };
   public static readonly RoutedCommand OpenReloadFileWindowCommand = new("OpenReloadFileWindowCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.OpenReloadFileWindow },
   };

}