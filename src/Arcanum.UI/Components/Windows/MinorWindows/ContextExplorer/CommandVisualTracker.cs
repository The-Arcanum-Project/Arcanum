using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;

public class CommandVisualTracker
{
   private readonly List<Adorner> _activeAdorners = [];

   // Maps a Command Instance to all UI elements displaying it
   private readonly Dictionary<IAppCommand, List<FrameworkElement>> _map = new();

   public void Scan(DependencyObject root)
   {
      _map.Clear();
      FindCommandsRecursive(root);
   }

   private void FindCommandsRecursive(DependencyObject parent)
   {
      var count = VisualTreeHelper.GetChildrenCount(parent);
      for (var i = 0; i < count; i++)
      {
         var child = VisualTreeHelper.GetChild(parent, i);
         if (child is FrameworkElement fe)
         {
            // check our custom 'Assign' property
            var cmd = CommandBinder.GetAssign(fe);

            if (cmd != null!)
            {
               if (!_map.ContainsKey(cmd))
                  _map[cmd] = [];
               _map[cmd].Add(fe);
            }
         }

         FindCommandsRecursive(child);
      }
   }

   public void ShowHighlights(IAppCommand cmd)
   {
      Clear();
      if (!_map.TryGetValue(cmd, out var elements))
         return;

      foreach (var el in elements)
      {
         var layer = AdornerLayer.GetAdornerLayer(el);
         if (layer == null)
            continue;

         var adorner = new CommandHighlightAdorner(el);
         layer.Add(adorner);
         _activeAdorners.Add(adorner);
      }
   }

   public void Clear()
   {
      foreach (var adorner in _activeAdorners)
      {
         var layer = AdornerLayer.GetAdornerLayer(adorner.AdornedElement);
         layer?.Remove(adorner);
      }

      _activeAdorners.Clear();
   }
}