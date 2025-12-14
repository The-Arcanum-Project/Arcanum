using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DataStructures;
using Arcanum.UI.SpecializedEditors.Editors;
using Arcanum.UI.SpecializedEditors.Util;

namespace Arcanum.UI.SpecializedEditors.EditorControls;

public partial class LocationCollectionEditor
{
    private static readonly Lazy<LocationCollectionEditor> LazyInstance = new(() => new());
    private bool _ignoreSelectionChanged;
    public static LocationCollectionEditor Instance => LazyInstance.Value;

    private ICollection? _parentCache;

    private Type _currentChildType = null!;
    private Type _currentParentType = null!;

    private LocationCollectionSpecializedEditor _editor = null!;

    public static readonly DependencyProperty LocationCollectionProperty =
        DependencyProperty.Register(nameof(LocationCollection),
            typeof(ObservableCollectionProxy<IEu5Object>),
            typeof(LocationCollectionEditor),
            new(default(ObservableCollectionProxy<IEu5Object>)));

    public ObservableCollectionProxy<IEu5Object> LocationCollection
    {
        get => (ObservableCollectionProxy<IEu5Object>)GetValue(LocationCollectionProperty);
        set => SetValue(LocationCollectionProperty, value);
    }

    public void SetLocationCollection<T>(IEu5Object locationCollection, AggregateLink<T> children,
        LocationCollectionSpecializedEditor editor) where T : IEu5Object
    {
        _editor = editor;
        if (_parentCache is not Collection<T>)
        {
            CollectionSelector.FullItemsSource = _parentCache =
                ((IEu5Object)EmptyRegistry.Empties[locationCollection
                    .GetType()]).GetGlobalItemsNonGeneric()
                .Values;
        }
        _currentParentType = locationCollection.GetType();
        _currentChildType = typeof(T);

        SelectLocation(locationCollection, true);
        LocationSelector.FullItemsSource = ((T)EmptyRegistry.Empties[typeof(T)]).GetGlobalItemsNonGeneric().Values;

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        LocationCollection?.Dispose();
        // TODO: Fill LocationSelector with child objects
        LocationCollection = new ObservableCollectionProxy<T, IEu5Object>(children, locationCollection);
    }

    public void SelectLocation(IEu5Object location, bool ignoreSelectionChanged = false)
    {
        _ignoreSelectionChanged = ignoreSelectionChanged;
        CollectionSelector.SelectedItem = location;
        _ignoreSelectionChanged = false;
    }

    public void SelectChild(IEu5Object child, bool ignoreSelectionChanged = false)
    {
        _ignoreSelectionChanged = ignoreSelectionChanged;
        LocationSelector.SelectedItem = child;
        _ignoreSelectionChanged = false;
    }


    public LocationCollectionEditor()
    {
        InitializeComponent();
    }

    private void CollectionSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_ignoreSelectionChanged || CollectionSelector.SelectedItem is not IEu5Object selectedLocation)
            return;

        _editor.ResetFor([selectedLocation]);
    }

    private void Location_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Get the location and remove it from the collection
        if (sender is not FrameworkElement { DataContext: IEu5Object location }) return;

        LocationCollection.TryRemove(location);
    }

    private void Add_OnClick(object sender, RoutedEventArgs e)
    {
        if (LocationSelector.SelectedItem is not IEu5Object selectedLocation)
        {
            var parent = Selection.GetSelectedLocations
                .Select(location => SelectionHelpers.GetParentOfType(location, _currentChildType))
                .Where(parent => !LocationCollection.Contains(parent)).ToArray();
            LocationCollection.TryAddRange(parent);


            return;
        }

        LocationCollection.TryAdd(selectedLocation);
    }

    private void Remove_OnClick(object sender, RoutedEventArgs e)
    {
        if (LocationSelector.SelectedItem is not IEu5Object selectedLocation)
        {
            var parent = Selection.GetSelectedLocations
                .Select(location => SelectionHelpers.GetParentOfType(location, _currentChildType))
                .Where(parent => LocationCollection.Contains(parent)).ToArray();
            
            LocationCollection.TryRemoveRange(parent);


            return;
        }

        LocationCollection.TryRemove(selectedLocation);
    }
}