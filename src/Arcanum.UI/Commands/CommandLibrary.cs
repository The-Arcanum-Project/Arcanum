using System.Windows;
using System.Windows.Input;

namespace Arcanum.UI.Commands;

public static class CommandLibrary
{
   public static void Initialize()
   {
      new ManagedCommand(CommandIds.UI.Window.Close,
                         "Close",
                         "Closes the current window.",
                         "Window",
                         CommandScopes.DIALOG,
                         param => (param as Window)?.Close()).WithDefaultGesture(Key.Escape);
   }

   private static void WithDefaultGesture(this ManagedCommand cmd, Key key, ModifierKeys modifiers = ModifierKeys.None)
      => cmd.Gestures.Add(new KeyGesture(key, modifiers));
}