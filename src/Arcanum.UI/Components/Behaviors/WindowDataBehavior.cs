using System.Windows;
using System.Windows.Data;
using Arcanum.Core.GlobalStates.BackingClasses;
using Arcanum.UI.Helpers;
using Microsoft.Xaml.Behaviors;

namespace Arcanum.UI.Components.Behaviors;

public class WindowDataBehavior : Behavior<Window>
{
   protected override void OnAttached()
   {
      base.OnAttached();

      AssociatedObject.SizeChanged += OnWindowSizeChanged;

      var locationBinding = new Binding("ActualX,ActualY") { Mode = BindingMode.TwoWay, Source = AssociatedObject };
      AssociatedObject.SetBinding(Window.LeftProperty, locationBinding);
      AssociatedObject.SetBinding(Window.TopProperty, locationBinding);

      AssociatedObject.StateChanged += OnWindowStateChanged;
   }

   protected override void OnDetaching()
   {
      base.OnDetaching();

      AssociatedObject.SizeChanged -= OnWindowSizeChanged;

      BindingOperations.ClearBinding(AssociatedObject, Window.LeftProperty);
      BindingOperations.ClearBinding(AssociatedObject, Window.TopProperty);

      AssociatedObject.StateChanged -= OnWindowStateChanged;
   }

   private static void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
   {
      if (sender is Window window)
         WindowDataChanged(window);
   }

   private static void OnWindowStateChanged(object? sender, EventArgs e)
   {
      if (sender is Window window)
         WindowDataChanged(window);
   }

   private static void WindowDataChanged(Window sender)
   {
      var relativePos = sender.GetRelativePosition();
      var newData = new WindowStateData(sender.GetType().Name,
                                    relativePos.Item1,
                                    relativePos.Item2,
                                        sender.Width,
                                        sender.Height,
                                        (int)sender.WindowState);

      WindowData.AddWindowStateData(newData);
   }
}