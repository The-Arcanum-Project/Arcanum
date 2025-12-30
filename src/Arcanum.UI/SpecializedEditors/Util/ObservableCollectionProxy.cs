using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.UI.SpecializedEditors.Util;

public class ObservableCollectionProxy<TSource, TTarget> : ObservableCollectionProxy<TTarget>, IDisposable
   where TSource : TTarget
{
   private readonly ObservableCollection<TSource> _source;
   private readonly IEu5Object _owner;

   public ObservableCollectionProxy(ObservableCollection<TSource> source, IEu5Object owner)
   {
      _source = source;
      _owner = owner;
      // Listen for changes in the source collection
      _source.CollectionChanged += OnCollectionChanged;
      // Also forward property changes (like for the Count property)
      ((INotifyPropertyChanged)_source).PropertyChanged += OnPropertyChanged;
   }

   public override IEnumerator<TTarget> GetEnumerator()
   {
      return _source.Cast<TTarget>().GetEnumerator();
   }

   public override int Count => _source.Count;

   public override TTarget this[int index]
   {
      get
      {
         Debug.Fail("I don't think this is needed");
         return _source[index];
      }
   }

   public override bool TryAdd(TTarget target)
   {
      if (target is not TSource s)
         return false;

      Nx.AddToCollection(_owner, SelectionHelpers.GetChildEnum(_owner), s);
      return true;
   }

   public override bool TryInsert(int index, TTarget target)
   {
      Debug.Fail("I don't think this is needed");
      if (target is not TSource s)
         return false;

      _source.Insert(index, s);
      return true;
   }

   public override bool TryRemove(TTarget target)
   {
      var result = target is TSource;
      if (result)
         Nx.RemoveFromCollection(_owner, SelectionHelpers.GetChildEnum(_owner), (TSource)target!);
      return result;
   }

   public override void RemoveAt(int index)
   {
      Nx.RemoveFromCollection(_owner, SelectionHelpers.GetChildEnum(_owner), _source[index]);
   }

   public new void Dispose()
   {
      _source.CollectionChanged -= OnCollectionChanged;
      ((INotifyPropertyChanged)_source).PropertyChanged -= OnPropertyChanged;
      GC.SuppressFinalize(this);
   }

   public override bool TryAddRange(IList<IEu5Object> parent)
   {
      if (parent.Count <= 1 || parent[0] is not TSource)
         return false;

      Nx.AddRangeToCollection(_owner, SelectionHelpers.GetChildEnum(_owner), parent);
      return true;
   }

   public override bool TryRemoveRange(IList<IEu5Object> parent)
   {
      if (parent.Count <= 1 || parent[0] is not TSource)
         return false;

      Nx.RemoveRangeFromCollection(_owner, SelectionHelpers.GetChildEnum(_owner), parent);
      return true;
   }
}

public abstract class ObservableCollectionProxy<TTarget>
   : IReadOnlyList<TTarget>,
     IDisposable,
     INotifyCollectionChanged,
     INotifyPropertyChanged
{
   public event NotifyCollectionChangedEventHandler? CollectionChanged;
   public event PropertyChangedEventHandler? PropertyChanged;

   protected void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
   {
      CollectionChanged?.Invoke(sender, e!);
   }

   protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs? e)
   {
      PropertyChanged?.Invoke(sender, e!);
   }

   public abstract IEnumerator<TTarget> GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator()
   {
      return GetEnumerator();
   }

   public abstract int Count { get; }
   public abstract TTarget this[int index] { get; }

   public abstract bool TryAdd(TTarget target);

   public abstract bool TryInsert(int index, TTarget target);

   public abstract bool TryRemove(TTarget target);

   public abstract void RemoveAt(int index);

   public void Dispose()
   {
      GC.SuppressFinalize(this);
   }

   public abstract bool TryAddRange(IList<IEu5Object> parent);
   public abstract bool TryRemoveRange(IList<IEu5Object> parent);
}