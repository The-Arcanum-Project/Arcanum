using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public static class InteractionMonitor
{
   // Bind this to the AllocatorViewModel
   public static readonly DependencyProperty AllocatorVmProperty =
      DependencyProperty.RegisterAttached("AllocatorVm",
                                          typeof(AllocatorViewModel),
                                          typeof(InteractionMonitor),
                                          new(null, OnVMChanged));

   public static void SetAllocatorVm(DependencyObject element, AllocatorViewModel value) => element.SetValue(AllocatorVmProperty, value);

   public static AllocatorViewModel GetAllocatorVm(DependencyObject element) => (AllocatorViewModel)element.GetValue(AllocatorVmProperty);

   private static void OnVMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not UIElement element)
         return;

      element.PreviewMouseDown -= Element_PreviewMouseDown;
      element.GotFocus -= Element_GotFocus;

      if (e.NewValue == null)
         return;

      if (element is Selector ||
          element is RangeBase || // Slider
          element is Button ||
          element is ToggleButton)
         element.PreviewMouseDown += Element_PreviewMouseDown;
      else if (element is TextBox)
         // For TextBox, Focus is the start of interaction (handles click AND tab)
         element.GotFocus += Element_GotFocus;
      else
         // Fallback for generic containers (like Border) -> MouseDown
         element.PreviewMouseDown += Element_PreviewMouseDown;
   }

   private static void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
   {
      Snapshot(sender);
   }

   private static void Element_GotFocus(object sender, RoutedEventArgs e)
   {
      Snapshot(sender);
   }

   private static void Snapshot(object sender)
   {
      if (sender is DependencyObject d)
      {
         var vm = GetAllocatorVm(d);
         // Snapshot state!
         vm.SnapshotState();
      }
   }
}