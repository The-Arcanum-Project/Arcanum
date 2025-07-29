using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Region : LocationCollection<Area>
{
   public Region(string name, ICollection<Area> provinces) : base(name, provinces)
   {
   }

   public Region(string name) : base(name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.Region;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }
}