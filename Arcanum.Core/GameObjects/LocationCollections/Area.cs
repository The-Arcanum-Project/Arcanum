using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class Area : LocationCollection<Province>
{
   public Area(string name, ICollection<Province> provinces) : base(name, provinces)
   {
   }

   public Area(string name) : base(name)
   {
   }

   public override LocationCollectionType LCType { get; } = LocationCollectionType.Area;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }
}