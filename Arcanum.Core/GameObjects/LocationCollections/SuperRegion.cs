using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class SuperRegion : LocationCollection<Region>
{
   public SuperRegion(string name, ICollection<Region> provinces) : base(name, provinces)
   {
   }

   public SuperRegion(string name) : base(name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.SuperRegion;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }
}