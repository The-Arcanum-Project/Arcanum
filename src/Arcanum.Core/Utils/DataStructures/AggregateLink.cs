using System.Collections.Specialized;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.Pools;

namespace Arcanum.Core.Utils.DataStructures;

public sealed class AggregateLink<T> : ObservableRangeCollection<T> where T : IEu5Object
{
   public readonly Enum NxOwnerProp;
   public readonly IEu5Object Owner;
   public readonly Enum NxChildsProp;

   public bool Lock = false;

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

      var empty = EmptyRegistry.Empties[Owner.GetType()];

      Lock = true;
      foreach (var item in newItems)
      {
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

   public void _removeFromChild(T child)
   {
      if (Lock)
         return;

      Lock = true;
      Nx.RemoveFromCollection(Owner, NxChildsProp, child);
      Lock = false;
   }

   public void _addFromChild(T child)
   {
      if (Lock)
         return;

      Lock = true;
      Nx.AddToCollection(Owner, NxChildsProp, child);
      Lock = false;
   }
}