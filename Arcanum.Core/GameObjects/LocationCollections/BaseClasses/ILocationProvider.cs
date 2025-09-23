using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public interface ILocationCollection<T> where T : ILocation
{
   [SuppressAgs]
   [AddModifiable]
   public ObservableRangeCollection<T> LocationChildren { get; set; }
}

public interface ILocation : IEu5Object
{
   public ICollection<Location> GetLocations();
   public LocationCollectionType LcType { get; }

   [SuppressAgs]
   [AddModifiable]
   public ObservableRangeCollection<ILocation> Parents { get; set; }
}