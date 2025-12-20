using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.UI.SpecializedEditors.EditorControls;
using Arcanum.UI.SpecializedEditors.Management;
using Common;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;

namespace Arcanum.UI.SpecializedEditors.Editors;

public abstract class LocationCollectionSpecializedEditor : ISpecializedEditor
{
   public abstract bool Enabled { get; set; }
   public abstract string DisplayName { get; }
   public abstract string? IconResource { get; }
   public abstract int Priority { get; }
   public abstract bool SupportsMultipleTargets { get; }
   public abstract bool CanEdit(object[] targets, Enum? prop);
   public abstract void Reset();
   public abstract void ResetFor(object[] targets);
   public abstract FrameworkElement GetEditorControl();
   public abstract IEnumerable<MenuItem> GetContextMenuActions();
}

public class ProvinceEditor : LocationCollectionSpecializedEditor
{
   private bool _wasValidated;
   public override bool Enabled { get; set; } = true;
   public override string DisplayName => "Province Children Editor";
   public override string? IconResource => null;
   public override int Priority => 10;
   public override bool SupportsMultipleTargets => false;

   public ProvinceEditor()
   {
      //TODO: @Melco - Unsubscribe on disable
      FileStateManager.FileChanged += OnFileStateManagerOnFileChanged;
      Selection.SelectionModified += OnSelectionChanged;
   }

   private static void OnSelectionChanged()
   {
      var obj = Selection.GetSelectedLocations;
      if (obj.Count != 1)
      {
         LocationCollectionEditor.Instance.SelectChild(null!);
         return;
      }

      var loc = obj[0];

      LocationCollectionEditor.Instance.SelectChild(loc);
   }

   ~ProvinceEditor()
   {
      FileStateManager.FileChanged -= OnFileStateManagerOnFileChanged;
   }

   public override bool CanEdit(object[] targets, Enum? prop)
   {
      return targets.All(t => t is Province);
   }

   private void OnFileStateManagerOnFileChanged(object? _, FileChangedEventArgs args)
   {
      if (args.FullPath.EndsWith("definitions.txt"))
         _wasValidated = false;
   }

   public override void Reset()
   {
   }

   public override void ResetFor(object[] targets)
   {
      Debug.Assert(targets.Length == 1);
      Debug.Assert(targets[0].GetType() == typeof(Province));
      var target = (Province)targets[0];
      LocationCollectionEditor.Instance.SetLocationCollection(target, target.Locations, this);
   }

   public override FrameworkElement GetEditorControl()
   {
      return LocationCollectionEditor.Instance;
   }

   public override IEnumerable<MenuItem> GetContextMenuActions() => [];
}