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
   public KeyGesture ExitArcanum { get; set; } = new(Key.None);
   
   // Saving
   public KeyGesture SaveAllModifiedObjects { get; set; } = new(Key.S, ModifierKeys.Control);
   public KeyGesture OpenSaveSelector { get; set; } = new(Key.S, ModifierKeys.Control | ModifierKeys.Shift);
   
   // Settings
   public KeyGesture OpenSettings { get; set; } = new(Key.F1);
   public KeyGesture OpenPluginSettings { get; set; } = new(Key.F2);
   
   // File Reloading
   public KeyGesture OpenReloadFolderWindow { get; set; } = new(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
   public KeyGesture OpenReloadFileWindow { get; set; } = new(Key.R, ModifierKeys.Control);
   
   // Console Commands
   public KeyGesture OpenConsoleCommand { get; set; } = new(Key.F8);
}