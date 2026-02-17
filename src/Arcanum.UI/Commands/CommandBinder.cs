using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Arcanum.UI.Commands.KeyMap;

namespace Arcanum.UI.Commands;

public static class CommandBinder
{
   private const int CHORD_TIMEOUT_MS = 1500;
   private static KeyGesture? _pendingFirstStroke;
   private static DateTime _lastStrokeTime;

   public static readonly DependencyProperty ScopesProperty =
      DependencyProperty.RegisterAttached("Scopes",
                                          typeof(string),
                                          typeof(CommandBinder),
                                          new(null, OnScopesChanged));

   public static readonly DependencyProperty AssignProperty =
      DependencyProperty.RegisterAttached("Assign",
                                          typeof(IAppCommand),
                                          typeof(CommandBinder),
                                          new(null, OnAssignChanged));

   public static void SetScopes(DependencyObject obj, string value) => obj.SetValue(ScopesProperty, value);
   public static string GetScopes(DependencyObject obj) => (string)obj.GetValue(ScopesProperty);

   private static void OnScopesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is FrameworkElement element)
      {
         var updateAction = () => SynchronizeScopedBindings(element, GetScopes(element));

         CommandRegistry.BindingsChanged += updateAction;
         element.PreviewKeyDown += OnElementPreviewKeyDown;

         element.Unloaded += (_, _) =>
         {
            CommandRegistry.BindingsChanged -= updateAction;
            element.PreviewKeyDown -= OnElementPreviewKeyDown;
         };

         // Initial sync
         updateAction();
      }
   }

   private static bool IsModifierKey(Key key)
      => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.System;

   private static void OnElementPreviewKeyDown(object sender, KeyEventArgs e)
   {
      if (sender is not FrameworkElement element)
         return;

      var key = e.Key == Key.System ? e.SystemKey : e.Key;

      // We wait for the 'Action' key to be pressed.
      if (IsModifierKey(key))
         return;

      var currentStroke = new KeyGesture(key, Keyboard.Modifiers);

      var scopeString = GetScopes(element);
      if (string.IsNullOrEmpty(scopeString))
         return;

      var activeScopes = scopeString.Split(',').Select(s => s.Trim()).ToList();
      var commands = CommandRegistry.AllCommands
                                    .Where(c => activeScopes.Contains(c.Scope, StringComparer.OrdinalIgnoreCase))
                                    .ToList();

      if (_pendingFirstStroke != null)
      {
         if ((DateTime.Now - _lastStrokeTime).TotalMilliseconds > CHORD_TIMEOUT_MS)
            _pendingFirstStroke = null;
         else
         {
            var chordCommand = commands.FirstOrDefault(c => c.Gestures.OfType<MultiKeyGesture>()
                                                             .Any(g => IsMatch(g.FirstGesture, _pendingFirstStroke) &&
                                                                       IsMatch(g.SecondGesture, currentStroke)));

            if (chordCommand != null)
            {
               ExecuteCommand(chordCommand, element);
               e.Handled = true;
               _pendingFirstStroke = null;
               return;
            }
         }
      }

      var isChordStart = commands.Any(c => c.Gestures.OfType<MultiKeyGesture>()
                                            .Any(g => IsMatch(g.FirstGesture, currentStroke)));

      if (isChordStart)
      {
         _pendingFirstStroke = currentStroke;
         _lastStrokeTime = DateTime.Now;
         e.Handled = true;
      }
      else
         _pendingFirstStroke = null;
   }

   private static bool IsMatch(KeyGesture template, KeyGesture input) => template.Key == input.Key && template.Modifiers == input.Modifiers;

   private static void ExecuteCommand(IAppCommand command, FrameworkElement element)
   {
      // Try to find a Window for Dialog scopes, otherwise null
      object? parameter = element as Window ?? Window.GetWindow(element);
      if (command.CanExecute(parameter))
         command.Execute(parameter);
   }

   public static void SynchronizeScopedBindings(FrameworkElement element, string? scopeString)
   {
      element.InputBindings.Clear();
      if (string.IsNullOrWhiteSpace(scopeString))
         return;

      var activeScopes = scopeString.Split(',').Select(s => s.Trim()).ToList();

      // Filter for standard KeyGestures (WPF handles these)
      var filteredCommands = CommandRegistry.AllCommands
                                            .Where(c => activeScopes.Contains(c.Scope));

      foreach (var cmd in filteredCommands)
      {
         foreach (var gesture in cmd.Gestures)
            // Only add standard KeyGestures to InputBindings
            // Chords are handled by the PreviewKeyDown event above
            if (gesture is KeyGesture kg)
            {
               var binding = new InputBinding(cmd, kg);
               if (element is Window w)
                  binding.CommandParameter = w;
               element.InputBindings.Add(binding);
            }
      }
   }

   public static void SetAssign(DependencyObject obj, IAppCommand value) => obj.SetValue(AssignProperty, value);
   public static IAppCommand GetAssign(DependencyObject obj) => (IAppCommand)obj.GetValue(AssignProperty);

   private static void OnAssignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is ButtonBase button && e.NewValue is IAppCommand cmd)
      {
         // Set the Command
         button.Command = cmd;

         // Bind Content to DisplayName
         BindingOperations.SetBinding(button,
                                      ContentControl.ContentProperty,
                                      new Binding(nameof(IAppCommand.DisplayName)) { Source = cmd, Mode = BindingMode.OneWay });

         // Bind ToolTip to Tooltip
         BindingOperations.SetBinding(button,
                                      FrameworkElement.ToolTipProperty,
                                      new Binding(nameof(IAppCommand.Tooltip)) { Source = cmd, Mode = BindingMode.OneWay });

         // Handle CommandParameter automatically for Dialogs
         if (cmd.Scope.Contains(CommandScopes.DIALOG, StringComparison.OrdinalIgnoreCase))
            BindingOperations.SetBinding(button,
                                         ButtonBase.CommandParameterProperty,
                                         new Binding { RelativeSource = new(RelativeSourceMode.FindAncestor, typeof(Window), 1) });
      }
      else if (d is MenuItem menuItem && e.NewValue is IAppCommand menuCmd)
      {
         // Set the Command
         menuItem.Command = menuCmd;

         // Bind Header to DisplayName
         BindingOperations.SetBinding(menuItem,
                                      HeaderedItemsControl.HeaderProperty,
                                      new Binding(nameof(IAppCommand.DisplayName)) { Source = menuCmd, Mode = BindingMode.OneWay });

         // Bind ToolTip to Tooltip
         BindingOperations.SetBinding(menuItem,
                                      FrameworkElement.ToolTipProperty,
                                      new Binding(nameof(IAppCommand.Tooltip)) { Source = menuCmd, Mode = BindingMode.OneWay });
      }
   }
}