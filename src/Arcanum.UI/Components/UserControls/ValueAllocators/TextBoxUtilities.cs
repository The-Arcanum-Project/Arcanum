using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public static class TextBoxUtilities
{
   // 1. The Property to set the Delay (in milliseconds)
   public static readonly DependencyProperty UpdateDelayProperty =
      DependencyProperty.RegisterAttached("UpdateDelay",
                                          typeof(int),
                                          typeof(TextBoxUtilities),
                                          new(0, OnUpdateDelayChanged));

   public static int GetUpdateDelay(DependencyObject obj) => (int)obj.GetValue(UpdateDelayProperty);
   public static void SetUpdateDelay(DependencyObject obj, int value) => obj.SetValue(UpdateDelayProperty, value);

   // 2. Private Property to store the Timer instance on the specific TextBox
   private static readonly DependencyProperty TimerProperty =
      DependencyProperty.RegisterAttached("Timer",
                                          typeof(DispatcherTimer),
                                          typeof(TextBoxUtilities),
                                          new(null));

   private static void OnUpdateDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not CorneredTextBox textBox)
         return;

      // Hook up events
      textBox.TextChanged -= TextBox_TextChanged;
      textBox.KeyDown -= TextBox_KeyDown;
      textBox.LostFocus -= TextBox_LostFocus;

      if ((int)e.NewValue > 0)
      {
         textBox.TextChanged += TextBox_TextChanged;
         textBox.KeyDown += TextBox_KeyDown;
         textBox.LostFocus += TextBox_LostFocus;
      }
   }

   private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      var textBox = (CorneredTextBox)sender;
      var delay = GetUpdateDelay(textBox);

      // Get or Create the Timer associated with this TextBox
      var timer = (DispatcherTimer)textBox.GetValue(TimerProperty);
      if (timer == null)
      {
         timer = new();
         timer.Tick += (s, args) =>
         {
            var t = (DispatcherTimer)s;
            t.Stop();
            // TRIGGER THE UPDATE
            TriggerUpdate(textBox);
         };
         textBox.SetValue(TimerProperty, timer);
      }

      // Reset the timer (Debounce logic)
      timer.Interval = TimeSpan.FromMilliseconds(delay);
      timer.Stop();
      timer.Start();
   }

   private static void TextBox_KeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Enter)
      {
         var textBox = (CorneredTextBox)sender;
         StopTimer(textBox);
         TriggerUpdate(textBox);

         // Optional: Select all text or move focus to make it feel "Submitted"
         textBox.SelectAll();
      }
   }

   private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
   {
      var textBox = (CorneredTextBox)sender;
      StopTimer(textBox);
      TriggerUpdate(textBox);
   }

   private static void StopTimer(CorneredTextBox textBox)
   {
      var timer = (DispatcherTimer)textBox.GetValue(TimerProperty);
      timer?.Stop();
   }

   private static void TriggerUpdate(CorneredTextBox textBox)
   {
      // This manually fires the binding update to the ViewModel
      var binding = textBox.GetBindingExpression(TextBox.TextProperty);
      binding?.UpdateSource();
   }
}