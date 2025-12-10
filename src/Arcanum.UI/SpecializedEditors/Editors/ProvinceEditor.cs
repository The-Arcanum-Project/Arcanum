using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.Analytics.MapData;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.SpecializedEditors.EditorControls;
using Arcanum.UI.SpecializedEditors.Management;
using Common;
using Common.UI;

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
      FileStateManager.FileChanged += OnFileStateManagerOnFileChanged;
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
      LocationCollectionEditor.Instance.SetLocationCollection(target, target.Locations ,this);
   }

   public override FrameworkElement GetEditorControl()
   {
      return LocationCollectionEditor.Instance;
   }

   public override IEnumerable<MenuItem> GetContextMenuActions() => [];
}