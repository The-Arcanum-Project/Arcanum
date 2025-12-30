using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;

namespace Arcanum.UI.SpecializedEditors.EditorControls.ViewModels;

public class LocationCollectionVm<T> where T : ILocation
{
   public ILocationCollection<T> LocationCollection { get; }

   public LocationCollectionVm(ILocationCollection<T> locationCollection)
   {
      LocationCollection = locationCollection;
   }

   public ObservableRangeCollection<T> Children => LocationCollection.LocationChildren;
}