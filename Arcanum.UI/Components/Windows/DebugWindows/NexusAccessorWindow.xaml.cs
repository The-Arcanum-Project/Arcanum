using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class NexusAccessorWindow
{
   public static readonly DependencyProperty NexusItemsProperty =
      DependencyProperty.Register(nameof(NexusItems),
                                  typeof(string),
                                  typeof(NexusAccessorWindow),
                                  new(default(string)));

   public string NexusItems
   {
      get => (string)GetValue(NexusItemsProperty);
      set => SetValue(NexusItemsProperty, value);
   }

   public NexusAccessorWindow()
   {
      InitializeComponent();
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
   }
}