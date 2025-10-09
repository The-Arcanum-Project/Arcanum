using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Nui2.Nui2Gen.NavHistory;

namespace Arcanum.UI.NUI.Nui2.Nui2Gen;

public static class EventHandlers
{
   /// <summary>
   /// Sets a MouseUp event handler on the given TextBlock to navigate to the specified target
   /// when clicked. If the right mouse button is clicked, a context menu with navigation options
   /// is displayed instead.
   /// </summary>
   public static void SetOnMouseUpHandler(TextBlock tb, NavH navh, IEu5Object target)
   {
      MouseButtonEventHandler handler = (_, args) =>
      {
         if (args.ChangedButton == MouseButton.Left)
         {
            navh.NavigateTo(target);
         }
         else
         {
            var navs = navh.GetNavigations();
            if (navs.Length < 1)
               return;

            var contextMenu = ControlFactory.GetContextMenu(navs, navh);
            tb.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;
            args.Handled = true;
         }
      };
      tb.MouseUp += handler;
      tb.Unloaded += (_, _) => tb.MouseUp -= handler;
   }

   public static MouseButtonEventHandler GetSimpleNavigationHandler(NavH navh, IEu5Object target)
   {
      return (_, args) =>
      {
         if (args.ChangedButton == MouseButton.Left)
         {
            navh.NavigateTo(target);
         }
         else
         {
            var navs = navh.GetNavigations();
            if (navs.Length < 1)
               return;

            args.Handled = true;
         }
      };
   }
}