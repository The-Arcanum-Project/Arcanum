using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.UI.SpecializedEditors.Management;

namespace Arcanum.UI.SpecializedEditors.Editors;

public class PoliticalEditor : ISpecializedEditor
{
   public bool Enabled { get; set; }
   public string DisplayName { get; } = "Political";
   public string? IconResource { get; } = null;
   public int Priority { get; } = 10;
   public bool SupportsMultipleTargets { get; } = false;
   public bool CanEdit(object[] targets, Enum? prop) => true;

   public void Reset()
   {
      var vm = EditorControls.PoliticalEditor.Instance.ViewModel;
      vm.Clear();
      if (SelectionManager.EditableObjects.Count == 1 && vm.SyncWithSearch && SelectionManager.EditableObjects[0] is Country country)
         vm.UpdateViewModel(country);
   }

   public void ResetFor(object[] targets)
   {
   }

   public FrameworkElement GetEditorControl() => EditorControls.PoliticalEditor.Instance;

   public IEnumerable<MenuItem> GetContextMenuActions() => [];

   public void OnEnabledChanged(bool value)
   {
      if (!value)
      {
         EditorControls.PoliticalEditor.Instance.ViewModel.UnsubscribeFromSelectionChanges();
         return;
      }

      if (SelectionManager.EditableObjects.Count == 1 &&
          EditorControls.PoliticalEditor.Instance.ViewModel.SyncWithSearch &&
          SelectionManager.EditableObjects[0] is Country country)
         EditorControls.PoliticalEditor.Instance.ViewModel.UpdateViewModel(country);

      EditorControls.PoliticalEditor.Instance.ViewModel.SubscribeToSelectionChanges();
   }
}