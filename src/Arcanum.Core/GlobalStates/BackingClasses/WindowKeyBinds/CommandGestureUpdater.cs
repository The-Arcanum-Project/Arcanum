using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.Utils.UiUtils;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Arcanum.Core.GlobalStates.BackingClasses.WindowKeyBinds;

public static class CommandGestureUpdater
{
   public static void UpdateGestureTextInMenuItems(Window? window, RoutedCommand command)
   {
      if (window == null || command == null!)
         return;

      var gesture = command.InputGestures.OfType<KeyGesture>().FirstOrDefault();
      if (gesture == null)
         return;

      var gestureText = gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);

#pragma warning disable CS0618 // Type or member is obsolete
      foreach (var menuItem in TreeTraversal.FindVisualChildren<MenuItem>(window))
#pragma warning restore CS0618 // Type or member is obsolete
         if (menuItem.Command == command)
            menuItem.InputGestureText = gestureText;
   }
}