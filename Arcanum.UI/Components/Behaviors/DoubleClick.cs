using Arcanum.UI.Helpers;

namespace Arcanum.UI.Components.Behaviors;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

public class ClickAndDoubleClickBehavior : Behavior<UIElement>
{
   public ICommand ClickCommand
   {
      get => (ICommand)GetValue(ClickCommandProperty);
      set => SetValue(ClickCommandProperty, value);
   }

   public static readonly DependencyProperty ClickCommandProperty =
      DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(ClickAndDoubleClickBehavior));

   public ICommand DoubleClickCommand
   {
      get => (ICommand)GetValue(DoubleClickCommandProperty);
      set => SetValue(DoubleClickCommandProperty, value);
   }

   public static readonly DependencyProperty DoubleClickCommandProperty =
      DependencyProperty.Register(nameof(DoubleClickCommand), typeof(ICommand), typeof(ClickAndDoubleClickBehavior));

   public bool FireSingleClickOnDoubleClick
   {
      get => (bool)GetValue(FireSingleClickOnDoubleClickProperty);
      set => SetValue(FireSingleClickOnDoubleClickProperty, value);
   }

   public static readonly DependencyProperty FireSingleClickOnDoubleClickProperty =
      DependencyProperty.Register(nameof(FireSingleClickOnDoubleClick),
                                  typeof(bool),
                                  typeof(ClickAndDoubleClickBehavior),
                                  new(false));

   private DateTime _lastClickTime;
   private DispatcherTimer _clickTimer = null!;

   protected override void OnAttached()
   {
      base.OnAttached();
      AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;

      _clickTimer = new() { Interval = TimeSpan.FromMilliseconds(NativeMethods.GetDoubleClickTime()) };
      _clickTimer.Tick += OnClickTimerTick;
   }

   protected override void OnDetaching()
   {
      base.OnDetaching();
      AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
      _clickTimer.Tick -= OnClickTimerTick;
      _clickTimer.Stop();
   }

   private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
   {
      var now = DateTime.Now;
      if ((now - _lastClickTime).TotalMilliseconds <= NativeMethods.GetDoubleClickTime())
      {
         _clickTimer.Stop();
         DoubleClickCommand.Execute(null);
      }
      else if (FireSingleClickOnDoubleClick)
      {
         ClickCommand.Execute(null);
      }
      else
      {
         _clickTimer.Start();
      }

      _lastClickTime = now;
   }

   private void OnClickTimerTick(object? sender, EventArgs e)
   {
      _clickTimer.Stop();
      ClickCommand.Execute(null);
   }
}