#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.SpecializedEditors.Management;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public partial class PopsEditor : ISpecializedEditor
{
   public PopsEditor()
   {
      InitializeComponent();
      DataContext = new AllocatorViewModel(Location.Empty);
   }

   public bool Enabled { get; set; } = false;
   public string DisplayName => "Pops";
   public string? IconResource => null;
   public int Priority => 0;
   public bool SupportsMultipleTargets => false;
   public bool CanEdit(object[] targets, Enum? prop) => true;

   public void Reset()
   {
      if (DataContext is AllocatorViewModel vm)
         vm.Reset();
      else
         throw new NotSupportedException();
   }

   public void ResetFor(object[] targets)
   {
      if (DataContext is AllocatorViewModel vm)
      {
         if (targets.Length < 1)
            return;

         var first = targets[0];
         if (first is not Location location)
            MBox.Show("IntValueAllocator can only edit Location targets.", "Error");
         else
            vm.ResetFor(location);
      }
      else
         throw new NotSupportedException();
   }

   public FrameworkElement GetEditorControl() => this;

   public IEnumerable<MenuItem> GetContextMenuActions() => [];

   public void OnEnabledChanged(bool value)
   {
   }

   private void Border_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
   {
      Console.WriteLine("Right click detected!");
   }

   private void PopupContent_PreviewKeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key != Key.Escape)
         return;

      if (sender is FrameworkElement element)
      {
         var parent = element.Parent;
         while (parent != null && parent is not Popup)
            parent = LogicalTreeHelper.GetParent(parent);

         if (parent is Popup popup)
         {
            popup.IsOpen = false;
            e.Handled = true;
         }
      }
   }

   private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is ComboBox selector)
      {
         var be = selector.GetBindingExpression(Selector.SelectedItemProperty);
         be?.UpdateSource();
      }
   }
}