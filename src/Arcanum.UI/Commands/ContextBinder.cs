using System.Windows;
using Arcanum.Core.ApplicationContext;

namespace Arcanum.UI.Commands;

public static class ContextBinder
{
   public static readonly DependencyProperty ObserveContextProperty =
      DependencyProperty.RegisterAttached("ObserveContext",
                                          typeof(bool),
                                          typeof(ContextBinder),
                                          new(false, OnObserveContextChanged));

   public static void SetObserveContext(DependencyObject obj, bool value) => obj.SetValue(ObserveContextProperty, value);

   private static void OnObserveContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is FrameworkElement element && (bool)e.NewValue)
         element.IsKeyboardFocusWithinChanged += (_, _) =>
         {
            if (element.IsKeyboardFocusWithin)
               RegisterContext(element.DataContext);
         };
   }

   private static void RegisterContext(object? dc)
   {
      if (dc == null)
         return;

      var moduleInterfaces = dc.GetType()
                               .GetInterfaces()
                               .Where(i => typeof(IAppContext).IsAssignableFrom(i));

      foreach (var @interface in moduleInterfaces)
         ArcAppContext.UpdateContext(@interface, dc);
   }
}