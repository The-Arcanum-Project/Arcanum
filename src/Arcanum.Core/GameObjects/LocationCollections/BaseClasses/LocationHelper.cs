using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.GameObjects.LocationCollections.BaseClasses;

public static class LocationHelper
{
   public static IEu5Object? GetFirstParentOfType(this Location location, LocationCollectionType type)
   {
      return type switch
      {
         LocationCollectionType.Location => location,
         LocationCollectionType.Province => location.Province,
         LocationCollectionType.Area => location.Province.Area,
         LocationCollectionType.Region => location.Province.Area.Region,
         LocationCollectionType.SuperRegion => location.Province.Area.Region.SuperRegion,
         LocationCollectionType.Continent => location.Province.Area.Region.SuperRegion.Continent,
         _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
   }
}