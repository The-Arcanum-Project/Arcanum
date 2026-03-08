using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.UI.SpecializedEditors.Management;

namespace Arcanum.UI.SpecializedEditors.Editors;

public class InstitutionEditor : ISpecializedEditor
{
   public bool Enabled { get; set; }
   public string DisplayName => "Institution";
   public string? IconResource => null;
   public int Priority => 0;
   public bool SupportsMultipleTargets => true;
   public bool CanEdit(object[] targets, Enum? prop) => true;

   public void Reset()
   {
      EditorControls.InstitutionEditor.Instance.ViewModel.Reset();
   }

   public void ResetFor(object[] targets)
   {
      EditorControls.InstitutionEditor.Instance.ViewModel.Reset();

      if (targets.Length > 0 && targets[0] is Location)
         EditorControls.InstitutionEditor.Instance.ViewModel.SetForLocations(targets.Cast<Location>().ToArray());
   }

   public FrameworkElement GetEditorControl() => EditorControls.InstitutionEditor.Instance;

   public IEnumerable<MenuItem> GetContextMenuActions() => [];

   public void OnEnabledChanged(bool value)
   {
      if (!value)
      {
         EditorControls.InstitutionEditor.Instance.ViewModel.UnsubscribeFromSelectionChanges();
         return;
      }

      EditorControls.InstitutionEditor.Instance.ViewModel.SubscribeToSelectionChanges();
   }
}