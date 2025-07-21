using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.UI.Helpers;

namespace Arcanum.Core.Globals.BackingClasses.WindowKeyBinds;

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

      foreach (var menuItem in TreeTraversal.FindVisualChildren<MenuItem>(window))
         if (menuItem.Command == command)
            menuItem.InputGestureText = gestureText;
   }

}