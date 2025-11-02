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

   public static readonly DependencyProperty RespectMouseMoveProperty =
      DependencyProperty.Register(nameof(RespectMouseMove),
                                  typeof(bool),
                                  typeof(ClickAndDoubleClickBehavior),
                                  new(false));

   public bool RespectMouseMove
   {
      get => (bool)GetValue(RespectMouseMoveProperty);
      set => SetValue(RespectMouseMoveProperty, value);
   }

   private static void ExecuteCommand(ICommand? command, MouseButtonEventArgs? parameter)
   {
      if (command?.CanExecute(parameter) == true)
         command.Execute(parameter);
   }

   private DateTime _lastClickTime;
   private readonly DispatcherTimer _clickTimer;
   private Point? _lastMousePosition;
   private MouseButtonEventArgs? _lastClickEventArgs;

   public ClickAndDoubleClickBehavior()
   {
      _clickTimer = new() { Interval = TimeSpan.FromMilliseconds(NativeMethods.GetDoubleClickTime()) };
      _clickTimer.Tick += OnClickTimerTick;
   }

   protected override void OnAttached()
   {
      base.OnAttached();
      AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
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
         if (_lastMousePosition != null && RespectMouseMove)
         {
            var currentPos = e.GetPosition(AssociatedObject);
            if (Math.Abs(currentPos.X - _lastMousePosition.Value.X) > SystemParameters.MouseHoverWidth ||
                Math.Abs(currentPos.Y - _lastMousePosition.Value.Y) > SystemParameters.MouseHoverHeight)
            {
               // Mouse has moved too much since the last click
               _lastMousePosition = currentPos;
               _lastClickTime = now;
               _clickTimer.Stop();
               ExecuteCommand(ClickCommand, e);
               return;
            }
         }

         if (!FireSingleClickOnDoubleClick)
            _clickTimer.Stop();
         ExecuteCommand(DoubleClickCommand, e);
         _lastClickTime = DateTime.MinValue;
      }
      else
      {
         _lastClickTime = now;
      }

      if (FireSingleClickOnDoubleClick)
      {
         ExecuteCommand(ClickCommand, e);
      }
      else
      {
         _lastClickEventArgs = e;
         _clickTimer.Start();
      }

      _lastMousePosition = e.GetPosition(AssociatedObject);
   }

   private void OnClickTimerTick(object? sender, EventArgs e)
   {
      _clickTimer.Stop();
      ExecuteCommand(ClickCommand, _lastClickEventArgs);
      _lastClickEventArgs = null;
   }
}