using System.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DataStructures;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Continent = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Continent;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;
using SuperRegion = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SuperRegion;

namespace Arcanum.Core.CoreSystems.Selection;

public static class SelectionHelpers
{
   public static IEu5Object FindBiggestFullySelectedParent(Location location)
   {
      var selected = Selection.GetSelectedLocations;
      IEu5Object current = location;

      while (current is IMapInferable and not Continent)
      {
         var parent = GetNextBiggestParentObj(current);
         Debug.Assert(parent is IMapInferable);
         var allChildren = ((IMapInferable)parent).GetRelevantLocations([location]);
         var allSelected = true;
         foreach (var child in allChildren)
            if (!selected.Contains(child))
            {
               allSelected = false;
               break;
            }

         if (!allSelected)
            break;

         current = parent;
      }

      return current;
   }

   public static IEu5Object GetParentOfType(IEu5Object current, Type targetType)
   {
      var curLevel = GetHierarchyLevel(current);
      var targetLevel = GetHierarchyLevel((IEu5Object)EmptyRegistry.Empties[targetType]);

      Debug.Assert(curLevel <= targetLevel);
      if (curLevel == targetLevel)
         return current;

      var curObject = current;
      while (curLevel < targetLevel)
      {
         curObject = GetNextBiggestParentObj(curObject);
         curLevel++;
         if (curLevel == targetLevel && curObject.GetType() == targetType)
            return curObject;
      }

      throw new ArgumentException("No parent of the specified type found");
   }

   public static int GetHierarchyLevel(IEu5Object obj)
   {
      if (obj is Location)
         return 0;
      if (obj is Province)
         return 1;
      if (obj is Area)
         return 2;
      if (obj is Region)
         return 3;
      if (obj is SuperRegion)
         return 4;
      if (obj is Continent)
         return 5;

      throw new ArgumentException("obj is not a valid type in the hierarchy");
   }

   public static IEu5Object GetNextBiggestParentObj(IEu5Object eu5Object)
   {
#if DEBUG
      var type = eu5Object.GetType();
      Debug.Assert(type == typeof(Location) ||
                   type == typeof(Province) ||
                   type == typeof(Area) ||
                   type == typeof(Region) ||
                   type == typeof(SuperRegion) ||
                   type == typeof(Continent));
#endif
      if (eu5Object is Location loc)
         return loc.Province;
      if (eu5Object is Province prov)
         return prov.Area;
      if (eu5Object is Area area)
         return area.Region;
      if (eu5Object is Region region)
         return region.SuperRegion;
      if (eu5Object is SuperRegion sRegion)
         return sRegion.Continent;

      throw new ArgumentException("current is not a valid parent type");
   }

   public static Enum GetParentEnumFromChildrenEnum(Enum children)
   {
      return children switch
      {
         Province.Field.Locations => Location.Field.Province,
         Area.Field.Provinces => Province.Field.Area,
         Region.Field.Areas => Area.Field.Region,
         SuperRegion.Field.Regions => Region.Field.SuperRegion,
         Continent.Field.SuperRegions => SuperRegion.Field.Continent,
         _ => throw new ArgumentException("children is not a valid children enum"),
      };
   }

   public static Enum GetChildEnum(IEu5Object obj)
   {
      switch (obj)
      {
         case Province:
            return Province.Field.Locations;
         case Area:
            return Area.Field.Provinces;
         case Region:
            return Region.Field.Areas;
         case SuperRegion:
            return SuperRegion.Field.Regions;
         case Continent:
            return Continent.Field.SuperRegions;
         default:
            Debug.Fail("GetChildEnum called with invalid type");
            throw new ArgumentException("obj is not a valid type in the hierarchy");
      }
   }

   public static AggregateLink<T> GetAllChildren<T>(IEu5Object eu5Object) where T : IEu5Object
   {
#if DEBUG
      var type = eu5Object.GetType();
      Debug.Assert(type == typeof(Province) ||
                   type == typeof(Area) ||
                   type == typeof(Region) ||
                   type == typeof(SuperRegion) ||
                   type == typeof(Continent));
#endif

      if (eu5Object is Province prov)
         return (prov.Locations as AggregateLink<T>)!;
      if (eu5Object is Area area)
         return (area.Provinces as AggregateLink<T>)!;
      if (eu5Object is Region region)
         return (region.Areas as AggregateLink<T>)!;
      if (eu5Object is SuperRegion sRegion)
         return (sRegion.Regions as AggregateLink<T>)!;
      if (eu5Object is Continent continent)
         return (continent.SuperRegions as AggregateLink<T>)!;

      throw new ArgumentException("eu5Object is not a valid type in the hierarchy");
   }

   public static IEu5Object? ShrintToNextChild(IEu5Object bp)
   {
      if (bp is Location loca)
         return loca;

      var selection = Selection.GetSelectedLocations;
      if (bp is Province prov)
         foreach (var loc in prov.Locations)
            if (selection.Contains(loc))
               return loc;

      if (bp is Area area)
         foreach (var province in area.Provinces)
            if (((IMapInferable)province).GetRelevantLocations([province]).All(x => selection.Contains(x)))
               return province;

      if (bp is Region region)
         foreach (var areaa in region.Areas)
            if (((IMapInferable)areaa).GetRelevantLocations([areaa]).All(x => selection.Contains(x)))
               return areaa;

      if (bp is SuperRegion sRegion)
         foreach (var regionn in sRegion.Regions)
            if (((IMapInferable)regionn).GetRelevantLocations([regionn]).All(x => selection.Contains(x)))
               return regionn;

      return null;
   }
}