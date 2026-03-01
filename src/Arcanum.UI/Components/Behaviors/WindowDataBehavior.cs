using System.ComponentModel;
using System.Windows;
using Arcanum.Core.GlobalStates.BackingClasses;
using Arcanum.UI.Helpers;
using Common.Logger;
using Microsoft.Xaml.Behaviors;

namespace Arcanum.UI.Components.Behaviors;

public class WindowDataBehavior : Behavior<Window>
{
   public int DefaultWidth { get; set; } = 800;
   public int DefaultHeight { get; set; } = 600;

   protected override void OnAttached()
   {
      base.OnAttached();

      AssociatedObject.Closing += OnWindowClosing;
      AssociatedObject.Loaded += LoadWindowData;
   }

   protected override void OnDetaching()
   {
      base.OnDetaching();

      AssociatedObject.Closing -= OnWindowClosing;
      AssociatedObject.Loaded -= LoadWindowData;
   }

   private static void OnWindowClosing(object? sender, CancelEventArgs e)
   {
      if (sender is Window window)
         WindowDataChanged(window);
   }

   private void LoadWindowData(object? sender, RoutedEventArgs e)
   {
      if (sender is not Window window)
         return;

      var data = WindowData.GetWindowStateData(window.GetType());
      if (data != null)
      {
         window.SetScreenOffset((int)data.Left, (int)data.Top, (int)data.Width, (int)data.Height);
         if (Enum.IsDefined(typeof(WindowState), data.WindowState))
            window.WindowState = (WindowState)data.WindowState;
      }
      else
      {
         ArcLog.WriteLine("MW", LogLevel.WRN, "Could not load window data for main window, using defaults.");
         window.SetScreen(DefaultWidth, DefaultHeight);
      }
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