using System.Windows;
using Arcanum.Core.ApplicationContext;
using Arcanum.UI.AppFeatures;

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

      element.Loaded -= OnElementOnLoaded;
      element.Unloaded -= OnElementOnUnloaded;
      element.IsVisibleChanged -= OnElementOnIsVisibleChanged;
      element.DataContextChanged -= OnElementOnDataContextChanged;

      element.Loaded += OnElementOnLoaded;
      element.Unloaded += OnElementOnUnloaded;
      element.IsVisibleChanged += OnElementOnIsVisibleChanged;
      element.DataContextChanged += OnElementOnDataContextChanged;
      return;

      void OnElementOnLoaded(object o, RoutedEventArgs routedEventArgs)
      {
         if (element.IsVisible)
            RegisterContext(element.DataContext);
      }

      void OnElementOnUnloaded(object o, RoutedEventArgs routedEventArgs)
      {
         UnregisterContext(element.DataContext);
         element.Loaded -= OnElementOnLoaded;
         element.IsVisibleChanged -= OnElementOnIsVisibleChanged;
         element.DataContextChanged -= OnElementOnDataContextChanged;
         element.Unloaded -= OnElementOnUnloaded;
      }

      void OnElementOnIsVisibleChanged(object _, DependencyPropertyChangedEventArgs args)
      {
         if ((bool)args.NewValue)
            RegisterContext(element.DataContext);
         else
            UnregisterContext(element.DataContext);
      }

      void OnElementOnDataContextChanged(object _, DependencyPropertyChangedEventArgs args)
      {
         switch (args.NewValue)
         {
            case IAppFeature feature:
               FeatureRegistry.Register(feature);
               break;
            case IAppFeatureProvider provider:
               FeatureRegistry.Register(provider.FeatureMetadata);
               break;
         }

         // If the element is already loaded and visible, update active status immediately
         if (element is { IsLoaded: true, IsVisible: true })
            RegisterContext(args.NewValue);
      }
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

      switch (dc)
      {
         case IAppFeature feature:
            FeatureRegistry.AddActiveFeature(feature);
            break;
         case IAppFeatureProvider provider:
            FeatureRegistry.AddActiveFeature(provider.FeatureMetadata);
            break;
      }
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

      if (dc is IAppFeature feature)
         FeatureRegistry.RemoveActiveFeature(feature);
   }
}