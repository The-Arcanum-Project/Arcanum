using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.DataStructures;
using Arcanum.UI.SpecializedEditors.EditorControls;
using Arcanum.UI.SpecializedEditors.Management;
using Common;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;

namespace Arcanum.UI.SpecializedEditors.Editors;

public class LocationCollectionSpecializedEditor<TChild, TParent> : ISpecializedEditor where TParent : IEu5Object where TChild : IEu5Object
{
    private bool _wasValidated = true;
    public bool Enabled { get; set; } = true;
    public string? IconResource => null;
    public int Priority => 10;
    public bool SupportsMultipleTargets => false;
    public string DisplayName => $"{typeof(TParent).Name} Children Editor";

    private Enum _targetedProperty;
    
    public LocationCollectionSpecializedEditor(Enum targetedProperty)
    {
        _targetedProperty = targetedProperty;
        FileStateManager.FileChanged += OnFileStateManagerOnFileChanged;
        Selection.SelectionModified += OnSelectionChanged;
    }

    ~LocationCollectionSpecializedEditor()
    {
        FileStateManager.FileChanged -= OnFileStateManagerOnFileChanged;
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

    private void OnFileStateManagerOnFileChanged(object? _, FileChangedEventArgs args)
    {
        if (args.FullPath.EndsWith("definitions.txt"))
            _wasValidated = false;
    }

    public bool CanEdit(object[] targets, Enum? prop)
    {
        return _wasValidated && targets.All(t => t is TParent);
    }

    public void Reset()
    {
    }

    public void ResetFor(object[] targets)
    {
        Debug.Assert(targets.Length == 1);
        Debug.Assert(targets[0].GetType() == typeof(TParent));
        var target = (TParent)targets[0];
        LocationCollectionEditor.Instance.SetLocationCollection(target, (AggregateLink<TChild>)target._getValue(_targetedProperty), this);
    }

    public FrameworkElement GetEditorControl() => LocationCollectionEditor.Instance;


    public IEnumerable<MenuItem> GetContextMenuActions() => [];
}