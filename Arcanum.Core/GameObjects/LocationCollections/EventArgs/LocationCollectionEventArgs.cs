using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections.EventArgs;

public class LocationCollectionEventArgs<T>(T composite, bool isAdded) : System.EventArgs
   where T : LocationComposite
{
   public T Composite { get; } = composite;
   public bool IsAdded { get; } = isAdded;
}