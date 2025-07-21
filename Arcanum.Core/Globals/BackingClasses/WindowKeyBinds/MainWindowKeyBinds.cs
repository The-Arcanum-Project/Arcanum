using System.Windows.Input;
using Arcanum.API.Core.KeyBinds;

namespace Arcanum.Core.Globals.BackingClasses.WindowKeyBinds;

public class MainWindowKeyBinds : KeyBindProvider
{
   // Only used for serialization purposes.
   public MainWindowKeyBinds()
   {
   }

   public KeyGesture CloseProject { get; set; } = new(Key.Escape, ModifierKeys.Control);
   public KeyGesture OpenProject { get; set; } = new(Key.O, ModifierKeys.Control);
   public KeyGesture NewProject { get; set; } = new(Key.N, ModifierKeys.Control | ModifierKeys.Shift);
}