using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Arcanum.Core.CoreSystems.NUI;

public class ObservableRangeCollection<T> : ObservableCollection<T>
{
   public ObservableRangeCollection()
   {
   }

   public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
   {
   }

   /// <summary>
   /// Adds a collection of items and raises a single notification.
   /// </summary>
   public void AddRange(IEnumerable<T> range)
   {
      foreach (var item in range)
         Items.Add(item); // Add to the internal list without raising events

      // Raise a single "Reset" event to tell the UI to refresh itself completely.
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
   }

   /// <summary>
   /// Clears the collection and adds a new collection of items, raising a single notification.
   /// </summary>
   public void ReplaceRange(IEnumerable<T> range)
   {
      Items.Clear(); 
      AddRange(range);
   }
}