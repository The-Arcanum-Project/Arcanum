namespace Arcanum.Core.DataStructures;

/// <summary>
/// A compact lookup structure optimized for memory usage and fast lookups.
/// </summary>
public class CompactLookup<TKey, TValue>(int capacity) where TKey : IComparable<TKey>
{
   private TKey[] _keys = new TKey[capacity];
   private TValue[] _values = new TValue[capacity];
   private int _count;
   private bool _isFrozen;

   // --- INITIALIZATION PHASE ---

   public void Add(TKey key, TValue value)
   {
      if (_isFrozen)
         throw new InvalidOperationException("Collection is frozen.");
      if (_count >= _keys.Length)
         throw new InvalidOperationException("Capacity exceeded.");

      _keys[_count] = key;
      _values[_count] = value;
      _count++;
   }

   /// <summary>
   /// Sorts the internal arrays to prepare for Binary Search.
   /// Also trims excess memory if capacity wasn't reached.
   /// </summary>
   public void Freeze()
   {
      if (_isFrozen)
         return;

      if (_count < _keys.Length)
      {
         Array.Resize(ref _keys, _count);
         Array.Resize(ref _values, _count);
      }

      Array.Sort(_keys, _values, 0, _count);

      _isFrozen = true;
   }

   // --- RUNTIME ACCESS (Binary Search) ---

   public bool TryGetValue(TKey key, out TValue value)
   {
      var index = Array.BinarySearch(_keys, 0, _count, key);

      if (index >= 0)
      {
         value = _values[index];
         return true;
      }

      value = default!;
      return false;
   }

   // --- ITERATION ---

   public ReadOnlySpan<TValue> Values => new(_values, 0, _count);

   public ReadOnlySpan<TKey> Keys => new(_keys, 0, _count);
}