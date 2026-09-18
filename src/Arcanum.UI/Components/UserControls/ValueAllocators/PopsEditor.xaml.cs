#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.UI.SpecializedEditors.Management;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public sealed partial class PopsEditor : ISpecializedEditor
{
   public PopEditorVm MainVm => (PopEditorVm)DataContext;

   public PopsEditor()
   {
      InitializeComponent();
      DataContext = new PopEditorVm();
   }

   protected override void OnPreviewKeyDown(KeyEventArgs e)
   {
      base.OnPreviewKeyDown(e);

      // Handle Ctrl+Q at the PopsEditor level as a fallback for cases where
      // descendant controls mark the key event handled (common with custom
      // controls or popups). This ensures the hotkey always reaches the VM.
      if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Q)
         if (DataContext is PopEditorVm vm && vm.ApplyChangesCommand != null && vm.ApplyChangesCommand.CanExecute(null))
         {
            vm.ApplyChangesCommand.Execute(null);
            e.Handled = true;
         }
   }

   public bool Enabled { get; set; } = false;
   public string DisplayName => "Pops";
   public string? IconResource => null;
   public int Priority => 0;
   public bool SupportsMultipleTargets => true;
   public bool CanEdit(object[] targets, Enum? prop) => true;

   public void Reset()
   {
      if (MainVm.ActiveEditor is AllocatorViewModel singleVm)
         singleVm.Reset();
   }

   public void ResetFor(object[] targets)
   {
      var selectedLocations = targets.OfType<Location>().ToArray();
      if (selectedLocations.Length == 0)
         MainVm.ActiveEditor = null;
      else if (selectedLocations.Length == 1)
      {
         if (MainVm.ActiveEditor is AllocatorViewModel singleVm && singleVm.LoadedLocation == selectedLocations[0])
         {
            singleVm.ResetFor(selectedLocations[0]);
            goto setTitle;
         }

         (MainVm.ActiveEditor as MassPopPainterViewModel)?.SaveState();
         MainVm.ActiveEditor = new AllocatorViewModel(selectedLocations[0]);
      }
      else
      {
         if (MainVm.ActiveEditor is MassPopPainterViewModel)
         {
            (MainVm.ActiveEditor as MassPopPainterViewModel)?.ResetFor(selectedLocations);
            goto setTitle;
         }

         MainVm.ActiveEditor = new MassPopPainterViewModel(selectedLocations);
      }

   setTitle:
      MainVm.Title = selectedLocations.Length switch
      {
         0 => "Pops",
         1 => $"Pops - {selectedLocations[0].UniqueId}",
         _ => $"Pops - {selectedLocations.Length} Locations",
      };
   }

   public FrameworkElement GetEditorControl() => this;

   public IEnumerable<MenuItem> GetContextMenuActions() => [];

   public void OnEnabledChanged(bool value)
   {
   }
}