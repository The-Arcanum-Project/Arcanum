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
      if (d is not FrameworkElement element || !(bool)e.NewValue)
         return;

      element.Loaded += (_, _) =>
      {
         if (element.IsVisible)
            RegisterContext(element.DataContext);
      };

      element.Unloaded += (_, _) => { UnregisterContext(element.DataContext); };

      element.IsVisibleChanged += (_, args) =>
      {
         if ((bool)args.NewValue)
            RegisterContext(element.DataContext);
         else
            UnregisterContext(element.DataContext);
      };
   }

   private static void RegisterContext(object? dc)
   {
      if (dc == null)
         return;

      List<Type> interfaces =
      [
         ..dc.GetType()
             .GetInterfaces()
             .Where(i => typeof(IAppContext).IsAssignableFrom(i)),
      ];

      foreach (var i in interfaces)
         ArcAppContext.UpdateContext(i, dc);
   }

   private static void UnregisterContext(object? dc)
   {
      if (dc == null)
         return;

      List<Type> interfaces =
      [
         ..dc.GetType()
             .GetInterfaces()
             .Where(i => typeof(IAppContext).IsAssignableFrom(i)),
      ];

      foreach (var i in interfaces)
         ArcAppContext.RemoveContext(i);
   }
}