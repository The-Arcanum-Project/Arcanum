using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Province : LocationCollection<Location>
{
   public Province(string name, ICollection<Location> provinces) : base(name, provinces)
   {
   }

   public Province(string name) : base(name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.Province;
   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }
}