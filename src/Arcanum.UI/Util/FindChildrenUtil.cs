using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arcanum.UI.Util;

public static class FindChildrenUtil
{
   public static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
   {
      List<Control> controls = [];

      foreach (var c in LogicalTreeHelper.GetChildren(parent))
      {
         if (c is not Control control)
            continue;

         controls.Add(control);
      }

      foreach (var control in controls)
      {
         if (control is T t)
            return t;

         var child = FindChild<T>(control);
         if (child != null)
            return child;
      }

      return null;
   }

   public static T? FindFirstParentOfType<T>(DependencyObject child) where T : DependencyObject
   {
      var parent = VisualTreeHelper.GetParent(child);
      while (parent != null)
      {
         if (parent is T t)
            return t;

         parent = VisualTreeHelper.GetParent(parent);
      }

      return null;
   }

   public static bool IsDescendantOf(DependencyObject? child, DependencyObject? parent)
   {
      while (child != null)
      {
         if (child == parent)
            return true;

         child = VisualTreeHelper.GetParent(child);
      }

      return false;
   }
}