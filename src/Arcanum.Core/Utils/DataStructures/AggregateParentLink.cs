using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.Core.Utils.DataStructures;

public sealed class AggregateParentLink<T> : ObservableRangeCollection<T> where T : IEu5Object
{
   public readonly Enum NxOwnerProp;
   public readonly IEu5Object Owner;

   public bool Lock = false;

   public AggregateParentLink(Enum nxOwnerProp, IEu5Object owner)
   {
      NxOwnerProp = nxOwnerProp;
      Owner = owner;
      Debug.Assert(typeof(T).GetProperty(NxOwnerProp.ToString()) != null,
                   $"The items being added do not contain the owner property {NxOwnerProp}.");
      CollectionChanged += AggregateCollection_CollectionChanged;
   }

   private void AggregateCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems == null && e.OldItems == null)
         return;
#if DEBUG
      if (e.NewItems != null)
         Debug.Assert(e.NewItems is IList<T>,
                      "NewItems is not of the expected type IList<T>.");
      if (e.OldItems != null)
         Debug.Assert(e.OldItems is IList<T>,
                      "OldItems is not of the expected type IList<T>.");
#endif

      switch (e.Action)
      {
         case NotifyCollectionChangedAction.Add:
            ItemsAdded((List<T>)e.NewItems!);
            break;

         case NotifyCollectionChangedAction.Remove:
            ItemsRemoved((List<T>)e.OldItems!);
            break;
         case NotifyCollectionChangedAction.Replace:
            ItemsRemoved((List<T>)e.OldItems!);
            ItemsAdded((List<T>)e.NewItems!);
            break;
         case NotifyCollectionChangedAction.Reset:
            ItemsRemoved((List<T>)Items);
            break;
      }
   }

   private void ItemsAdded(List<T> newItems)
   {
      if (newItems.Count == 0)
         return;

      var empty = EmptyRegistry.Empties[Owner.GetType()];
      foreach (var item in newItems)
      {
         Debug.Assert(!Equals(item._getValue(NxOwnerProp), empty));
         item._setValue(NxOwnerProp, Owner);
      }
   }

   private void ItemsRemoved(List<T> oldItems)
   {
      if (oldItems.Count == 0)
         return;

      var empty = EmptyRegistry.Empties[Owner.GetType()];
      if(Lock) return;
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
      if (Lock) return;
      Lock = true;
      Remove(child);
      Lock = false;
   }

   public void _addFromChild(T child)
   {
      if (Lock) return;
      Lock = true;
      Add(child);
      Lock = false;
   }
}