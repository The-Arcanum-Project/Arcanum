using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arcanum.UI.Components.StyleClasses;

/// <summary>
///    Behaves like a text block, but with a command. When the user clicks on it, it executes the command.
///    It also has a hover effect to indicate that it's clickable.
/// </summary>
public class CommandTextBlock : TextBlock
{
   public static readonly DependencyProperty CommandParameterProperty =
      DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandTextBlock), new(default(object)));

   public static readonly DependencyProperty CommandProperty =
      DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandTextBlock), new(default(ICommand)));

   public object CommandParameter
   {
      get => GetValue(CommandParameterProperty);
      set => SetValue(CommandParameterProperty, value);
   }

   public ICommand? Command
   {
      get => (ICommand)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
   }

   protected override void OnMouseDown(MouseButtonEventArgs e)
   {
      base.OnMouseDown(e);
      if (e.ChangedButton != MouseButton.Left)
         return;

      if (Command != null && Command.CanExecute(CommandParameter))
      {
         Command.Execute(CommandParameter);
         e.Handled = true;
      }
   }
}