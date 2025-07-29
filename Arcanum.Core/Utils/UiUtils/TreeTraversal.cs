using System.Windows;
using System.Windows.Media;

namespace Arcanum.Core.Utils.UiUtils;

public static class TreeTraversal
{
   public static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
   {
      if (parent == null)
         return null;

      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
         var child = VisualTreeHelper.GetChild(parent, i);
         if (child is T typedChild)
         {
            return typedChild;
         }

         var result = FindVisualChild<T>(child);
         if (result != null)
            return result;
      }

      return null;
   }

   public static DependencyObject? FindVisualParent(DependencyObject? child, Type type)
   {
      while (child != null)
      {
         if (type.IsInstanceOfType(child))
            return child;

         child = VisualTreeHelper.GetParent(child);
      }

      return null;
   }

   public static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
   {
      while (child != null)
      {
         if (child is T parent)
            return parent;

         child = VisualTreeHelper.GetParent(child);
      }

      return null;
   }

   public static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
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