using System.Windows;
using System.Windows.Input;
using Arcanum.Core.ApplicationContext;
using Arcanum.Core.ApplicationContext.Contexts.SpecializedEditors;
using Arcanum.UI.Commands.KeyMap;

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
                         _ => MessageBox.Show("Open Queastor command executed!")).WithDefaultGesture(Key.F, ModifierKeys.Control);

      // Map Commands
      new ManagedCommand(CommandIds.Editor.Map.RectangleSelectModifier,
                         "Rectangle Select Modifier",
                         "Hold to enable rectangle selection in the map editor.",
                         CommandScopes.EDITOR,
                         _ =>
                         {
                            /* This command is meant to be used as a modifier, so it doesn't execute an action on its own. */
                         }).WithDefaultGesture(Key.LeftShift);

      // Specialized Editors Commands
      new ManagedCommand(CommandIds.Editor.SpecializedEditors.PoliticalEditor.SyncWithSelection,
                         "Sync with Selection",
                         "Toggles whether the Political Editor should sync with the current selection.",
                         CommandScopes.POLITICAL_EDITOR,
                         _ => { ArcAppContext.Get<IPoliticalEditor>()?.ToggleSyncState(); }).WithDefaultGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt);
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

public class KeyGestureComparer : IEqualityComparer<InputGesture>
{
   public bool Equals(InputGesture? x, InputGesture? y)
   {
      if (x == null || y == null)
         return false;

      if (x is KeyGesture kgX && y is KeyGesture kgY)
         return kgX.Key == kgY.Key && kgX.Modifiers == kgY.Modifiers;
      if (x is MultiKeyGesture mkX && y is MultiKeyGesture mkY)
         return mkX.Equals(mkY);

      return false;
   }

   public int GetHashCode(InputGesture obj) => obj switch
   {
      KeyGesture kg => HashCode.Combine(kg.Key, kg.Modifiers),
      MultiKeyGesture mk => HashCode.Combine(mk.FirstGesture.Key, mk.FirstGesture.Modifiers, mk.SecondGesture.Key, mk.SecondGesture.Modifiers),
      _ => obj.GetHashCode(),
   };
}