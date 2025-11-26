using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Arcanum.UI.SpecializedEditors.Util;


public class ObservableCollectionProxy<TSource, TTarget> : ObservableCollectionProxy<TTarget> where TSource : TTarget
{
    private readonly ObservableCollection<TSource> _source;
    
    public ObservableCollectionProxy(ObservableCollection<TSource> source)
    {
        _source = source;
        // Listen for changes in the source collection
        _source.CollectionChanged += OnCollectionChanged;
        // Also forward property changes (like for the Count property)
        ((INotifyPropertyChanged)_source).PropertyChanged += OnPropertyChanged;
    }

    ~ObservableCollectionProxy()
    {
        // Unsubscribe from events to prevent memory leaks
        _source.CollectionChanged -= OnCollectionChanged;
        ((INotifyPropertyChanged)_source).PropertyChanged -= OnPropertyChanged;
    }
    
    public override IEnumerator<TTarget> GetEnumerator()
    {
        return _source.Cast<TTarget>().GetEnumerator();
    }

    public override int Count => _source.Count;

    public override TTarget this[int index] => _source[index];
    
    public override bool TryAdd(TTarget target)
    {
        if(target is not TSource s)
            return false;
        
        _source.Add(s);
        return true;
    }

    public override bool TryInsert(int index, TTarget target)
    {
        if(target is not TSource s)
            return false;
        
        _source.Insert(index, s);
        return true;
    }

    public override bool TryRemove(TTarget target)
    {
        return target is TSource s && _source.Remove(s);
    }

    public override void RemoveAt(int index)
    {
        _source.RemoveAt(index);
    }
}

public abstract class ObservableCollectionProxy<TTarget> : IReadOnlyList<TTarget>, INotifyCollectionChanged,
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
}