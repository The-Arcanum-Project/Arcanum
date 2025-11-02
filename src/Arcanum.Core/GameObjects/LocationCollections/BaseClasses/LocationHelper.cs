namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public static class LocationHelper
{
   public static ILocation? GetFirstParentOfType(this ILocation location, LocationCollectionType type)
   {
      if (location.LcType > type)
         throw new ArgumentException($"Location of type {location.LcType} cannot have a parent of type {type}.");

      if (location.LcType == type)
         return location;

      foreach (var parent in location.Parents)
         return parent.LcType == type ? parent : parent.GetFirstParentOfType(type);

      return null;
   }
}