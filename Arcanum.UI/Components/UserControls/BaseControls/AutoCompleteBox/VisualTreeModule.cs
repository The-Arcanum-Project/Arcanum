using System.Windows;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;

static class VisualTreeModule
{
   public static FrameworkElement? FindChild(DependencyObject? obj, string childName)
   {
      if (obj == null!)
         return null!;

      var queue = new Queue<DependencyObject?>();
      queue.Enqueue(obj);

      while (queue.Count > 0)
      {
         obj = queue.Dequeue();

         if (obj == null)
            continue;

         var childCount = VisualTreeHelper.GetChildrenCount(obj);
         for (var i = 0; i < childCount; i++)
         {
            var child = VisualTreeHelper.GetChild(obj, i);

            if (child is FrameworkElement fe && fe.Name == childName)
               return fe;

            queue.Enqueue(child);
         }
      }

      return null;
   }
}