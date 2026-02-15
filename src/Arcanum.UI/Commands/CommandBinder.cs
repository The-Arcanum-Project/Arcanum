using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Arcanum.UI.Commands;

public static class CommandBinder
{
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
         element.Unloaded += (s, ev) => { CommandRegistry.BindingsChanged -= updateAction; };

         // Initial sync
         updateAction();
      }
   }

   public static void SynchronizeScopedBindings(FrameworkElement element, string? scopeString)
   {
      element.InputBindings.Clear();

      if (string.IsNullOrWhiteSpace(scopeString))
         return;

      var activeScopes = scopeString.Split(',').Select(s => s.Trim()).ToList();
      var filteredCommands = CommandRegistry.AllCommands
                                            .Where(c => activeScopes.Contains(c.Scope));

      foreach (var cmd in filteredCommands)
      {
         foreach (var gesture in cmd.Gestures)
         {
            var binding = new InputBinding(cmd, gesture);

            // If the element is a Window, we pass the window
            // as the default parameter for the shortcut execution.
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