using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

      foreach (var menuItem in FindVisualChildren<MenuItem>(window))
         if (menuItem.Command == command)
            menuItem.InputGestureText = gestureText;
   }

   private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
   {
      if (depObj == null)
         yield break;

      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
      {
         var child = VisualTreeHelper.GetChild(depObj, i);
         if (child is T t)
            yield return t;

         foreach (var descendant in FindVisualChildren<T>(child))
            yield return descendant;
      }
   }
}