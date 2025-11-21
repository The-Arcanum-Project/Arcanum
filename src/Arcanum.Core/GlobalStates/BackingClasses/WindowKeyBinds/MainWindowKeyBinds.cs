using System.Windows.Input;
using Arcanum.API.Core.KeyBinds;

namespace Arcanum.Core.GlobalStates.BackingClasses.WindowKeyBinds;

public class MainWindowKeyBinds : KeyBindProvider
{
   // Menu Items
   public KeyGesture GoToPreviousINUI { get; set; } = new(Key.Left, ModifierKeys.Alt);
   public KeyGesture GoToNextINUI { get; set; } = new(Key.Right, ModifierKeys.Alt);

   // Project Management
   public KeyGesture CloseProject { get; set; } = new(Key.Escape, ModifierKeys.Control);
   public KeyGesture OpenProject { get; set; } = new(Key.O, ModifierKeys.Control);
   public KeyGesture NewProject { get; set; } = new(Key.N, ModifierKeys.Control | ModifierKeys.Shift);
   public KeyGesture ExitArcanum { get; set; } = new(Key.None);

   // Saving
   public KeyGesture SaveAllModifiedObjects { get; set; } = new(Key.S, ModifierKeys.Control);
   public KeyGesture OpenSaveSelector { get; set; } = new(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

   // AgsSettings
   public KeyGesture OpenSettings { get; set; } = new(Key.F1);
   public KeyGesture OpenPluginSettings { get; set; } = new(Key.F2);

   // File Reloading
   public KeyGesture OpenReloadFolderWindow { get; set; } = new(Key.R, ModifierKeys.Control | ModifierKeys.Shift);
   public KeyGesture OpenReloadFileWindow { get; set; } = new(Key.R, ModifierKeys.Control);

   // Console Commands
   public KeyGesture OpenConsoleCommand { get; set; } = new(Key.F8);

   // Wiki Commands
   public KeyGesture OpenEffectWiki { get; set; } = new(Key.None);
   public KeyGesture OpenTriggerWiki { get; set; } = new(Key.None);
   public KeyGesture OpenModifierWiki { get; set; } = new(Key.None);

   // Search Bindings
   public KeyGesture OpenSearchWindow { get; set; } = new(Key.F, ModifierKeys.Control);

   // History Bindings
   public KeyGesture OpenHistoryWindow { get; set; } = new(Key.H, ModifierKeys.Control);
   public KeyGesture UndoCommand { get; set; } = new(Key.Z, ModifierKeys.Control);
   public KeyGesture StepUndoCommand { get; set; } = new(Key.Z, ModifierKeys.Control | ModifierKeys.Shift);
   public KeyGesture RedoCommand { get; set; } = new(Key.Y, ModifierKeys.Control);
   public KeyGesture StepRedoCommand { get; set; } = new(Key.Y, ModifierKeys.Control | ModifierKeys.Shift);

   // Error Log Bindings
   public KeyGesture OpenErrorLogWindow { get; set; } = new(Key.F10);

   // Debug Bindings
   public KeyGesture OpenUIElementsBrowser { get; set; } = new(Key.F11);
   public KeyGesture OpenGlobalsBrowser { get; set; } = new(Key.F7);
   public KeyGesture OpenParsingStepBrowser { get; set; } = new(Key.F9);
   public KeyGesture TempTestingCommand { get; set; } = new(Key.F5);
   public KeyGesture ViewINUIObjects { get; set; } = new(Key.F6);

   // Map Mode Quick buttons
   public KeyGesture MapModeButton1 { get; set; } = new(Key.D1, ModifierKeys.Control);
   public KeyGesture MapModeButton2 { get; set; } = new(Key.D2, ModifierKeys.Control);
   public KeyGesture MapModeButton3 { get; set; } = new(Key.D3, ModifierKeys.Control);
   public KeyGesture MapModeButton4 { get; set; } = new(Key.D4, ModifierKeys.Control);
   public KeyGesture MapModeButton5 { get; set; } = new(Key.D5, ModifierKeys.Control);
   public KeyGesture MapModeButton6 { get; set; } = new(Key.D6, ModifierKeys.Control);
   public KeyGesture MapModeButton7 { get; set; } = new(Key.D7, ModifierKeys.Control);
   public KeyGesture MapModeButton8 { get; set; } = new(Key.D8, ModifierKeys.Control);
   public KeyGesture MapModeButton9 { get; set; } = new(Key.D9, ModifierKeys.Control);
   public KeyGesture MapModeButton10 { get; set; } = new(Key.D0, ModifierKeys.Control);
}