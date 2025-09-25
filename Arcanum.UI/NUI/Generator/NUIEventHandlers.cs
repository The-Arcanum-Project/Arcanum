using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Selection;

namespace Arcanum.UI.NUI.Generator;

public static class NUIEventHandlers
{
   internal static MouseButtonEventHandler MouseButtonEventHandler<T>(NUINavHistory navh, T value, TextBlock header)
      where T : INUI
   {
      MouseButtonEventHandler clickHandler = (sender, e) =>
      {
         var root = navh.Root;
         if (e.ChangedButton == MouseButton.Right)
         {
            var navigations = navh.GetNavigations();
            if (navigations.Length == 0)
            {
               e.Handled = true;
               return;
            }

            var contextMenu = NUIViewGenerator.GetContextMenu(navigations, root);
            contextMenu.PlacementTarget = sender as UIElement ?? header;
            contextMenu.IsOpen = true;
            e.Handled = true;
         }
         else if (e.ChangedButton == MouseButton.Left)
            root.Content = NUIViewGenerator.GenerateView(new(value, true, root));
      };
      return clickHandler;
   }
}