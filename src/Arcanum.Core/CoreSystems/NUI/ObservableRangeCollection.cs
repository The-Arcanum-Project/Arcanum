using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arcanum.Core.Utils.Pools;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// The <see cref="ObservableRangeCollection{T}"/> class is an extension of <see cref="ObservableCollection{T}"/>
/// that provides methods to add or replace a range of items with a single notification to observers.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
   public bool IsDistinct { get; set; }

   public ObservableRangeCollection()
   {
   }

   public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
   {
   }

   public List<T>? UnderlyingList => Items as List<T>;

   /// <summary>
   /// Adds a collection of items and raises a single notification.
   /// </summary>
   public void AddRange(IEnumerable<T> range)
   {
      using var _ = ListPool<T>.Get(out var toAdd);
      foreach (var item in range)
      {
         if (IsDistinct && Items.Contains(item))
            continue;

         toAdd.Add(item); // Add to the internal list without raising events
         Items.Add(item);
      }

      // Raise a single "Reset" event to tell the UI to refresh itself completely.
      OnCollectionChanged(new(NotifyCollectionChangedAction.Add, toAdd));
   }

   public void RemoveRange(IEnumerable<T> range)
   {
      using var _ = ListPool<T>.Get(out var toRemove);
      foreach (var item in range)
         if (Items.Remove(item))
            toRemove.Add(item);

      if (toRemove.Count > 0)
         OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, toRemove));
   }

   /// <summary>
   /// Clears the collection and adds a new collection of items, raising a single notification.
   /// </summary>
   public void ClearAndAdd(IEnumerable<T> range)
   {
      using var _ = ListPool<T>.Get(out var oldItems);
      oldItems.AddRange(Items);
      Items.Clear();
      foreach (var t in range)
         Items.Add(t);
      OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, range, oldItems));
   }
}