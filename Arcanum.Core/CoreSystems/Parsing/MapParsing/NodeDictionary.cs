using System.Collections;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public class QueueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dictionary;
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _linkedList;

    public QueueDictionary()
    {
        _dictionary = new();
        _linkedList = [];
    }

    public QueueDictionary(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new(comparer);
        _linkedList = [];
    }

    /// <summary>
    /// Gets the number of key-value pairs contained in the QueueDictionary.
    /// </summary>
    public int Count => _dictionary.Count;

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key.</returns>
    public TValue this[TKey key]
    {
        get => _dictionary[key].Value.Value;
        set
        {
            if (_dictionary.TryGetValue(key, out var node))
                node.Value = new(key, value);
            else
                Enqueue(key, value);
        }
    }

    /// <summary>
    /// Adds an object to the end of the QueueDictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    public void Enqueue(TKey key, TValue value)
    {
        if (_dictionary.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists.", nameof(key));

        var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new(key, value));
        _linkedList.AddLast(node);
        _dictionary.Add(key, node);
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the QueueDictionary.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the QueueDictionary.</returns>
    public KeyValuePair<TKey, TValue> Dequeue()
    {
        if (_linkedList.First == null)
            throw new InvalidOperationException("The QueueDictionary is empty.");

        var node = _linkedList.First;
        _linkedList.RemoveFirst();
        _dictionary.Remove(node.Value.Key);
        return node.Value;
    }

    /// <summary>
    /// Returns the object at the beginning of the QueueDictionary without removing it.
    /// </summary>
    /// <returns>The object at the beginning of the QueueDictionary.</returns>
    public KeyValuePair<TKey, TValue> Peek()
    {
        if (_linkedList.First == null)
            throw new InvalidOperationException("The QueueDictionary is empty.");

        return _linkedList.First.Value;
    }

    /// <summary>
    /// Removes the element with the specified key from the QueueDictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
    public bool Remove(TKey key)
    {
        if (!_dictionary.TryGetValue(key, out var node)) return false;
        _dictionary.Remove(key);
        _linkedList.Remove(node);
        return true;
    }

    /// <summary>
    /// Determines whether the QueueDictionary contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the QueueDictionary.</param>
    /// <returns>true if the QueueDictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    /// <summary>
    /// Removes all objects from the QueueDictionary.
    /// </summary>
    public void Clear()
    {
        _dictionary.Clear();
        _linkedList.Clear();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _linkedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}