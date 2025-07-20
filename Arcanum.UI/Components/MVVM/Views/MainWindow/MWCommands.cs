using System.Windows.Input;
using Arcanum.Core.Globals;

namespace Arcanum.UI.Components.MVVM.Views.MainWindow;

public static class MwCommands
{
   public static readonly RoutedCommand CloseProjectFileCommand = new("CloseProjectFileCommand", typeof(MainWindowView))
   {
      InputGestures = { Config.UserKeyBinds.MainWindowKeyBinds.CloseProject },
   };
   public static readonly RoutedCommand OpenProjectFileCommand = new("OpenProjectFileCommand", typeof(MainWindowView));
   public static readonly RoutedCommand NewProjectFileCommand = new("NewProjectFileCommand", typeof(MainWindowView));
   public static readonly RoutedCommand ExitArcanumCommand = new("ExitArcanumCommand", typeof(MainWindowView));
}