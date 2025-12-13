using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Clipboard;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.NUI.Generator;

public static class EventHandlers
{
   /// <summary>
   /// Sets a MouseUp event handler on the given TextBlock to navigate to the specified target
   /// when clicked. If the right mouse button is clicked, a context menu with navigation options
   /// is displayed instead.
   /// </summary>
   public static void SetOnMouseUpHandler(TextBlock tb, NavH navh, Enum nxProp)
   {
      MouseButtonEventHandler handler = (sender, args) =>
      {
         if (sender is not FrameworkElement { Tag: IEu5Object currentTarget })
            return;

         if (args.ChangedButton == MouseButton.Left)
         {
            // We Paste here if Shift is held down
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
               foreach (var navTarget in navh.Targets)
                  ArcClipboard.Paste(navTarget, nxProp);

               if (navh.Targets.Count >= 1)
                  NUINavigation.Instance.ForceInvalidateUi();
            }
            else
               navh.NavigateTo(currentTarget);
         }
         else if (args.ChangedButton == MouseButton.Right)
         {
            if (Keyboard.IsKeyDown(Key.LeftShift) && navh.Targets is [{ } target])
               ArcClipboard.Copy(target, nxProp);
            else
            {
               var contextMenu = ControlFactory.GetContextMenu(navh.GetNavigations(), navh, nxProp);
               tb.ContextMenu = contextMenu;
               contextMenu.IsOpen = true;
               args.Handled = true;
            }
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