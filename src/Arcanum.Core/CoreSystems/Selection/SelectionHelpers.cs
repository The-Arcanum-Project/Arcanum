using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
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