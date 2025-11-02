using System.Collections.Specialized;
using System.ComponentModel;

namespace Arcanum.Core.CoreSystems.NUI;

public sealed class ObservableHashSet<T> : HashSet<T>, ISet<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
   public event NotifyCollectionChangedEventHandler? CollectionChanged;
   public event PropertyChangedEventHandler? PropertyChanged;

   public new bool Add(T item)
   {
      var added = base.Add(item);
      if (!added)
         return added;

      OnCollectionChanged(new(NotifyCollectionChangedAction.Add, item));
      OnPropertyChanged(new(nameof(Count)));

      return added;
   }

   public new bool Remove(T item)
   {
      var removed = base.Remove(item);
      if (!removed)
         return removed;

      OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, item));
      OnPropertyChanged(new(nameof(Count)));

      return removed;
   }

   public new void Clear()
   {
      if (Count == 0)
         return;

      base.Clear();
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
      OnPropertyChanged(new(nameof(Count)));
   }

   private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
   {
      CollectionChanged?.Invoke(this, e);
   }

   private void OnPropertyChanged(PropertyChangedEventArgs e)
   {
      PropertyChanged?.Invoke(this, e);
   }

   void ICollection<T>.Add(T item) => Add(item);

   public new void UnionWith(IEnumerable<T> other)
   {
      ArgumentNullException.ThrowIfNull(other);

      var originalCount = Count;
      base.UnionWith(other);

      if (Count == originalCount)
         return;

      OnPropertyChanged(new(nameof(Count)));
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
   }

   public new void IntersectWith(IEnumerable<T> other)
   {
      ArgumentNullException.ThrowIfNull(other);

      var originalCount = Count;
      base.IntersectWith(other);

      if (Count == originalCount)
         return;

      OnPropertyChanged(new(nameof(Count)));
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
   }

   public new void ExceptWith(IEnumerable<T> other)
   {
      ArgumentNullException.ThrowIfNull(other);

      var originalCount = Count;
      base.ExceptWith(other);

      if (Count == originalCount)
         return;

      OnPropertyChanged(new(nameof(Count)));
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
   }

   public new void SymmetricExceptWith(IEnumerable<T> other)
   {
      ArgumentNullException.ThrowIfNull(other);

      var originalCount = Count;
      base.SymmetricExceptWith(other);

      if (Count == originalCount)
         return;

      OnPropertyChanged(new(nameof(Count)));
      OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
   }

   public new bool IsSubsetOf(IEnumerable<T> other) => base.IsSubsetOf(other);
   public new bool IsSupersetOf(IEnumerable<T> other) => base.IsSupersetOf(other);
   public new bool IsProperSupersetOf(IEnumerable<T> other) => base.IsProperSupersetOf(other);
   public new bool IsProperSubsetOf(IEnumerable<T> other) => base.IsProperSubsetOf(other);
   public new bool Overlaps(IEnumerable<T> other) => base.Overlaps(other);
   public new bool SetEquals(IEnumerable<T> other) => base.SetEquals(other);
}