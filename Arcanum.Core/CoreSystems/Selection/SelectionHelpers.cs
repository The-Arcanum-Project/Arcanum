using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Selection;

public static class SelectionHelpers
{
   public static ILocation? FindBiggestFullySelectedParent(Location location)
   {
      var selected = Selection.GetSelectedLocations;
      ILocation current = location;

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

   public static ILocation? FindParentToShrinkTo(Location location)
   {
      var bp = FindBiggestFullySelectedParent(location);
      return bp == null ? null : GetChildContainingLocation(bp, location);
   }

   private static ILocation? GetChildContainingLocation(ILocation loc, Location child)
   {
      return loc.LcType switch
      {
         LocationCollectionType.Continent => ((Continent)loc).LocationChildren
                                                             .FirstOrDefault(sr => sr.GetLocations().Contains(child)),
         LocationCollectionType.SuperRegion => ((SuperRegion)loc).LocationChildren
                                                                 .FirstOrDefault(r => r.GetLocations().Contains(child)),
         LocationCollectionType.Region => ((Region)loc).LocationChildren
                                                       .FirstOrDefault(a => a.GetLocations().Contains(child)),
         LocationCollectionType.Area => ((Area)loc).LocationChildren
                                                   .FirstOrDefault(p => p.GetLocations().Contains(child)),
         LocationCollectionType.Province => ((Province)loc).LocationChildren
                                                           .FirstOrDefault(l => l == child),
         _ => null,
      };
   }

   private static ILocation? GetNextParentType(ILocation loc)
   {
      return loc.LcType switch
      {
         LocationCollectionType.Location => loc.GetFirstParentOfType(LocationCollectionType.Province),
         LocationCollectionType.Province => loc.GetFirstParentOfType(LocationCollectionType.Area),
         LocationCollectionType.Area => loc.GetFirstParentOfType(LocationCollectionType.Region),
         LocationCollectionType.Region => loc.GetFirstParentOfType(LocationCollectionType.SuperRegion),
         LocationCollectionType.SuperRegion => loc.GetFirstParentOfType(LocationCollectionType.Continent),
         _ => null,
      };
   }
}