using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

namespace Arcanum.Core.GameObjects.LocationCollections;

public class SuperRegion : LocationCollection<Region>
{
   public SuperRegion(FileInformation fileInfo, string name, ICollection<Region> provinces) : base(fileInfo, name, provinces)
   {
   }

   public SuperRegion(FileInformation fileInfo, string name) : base(fileInfo, name)
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