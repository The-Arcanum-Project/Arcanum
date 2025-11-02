using System.Collections.ObjectModel;

namespace Arcanum.UI.Saving.Backend;

public class SortedObservableCollection<T>(ICollection<T> data) : ObservableCollection<T>(data)
{
   public void InsertSorted(T item)
   {
      var index = BinarySearch(item);
      if (index < 0)
         index = ~index; // Bitwise complement: insertion point
      Insert(index, item);
   }

   public bool TryInsertSorted(T item, IComparer<T>? comparer = null)
   {
      var index = BinarySearch(item, comparer);
      if (index >= 0)
         return false; // Item already exists

      index = ~index; // Bitwise complement: insertion point
      Insert(index, item);
      return true;
   }

   public int BinarySearch(T item, IComparer<T>? comparer = null)
   {
      comparer ??= Comparer<T>.Default;
      int lo = 0,
          hi = Count - 1;
      while (lo <= hi)
      {
         var mid = lo + (hi - lo) / 2;
         var cmp = comparer.Compare(this[mid], item);
         switch (cmp)
         {
            case 0:
               return mid;
            case < 0:
               lo = mid + 1;
               break;
            default:
               hi = mid - 1;
               break;
         }
      }

      return ~lo;
   }
}