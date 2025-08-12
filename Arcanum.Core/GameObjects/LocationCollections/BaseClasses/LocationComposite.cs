using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public abstract class LocationComposite(string name, FileInformation information) : ISaveable // TODO: @Melco @Minnator implement ISaveable here
{
   public string Name { get; } = name;
   public List<LocationComposite> Parents { get; } = [];
   public abstract ICollection<Location> GetLocations();
   public abstract LocationCollectionType LCType { get; }

   public virtual LocationComposite GetFirstParentOfType(LocationCollectionType type)
   {
      foreach (var parent in Parents)
      {
         if (parent.LCType == type)
            return parent;

         var recursiveParent = parent.GetFirstParentOfType(type);
         if (recursiveParent != Empty)
            return recursiveParent;
      }

      return Empty;
   }

   public override bool Equals(object? obj)
   {
      if (obj is LocationComposite other)
         return Name == other.Name;

      return false;
   }

   public override int GetHashCode() => Name.GetHashCode();
   public override string ToString() => Name;

   public static bool operator ==(LocationComposite? left, LocationComposite? right)
   {
      if (left is null)
         return right is null;

      return left.Equals(right);
   }

   public static bool operator !=(LocationComposite? left, LocationComposite? right) => !(left == right);

   public static LocationComposite Empty { get; } = Location.Empty;

   public FileInformation FileInformation { get; } = information;
   public SaveableType SaveType { get; } = SaveableType.Location;
}