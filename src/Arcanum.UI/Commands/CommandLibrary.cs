using System.Windows;
using System.Windows.Input;
using Arcanum.Core.ApplicationContext;
using Arcanum.Core.ApplicationContext.Contexts.SpecializedEditors;
using Arcanum.UI.Commands.KeyMap;
using Arcanum.UI.Components.Windows.MainWindows;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;

namespace Arcanum.UI.Commands;

public static class CommandLibrary
{
   public static void Initialize()
   {
      // Window commands
      new ManagedCommand(CommandIds.UI.Window.Close,
                         "Close",
                         "Closes the current window.",
                         CommandScopes.DIALOG,
                         param =>
                         {
                            if (param is Window window)
                               window.Close();
                         }).WithDefaultGesture(Key.Escape);

      new ManagedCommand(CommandIds.UI.TestCommand,
                         "Test Command",
                         "This is a test command for demonstration purposes.",
                         CommandScopes.GLOBAL,
                         _ => MessageBox.Show("Test Command executed!")).WithDefaultGesture(Key.T, ModifierKeys.Control);

      new ManagedCommand(CommandIds.UI.Window.Maximize,
                         "Maximize",
                         "Maximizes the current window.",
                         CommandScopes.DIALOG,
                         param =>
                         {
                            if (param is Window window)
                               window.WindowState = WindowState.Maximized;
                         }).WithDefaultGesture(Key.F11);

      new ManagedCommand(CommandIds.UI.Window.Minimize,
                         "Minimize",
                         "Minimizes the current window.",
                         CommandScopes.DIALOG,
                         param =>
                         {
                            if (param is Window window)
                               window.WindowState = WindowState.Minimized;
                         }).WithDefaultGestures((Key.F9, ModifierKeys.None), (Key.B, ModifierKeys.Control));

      new ManagedCommand(CommandIds.UI.Window.Layout.Load,
                         "Load Layout",
                         "Loads a previously saved window layout.",
                         CommandScopes.DIALOG,
                         _ => MessageBox.Show("Load Layout command executed!")).WithDefaultGesture(Key.L, ModifierKeys.Control);

      new ManagedCommand(CommandIds.UI.Window.Layout.Save,
                         "Save Layout",
                         "Saves the current window layout for later use.",
                         CommandScopes.DIALOG,
                         _ => MessageBox.Show("Save Layout command executed!")).WithDefaultGesture(Key.S, ModifierKeys.Control);

      // Editor Commands
      new ManagedCommand(CommandIds.Editor.OpenQueastor,
                         "Open Queastor",
                         "Opens the Queastor editor.",
                         CommandScopes.EDITOR,
                         _ =>
                         {
                            var mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                            if (mw == null)
                               return;

                            mw.BringIntoView();
                            SearchWindow.ShowSearchWindow(mw.MainMap);
                         }).WithDefaultGesture(Key.F, ModifierKeys.Control);

      // Map Commands
      new ManagedCommand(CommandIds.Editor.Map.RectangleSelectModifier,
                         "Rectangle Select Modifier",
                         "Hold to enable rectangle selection in the map editor.",
                         CommandScopes.EDITOR,
                         _ =>
                         {
                            /* This command is meant to be used as a modifier, so it doesn't execute an action on its own. */
                         },
                         canExecute: _ => false).WithDefaultGesture(Key.LeftShift);

      // Specialized Editors Commands
      new ManagedCommand(CommandIds.Editor.SpecializedEditors.PoliticalEditor.SyncWithSelection,
                         "Sync with Selection",
                         "Toggles whether the Political Editor should sync with the current selection.",
                         CommandScopes.POLITICAL_EDITOR,
                         _ => { ArcAppContext.Get<IPoliticalEditor>()?.ToggleSyncState(); },
                         canExecute: _ => ArcAppContext.Has<IPoliticalEditor>()).WithDefaultGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt);

      #region Global Commands

      // Open Context Explorer
      new ManagedCommand(CommandIds.Global.OpenContextExplorerWindow,
                         "Open Context Explorer",
                         "Opens the Context Explorer for the currently active window, allowing you to see available commands based on the current UI context.",
                         CommandScopes.GLOBAL,
                         _ =>
                         {
                            var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                            if (activeWindow == null)
                               return;

                            new ContextExplorerWindow(activeWindow).Show();
                         }).WithDefaultGesture(Key.K, ModifierKeys.Control | ModifierKeys.Shift);

      // Open Control Explorer (TODO)
      new ManagedCommand(CommandIds.Global.OpenControlExplorer,
                         "Open Context Explorer Control",
                         "Opens the Context Explorer for the currently active control, allowing you to see available commands based on the current UI context.",
                         CommandScopes.GLOBAL,
                         _ =>
                         {
                            // TODO: Implement control-focused context explorer
                         }).WithDefaultGesture(Key.K, ModifierKeys.Control);

      // Open Help
      new ManagedCommand(CommandIds.Global.OpenHelp,
                         "Open Help",
                         "Opens the help documentation.",
                         CommandScopes.GLOBAL,
                         _ => { new HelpWindow().ShowDialog(); }).WithDefaultGesture(Key.F12);

      #endregion
   }

   extension(ManagedCommand cmd)
   {
      private void WithDefaultGesture(Key key, ModifierKeys modifiers = ModifierKeys.None)
      {
         var gesture = new KeyGesture(key, modifiers);
         cmd.Gestures.Add(gesture);
         cmd.AddDefaultGesture(gesture);
      }

      private void WithDefaultGestures(params (Key key, ModifierKeys modifiers)[] gestures)
      {
         foreach (var (key, modifiers) in gestures)
         {
            var gesture = new KeyGesture(key, modifiers);
            cmd.Gestures.Add(gesture);
            cmd.AddDefaultGesture(gesture);
         }
      }

      public ManagedCommand WithChord(Key k1, ModifierKeys m1, Key k2, ModifierKeys m2)
      {
         cmd.Gestures.Add(new MultiKeyGesture(k1, m1, k2, m2));
         return cmd;
      }
   }
}