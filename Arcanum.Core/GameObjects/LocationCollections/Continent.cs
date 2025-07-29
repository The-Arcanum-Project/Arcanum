using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Continent : LocationCollection<SuperRegion>
{
   public Continent(string name, ICollection<SuperRegion> provinces) : base(name, provinces)
   {
   }

   public Continent(string name) : base(name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.Continent;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }
}