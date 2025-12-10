using System.Diagnostics;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.Registry;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Selection;

public static class SelectionHelpers
{
   public static IEu5Object? FindBiggestFullySelectedParent(Location location)
   {
      var selected = Selection.GetSelectedLocations;
      IEu5Object current = location;

      while (current.Parents.Count > 0)
      {
         var nextParent = GetNextParentType(current);
         if (nextParent == null)
            break;

         var selectionCopy = selected.ToList();
         foreach (var loc in nextParent.GetLocations())
            selectionCopy.Remove(loc);

         if (selectionCopy.Count == 0)
            current = current.Parents.First();
         else
            return null;
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

   public static IEu5Object GetNextBiggestParentObj(IEu5Object current)
   {
      if (current is Location loc)
         return loc.Province;
      if (current is Province prov)
         return prov.Area;
      if (current is Area area)
         return area.Region;
      if (current is Region region)
         return region.SuperRegion;
      if (current is SuperRegion sRegion)
         return sRegion.Continent;

      throw new ArgumentException("current is not a valid parent type");
   }

   public static List<IEu5Object> GetAllChildren(IEu5Object eu5Object)
   {
      if (eu5Object is Location loc)
         return [loc];
      if (eu5Object is Province prov)
         return prov.Locations.Cast<IEu5Object>().ToList();
      if (eu5Object is Area area)
         return area.Provinces.Cast<IEu5Object>().ToList();
      if (eu5Object is Region region)
         return region.Areas.Cast<IEu5Object>().ToList();
      if (eu5Object is SuperRegion sRegion)
         return sRegion.Regions.Cast<IEu5Object>().ToList();
      if (eu5Object is Continent continent)
         return continent.SuperRegions.Cast<IEu5Object>().ToList();

      throw new ArgumentException("eu5Object is not a valid type in the hierarchy");
   }

   public static ILocation? FindParentToShrinkTo(Location location)
   {
      var bp = FindBiggestFullySelectedParent(location);
      return bp == null ? null : GetChildContainingLocation(bp, location);
   }

   // TODO: Make this use the new system with aggregate links and such
   private static ILocation? GetChildContainingLocation(ILocation loc, Location child)
   {
      return loc.LcType switch
      {
         _ => null,
      };
   }

   private static ILocation? GetNextParentType(ILocation loc)
   {
      return loc.LcType switch
      {
         _ => null,
      };
   }
}