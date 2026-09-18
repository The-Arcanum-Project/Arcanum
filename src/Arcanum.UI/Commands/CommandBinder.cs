#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Arcanum.UI.Commands.KeyMap;
using Arcanum.UI.Components.Converters;

#endregion

namespace Arcanum.UI.Commands;

public static class CommandBinder
{
   private const int CHORD_TIMEOUT_MS = 1500;
   private static KeyGesture? _pendingFirstStroke;
   private static DateTime _lastStrokeTime;

   public static readonly DependencyProperty ScopeProperty =
      DependencyProperty.RegisterAttached("Scope",
                                          typeof(string),
                                          typeof(CommandBinder),
                                          new(null, OnScopeChanged));

   public static readonly DependencyProperty ScopesProperty =
      DependencyProperty.RegisterAttached("Scopes",
                                          typeof(string[]),
                                          typeof(CommandBinder),
                                          new(null, OnScopesChanged));

   public static readonly DependencyProperty AssignProperty =
      DependencyProperty.RegisterAttached("Assign",
                                          typeof(IAppCommand),
                                          typeof(CommandBinder),
                                          new(null, OnAssignChanged));

   private static readonly DependencyProperty OriginalToolTipProperty =
      DependencyProperty.RegisterAttached("OriginalToolTip", typeof(object), typeof(CommandBinder), new(null));

   private static readonly CommandToolTipConverter ToolTipConverter = new();

   public static void SetScope(DependencyObject obj, string value) => obj.SetValue(ScopeProperty, value);
   public static string? GetScope(DependencyObject obj) => (string?)obj.GetValue(ScopeProperty);
   public static void SetScopes(DependencyObject obj, string[] value) => obj.SetValue(ScopesProperty, value);
   public static string[] GetScopes(DependencyObject obj) => (string[])obj.GetValue(ScopesProperty);

