using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.Registry;
using Arcanum.UI.SpecializedEditors.Editors;
using Arcanum.UI.SpecializedEditors.Util;

namespace Arcanum.UI.SpecializedEditors.EditorControls;

public partial class LocationCollectionEditor
{
   private static readonly Lazy<LocationCollectionEditor> LazyInstance = new(() => new());
   private bool _ignoreSelectionChanged;
   public static LocationCollectionEditor Instance => LazyInstance.Value;

   private ICollection? _parentCache;

   private LocationCollectionSpecializedEditor _editor = null!;

   public static readonly DependencyProperty LocationCollectionProperty =
      DependencyProperty.Register(nameof(LocationCollection),
                                  typeof(ObservableCollectionProxy<ILocation>),
                                  typeof(LocationCollectionEditor),
                                  new(default(ObservableCollectionProxy<ILocation>)));

   public ObservableCollectionProxy<ILocation> LocationCollection
   {
      get => (ObservableCollectionProxy<ILocation>)GetValue(LocationCollectionProperty);
      set => SetValue(LocationCollectionProperty, value);
   }

   public void SetLocationCollection<T>(ILocationCollection<T> locationCollection,
                                        LocationCollectionSpecializedEditor editor) where T : ILocation
   {
      _editor = editor;
      if (_parentCache is not Collection<T>)
      {
         CollectionSelector.FullItemsSource = _parentCache =
                                                 ((ILocationCollection<T>)EmptyRegistry.Empties[locationCollection
                                                      .GetType()]).GetGlobalItemsNonGeneric()
                                                                  .Values;
      }

      SelectLocation(locationCollection, true);
        LocationSelector.FullItemsSource = ((T)EmptyRegistry.Empties[typeof(T)]).GetGlobalItemsNonGeneric().Values;

      // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
      LocationCollection?.Dispose();
      // TODO: Fill LocationSelector with child objects
      LocationCollection = new ObservableCollectionProxy<T, ILocation>(locationCollection.LocationChildren);
   }

   public void SelectLocation(ILocation location, bool ignoreSelectionChanged = false)
   {
      _ignoreSelectionChanged = ignoreSelectionChanged;
      CollectionSelector.SelectedItem = location;
      _ignoreSelectionChanged = false;
   }

   public LocationCollectionEditor()
   {
      InitializeComponent();
   }

   private void CollectionSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (_ignoreSelectionChanged || CollectionSelector.SelectedItem is not ILocation selectedLocation)
         return;

      _editor.ResetFor([selectedLocation]);
   }

    private void Location_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Get the location and remove it from the collection
        if (sender is not FrameworkElement { DataContext: ILocation location }) return;
        
        LocationCollection.TryRemove(location);
    }
}