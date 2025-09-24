using System.Collections;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

// Place this in a static utility class, e.g., EnumerableExtensions.cs
public static class EnumerableExtensions
{
   /// <summary>
   /// Efficiently checks if an IEnumerable has any items without iterating the entire collection.
   /// </summary>
   public static bool HasItems(this IEnumerable source)
   {
      // This is much faster than .Any() for non-ICollection types, as it avoids enumerator allocation.
      if (source is ICollection collection)
         return collection.Count > 0;

      foreach (var _ in source)
         return true; // Found an item, stop immediately.

      return false;
   }
}