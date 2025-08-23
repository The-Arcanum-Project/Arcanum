using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace Arcanum.UI.Components.Behaviors;

public class WindowDrag : Behavior<FrameworkElement>
{
   private Point? _lastMousePosition;
   private Window _window = null!;

   public static readonly DependencyProperty EnableDoubleClickMaximizeProperty =
      DependencyProperty.Register(nameof(EnableDoubleClickMaximize),
                                  typeof(bool),
                                  typeof(WindowDrag),
                                  new(true));

   public bool EnableDoubleClickMaximize
   {
      get => (bool)GetValue(EnableDoubleClickMaximizeProperty);
      set => SetValue(EnableDoubleClickMaximizeProperty, value);
   }

   protected override void OnAttached()
   {
      var nullableWindow = Window.GetWindow(AssociatedObject);
      if (nullableWindow is null)
         return;

      _window = nullableWindow;
      base.OnAttached();
      AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
      AssociatedObject.MouseMove += AssociatedObject_MouseMove;
   }

   protected override void OnDetaching()
   {
      base.OnDetaching();
      AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
   }

   // ===================================================================
   // ATTACHED PROPERTY FOR EXCLUSION ZONES
   // ===================================================================
   public static readonly DependencyProperty IsDoubleClickExclusionZoneProperty =
      DependencyProperty.RegisterAttached("IsDoubleClickExclusionZone",
                                          typeof(bool),
                                          typeof(WindowDrag),
                                          new FrameworkPropertyMetadata(false));

   /// <summary>
   /// Gets the value of the IsDoubleClickExclusionZone attached property for a specified UIElement.
   /// </summary>
   public static bool GetIsDoubleClickExclusionZone(UIElement element)
   {
      return (bool)element.GetValue(IsDoubleClickExclusionZoneProperty);
   }

   /// <summary>
   /// Sets the value of the IsDoubleClickExclusionZone attached property for a specified UIElement.
   /// When set to true, double-clicks on this element will not trigger window maximization.
   /// </summary>
   public static void SetIsDoubleClickExclusionZone(UIElement element, bool value)
   {
      element.SetValue(IsDoubleClickExclusionZoneProperty, value);
   }
   // ===================================================================

   private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount == 2 && EnableDoubleClickMaximize)
      {
         var clickPoint = e.GetPosition(AssociatedObject);
         foreach (var zone in FindExclusionZones(AssociatedObject))
         {
            // Get the bounds of the exclusion zone relative to the AssociatedObject (the header).
            // This is crucial for correct hit-testing.
            var zoneBounds = zone.TransformToAncestor(AssociatedObject)
                                 .TransformBounds(new Rect(0, 0, zone.ActualWidth, zone.ActualHeight));

            if (zoneBounds.Contains(clickPoint))
               // The click was inside an exclusion zone, so we do nothing.
               return;
         }

         if (_window.WindowState == WindowState.Normal)
            _window.WindowState = WindowState.Maximized;
         else if (_window.WindowState == WindowState.Maximized)
            _window.WindowState = WindowState.Normal;
      }
      else if (_window.WindowState == WindowState.Maximized)
         _lastMousePosition ??= e.GetPosition(_window);
      // double click
   }

   private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
   {
      if (e.LeftButton != MouseButtonState.Pressed)
         return;

      // If we are in a maximized state, we only want to move the window once we drag it, not just by clicking 
      if (_window.WindowState == WindowState.Maximized)
      {
         var position = e.GetPosition(_window);
         if (_lastMousePosition != null && _lastMousePosition != position)
         {
            var mousePosition = _window.PointToScreen(position);

            // Calculate the desired left offset so that the mouse remains at the same
            // relative position on the window after restoring
            var targetLeft = mousePosition.X - _window.RestoreBounds.Width / 2;

            _window.WindowState = WindowState.Normal;

            // Set windows left, so center is under mouse
            _window.Left = targetLeft;
            _window.Top = mousePosition.Y - position.Y; // preserve Y coordinate

            _lastMousePosition = null;
         }
      }

      _window.DragMove();
   }

   /// <summary>
   /// Traverses the visual tree starting from the parent and finds all FrameworkElements
   /// tagged with the IsDoubleClickExclusionZone attached property.
   /// </summary>
   private static IEnumerable<FrameworkElement> FindExclusionZones(DependencyObject parent)
   {
      if (parent == null!)
         yield break;

      var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
      for (var i = 0; i < childrenCount; i++)
      {
         var child = VisualTreeHelper.GetChild(parent, i);
         if (child is FrameworkElement frameworkElement && GetIsDoubleClickExclusionZone(frameworkElement))
            yield return frameworkElement;

         // Recurse into the child's children
         foreach (var nestedZone in FindExclusionZones(child))
            yield return nestedZone;
      }
   }
}