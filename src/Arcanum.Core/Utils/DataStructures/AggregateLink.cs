using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.Pools;

namespace Arcanum.Core.Utils.DataStructures;

public interface IAggregateLink : IList
{
   public Enum NxOwnerProp { get; }
   public IEu5Object Owner { get; }
   public Enum NxChildsProp { get; }
}

public sealed class AggregateLink<T> : ObservableRangeCollection<T>, IAggregateLink where T : IEu5Object
{
   
   public Enum NxOwnerProp { get; }
   public IEu5Object Owner { get; }
   public Enum NxChildsProp { get; }
   
   public bool Lock;

   public AggregateLink(Enum nxOwnerProp, Enum nxChildsProp, IEu5Object owner)
   {
      NxChildsProp = nxChildsProp;
      NxOwnerProp = nxOwnerProp;
      Owner = owner;
      Debug.Assert(typeof(T).GetProperty(NxOwnerProp.ToString()) != null,
                   $"The items being added do not contain the owner property {NxOwnerProp}.");
      CollectionChanged += AggregateCollection_CollectionChanged;
   }

   private void AggregateCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      switch (e.Action)
      {
         case NotifyCollectionChangedAction.Add:
         {
            using var _ = ListPool<T>.EnumerateTo(e.NewItems!, out var newItems);
            ItemsAdded(newItems);
            break;
         }

         case NotifyCollectionChangedAction.Remove:
         {
            using var _ = ListPool<T>.EnumerateTo(e.OldItems!, out var oldItems);
            ItemsRemoved(oldItems);
            break;
         }
         case NotifyCollectionChangedAction.Replace:
         {
            using var __ = ListPool<T>.EnumerateTo(e.OldItems!, out var oldItems);
            ItemsRemoved(oldItems);

            using var _ = ListPool<T>.EnumerateTo(e.NewItems!, out var newItems);
            ItemsAdded(newItems);
            break;
         }
         case NotifyCollectionChangedAction.Reset:
         {
            using var _ = ListPool<T>.EnumerateTo(Items, out var oldItems);
            ItemsRemoved(oldItems);
            break;
         }
      }
   }

   private void ItemsAdded(List<T> newItems)
   {
      if (newItems.Count == 0 || Lock)
         return;

      Lock = true;
      foreach (var item in newItems)
      {
         var oldOwner = (IEu5Object)item._getValue(NxOwnerProp);
         if (!IEu5Object.IsEmpty(oldOwner))
            ((AggregateLink<T>)((IEu5Object)item._getValue(NxOwnerProp))._getValue(NxChildsProp)).Remove(item);

         item._setValue(NxOwnerProp, Owner);
      }

      Lock = false;
   }

   private void ItemsRemoved(List<T> oldItems)
   {
      if (oldItems.Count == 0 || Lock)
         return;

      var empty = EmptyRegistry.Empties[Owner.GetType()];

      Lock = true;
      foreach (var item in oldItems)
      {
         Debug.Assert(!Equals(item._getValue(NxOwnerProp), empty));
         item._setValue(NxOwnerProp, empty);
      }

      Lock = false;
   }
}