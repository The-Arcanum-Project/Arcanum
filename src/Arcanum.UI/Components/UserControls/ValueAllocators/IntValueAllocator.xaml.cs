using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.SpecializedEditors.Management;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public partial class IntValueAllocator : ISpecializedEditor
{
   public IntValueAllocator()
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
}