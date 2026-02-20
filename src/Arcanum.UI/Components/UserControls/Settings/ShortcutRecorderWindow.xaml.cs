using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.KeyMap;
using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.UserControls.Settings;

public partial class ShortcutRecorderWindow
{
   private readonly ShortcutRecorderViewModel _vm;

   public ShortcutRecorderWindow(IAppCommand command)
   {
      InitializeComponent();
      _vm = new()
      {
         CommandId = command.Id.Value,
         CommandName = command.DisplayName,
         CommandScope = command.Scope,
         CommandDescription = command.Description,
      };
      DataContext = _vm;

      PreviewKeyDown += OnRecorderKeyDown;
   }

   public ShortcutChord? Result { get; private set; }

   private void OnRecorderKeyDown(object sender, KeyEventArgs e)
   {
      e.Handled = true;
      var key = e.Key == Key.System ? e.SystemKey : e.Key;
      if (IsModifier(key))
         return;

      var stroke = new ShortcutStroke(key.ToString(), Keyboard.Modifiers.ToString());

      if (_vm.ActiveSlotIndex == 0)
      {
         _vm.FirstStroke = stroke;
         if (_vm.IsSecondStrokeEnabled)
            _vm.ActiveSlotIndex = 1;
      }
      else
         _vm.SecondStroke = stroke;
   }

   private void Input_MouseDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is FrameworkElement el && int.TryParse(el.Tag.ToString(), out var index))
         _vm.ActiveSlotIndex = index;
   }

   private static bool IsModifier(Key k)
      => k is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin;

   private void Ok_Click(object sender, RoutedEventArgs e)
   {
      if (_vm.FirstStroke != null)
      {
         Result = new(_vm.FirstStroke, _vm.SecondStroke);
         DialogResult = true;
      }
   }

   private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

   private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
   {
      if (_vm.FirstStroke != null)
         _vm.ActiveSlotIndex = 1;
   }
}