   private static void OnScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not FrameworkElement element)
         return;

      if (e.NewValue is string scope && !string.IsNullOrWhiteSpace(scope))
         SetScopes(element, [scope]);
      else
         SetScopes(element, null!);
   }

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

      var focused = Keyboard.FocusedElement;
      if (focused is TextBoxBase
                  or PasswordBox
                  or ComboBox { IsEditable: true }
                  or RichTextBox)
      {
         var modifiers = Keyboard.Modifiers;
         var isCommand = (modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != 0 ||
                         e.Key is >= Key.F1 and <= Key.F24;

         if (!isCommand)
            // Yield to the TextBox. 
            // If the TextBox doesn't handle it, it will bubble up
            return;
      }

      var key = e.Key == Key.System ? e.SystemKey : e.Key;

      if (ValidateKeyInput(key))
         return;

      // We wait for the 'Action' key to be pressed.
      if (IsModifierKey(key))
         return;

      var currentStroke = new KeyGesture(key, Keyboard.Modifiers);

      var scopes = GetScopes(element);
      if (scopes == null! || scopes.Length == 0)
         return;

      var activeScopes = !scopes.Contains(CommandScopes.GLOBAL) ? [..scopes, CommandScopes.GLOBAL] : scopes;
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

      var singleCommand = commands.FirstOrDefault(c => c.Gestures.Where(g => g is not MultiKeyGesture)
                                                        .Any(g => IsMatch((KeyGesture)g, currentStroke)));

      if (singleCommand != null)
      {
         ExecuteCommand(singleCommand, element);
         e.Handled = true;
      }
   }

   private static bool ValidateKeyInput(Key key)
   {
      if (key is Key.None
              or Key.DeadCharProcessed
              or Key.ImeProcessed
              or Key.LeftCtrl
              or Key.RightCtrl
              or Key.LeftAlt
              or Key.RightAlt
              or Key.LeftShift
              or Key.RightShift
              or Key.LWin
              or Key.RWin)
         return true;

      // TEXT INPUT GUARD: Block "Typing" gestures.
      // We want to ignore: 'A', 'Shift + A', '1', 'Shift + 1', 'Space', 'Shift + Space'.
      // We want to allow: 'Ctrl + A', 'Alt + F4', 'F1', 'Shift + F1', 'Delete'.

      var modifiers = Keyboard.Modifiers;

      // Check if a "Command Modifier" (Ctrl, Alt, or Win) is pressed.
      var hasCommandModifier = (modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) != 0;

      if (!hasCommandModifier)
      {
         // If we are here, the user is pressing [Key] or [Shift + Key].
         // We must BLOCK everything that looks like text input.
         // We only ALLOW specific non-printable "Function" keys.

         var isAllowedFunctionKey = key switch
         {
            // Function Keys (F1-F24) are always valid commands, even with just Shift
            >= Key.F1 and <= Key.F24 => true,

            // Core navigation/editing keys
            Key.Escape or Key.Tab or Key.Enter => true,
            Key.Insert or Key.Delete or Key.Back => true,
            Key.Home or Key.End or Key.PageUp or Key.PageDown => true,
            Key.Left or Key.Right or Key.Up or Key.Down => true,

            // Media keys
            Key.MediaNextTrack or Key.MediaPreviousTrack or Key.MediaPlayPause => true,

            // Everything else (Letters A-Z, Digits 0-9, OemSymbols, Space) is rejected
            // because without Ctrl/Alt, it is just text input.
            _ => false,
         };

         if (!isAllowedFunctionKey)
            return true;
      }

      return false;
   }

   private static bool IsMatch(KeyGesture template, KeyGesture input) => template.Key == input.Key && template.Modifiers == input.Modifiers;

   private static void ExecuteCommand(IAppCommand command, FrameworkElement element)
   {
      var sourceElement = FindElementBoundToCommand(element, command);

      object? parameter = null;

      if (sourceElement is ICommandSource commandSource)
         parameter = commandSource.CommandParameter;

      if (parameter == null &&
          command.Scope.Contains(CommandScopes.DIALOG, StringComparison.OrdinalIgnoreCase))
         parameter = element as Window ?? Window.GetWindow(element);

      if (command.CanExecute(parameter))
         command.Execute(parameter);
   }

   private static FrameworkElement? FindElementBoundToCommand(DependencyObject root, IAppCommand cmd)
   {
      if (root == null!)
         return null;

      if (root is FrameworkElement fe and ICommandSource source && ReferenceEquals(source.Command, cmd))
         return fe;

      var count = VisualTreeHelper.GetChildrenCount(root);
      for (var i = 0; i < count; i++)
      {
         var child = VisualTreeHelper.GetChild(root, i);
         var result = FindElementBoundToCommand(child, cmd);
         if (result != null)
            return result;
      }

      return null;
   }

   public static void SynchronizeScopedBindings(FrameworkElement element, string[]? scopes)
   {
      element.InputBindings.Clear();
      if (scopes == null || scopes.Length == 0)
         return;

      var activeScopes = !scopes.Contains(CommandScopes.GLOBAL) ? [..scopes, CommandScopes.GLOBAL] : scopes;

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
      if (d is not FrameworkElement element || e.NewValue is not IAppCommand cmd)
         return;

      // We must wait for the XAML parser to finish processing nested elements 
      element.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                                     new Action(() =>
                                     {
                                        if (element.ReadLocalValue(OriginalToolTipProperty) == DependencyProperty.UnsetValue)
                                           if (element.ToolTip != null)
                                              element.SetValue(OriginalToolTipProperty, element.ToolTip);

                                        switch (d)
                                        {
                                           case ButtonBase btn:
                                              btn.Command = cmd;
                                              break;
                                           case MenuItem mi:
                                              mi.Command = cmd;
                                              break;
                                        }

                                        var ttBinding = new MultiBinding { Converter = ToolTipConverter };
                                        ttBinding.Bindings.Add(new Binding { Source = cmd });
                                        ttBinding.Bindings.Add(new Binding { Source = element, Path = new(OriginalToolTipProperty) });

                                        BindingOperations.SetBinding(element, FrameworkElement.ToolTipProperty, ttBinding);

                                        ApplyContentBindings(element, cmd);
                                        ApplySmartCommandParameter(element, cmd);
                                     }));
   }

   private static void ApplySmartCommandParameter(FrameworkElement element, IAppCommand cmd)
   {
      if (cmd.Scope.Contains(CommandScopes.DIALOG, StringComparison.OrdinalIgnoreCase))
      {
         var currentParam = element.ReadLocalValue(ButtonBase.CommandParameterProperty);
         if (currentParam == DependencyProperty.UnsetValue)
            BindingOperations.SetBinding(element,
                                         ButtonBase.CommandParameterProperty,
                                         new Binding { RelativeSource = new(RelativeSourceMode.FindAncestor, typeof(Window), 1) });
      }
   }

   private static void ApplyContentBindings(FrameworkElement element, IAppCommand cmd)
   {
      if (element is ButtonBase { Content: null } btn)
         BindingOperations.SetBinding(btn, ContentControl.ContentProperty, new Binding(nameof(IAppCommand.DisplayName)) { Source = cmd });

      else if (element is MenuItem { Header: null } mi)
         BindingOperations.SetBinding(mi, HeaderedItemsControl.HeaderProperty, new Binding(nameof(IAppCommand.DisplayName)) { Source = cmd });
   }
}