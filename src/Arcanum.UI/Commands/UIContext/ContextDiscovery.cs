using System.Windows;
using System.Windows.Media;

namespace Arcanum.UI.Commands.UIContext;

public static class ContextDiscovery
{
   public static List<string> GetActiveScopes(DependencyObject? focusedElement, bool deepSearch)
   {
      var scopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var current = focusedElement;

      while (current != null)
      {
         var localScopes = CommandBinder.GetScopes(current);
         if (localScopes != null! && localScopes.Length > 0)
         {
            scopes.UnionWith(localScopes);

            if (!deepSearch)
               break;
         }

         current = VisualTreeHelper.GetParent(current);
      }

      scopes.Add(CommandScopes.GLOBAL);
      return scopes.OrderBy(x => x).ToList();
   }

   public static List<string> GetActiveScopes(Window window, bool deepSearch)
   {
      // Get scopes of a dependencyObject by calling the static method: CommandBinder.GetScopes(dependencyObject);
      var scopes = new HashSet<string>();

      SearchScopes(window);
      scopes.Add(CommandScopes.GLOBAL);
      return scopes.Distinct().ToList();

      void SearchScopes(DependencyObject obj)
      {
         var objScopes = CommandBinder.GetScopes(obj);
         if (objScopes != null! && objScopes.Length > 0)
         {
            scopes.UnionWith(objScopes);

            if (!deepSearch)
               return;
         }

         for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            SearchScopes(VisualTreeHelper.GetChild(obj, i));
      }
   }
}