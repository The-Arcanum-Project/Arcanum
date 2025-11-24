using System.Runtime.CompilerServices;

namespace Arcanum.Core.DataStructures;

/// <summary>
/// A Dictionary that has supreme read performance after an initial bulk load phase
/// combined with optimized iteration via Span.
/// <br/>
/// Has more overhead on writes after the initial phase, as it needs to copy the internal array.
/// </summary>
public class SnapShotDictionary<TKey, TValue> where TKey : notnull
{
   private class State(Dictionary<TKey, TValue> map, TValue[] array)
   {
      public readonly Dictionary<TKey, TValue> Map = map;
      public TValue[] Array = array;
   }

   private readonly object _writeLock = new();

   private volatile State _currentState;

   // Tracks which "Phase" we are in
   private bool _isFrozen;

   // Tracks the pointer during init
   private int _initCount;

   /// <summary>
   /// Creates the collection with a pre-allocated buffer.
   /// </summary>
   /// <param name="capacity">The expected number of items (margin included).</param>
   public SnapShotDictionary(int capacity)
   {
      _currentState = new(new(capacity),
                          new TValue[capacity]);
   }

   // --- WRITE OPERATIONS ---

   public void Add(TKey key, TValue value)
   {
      lock (_writeLock)
         if (!_isFrozen)
         {
            var state = _currentState;

            if (_initCount >= state.Array.Length)
               Array.Resize(ref state.Array, state.Array.Length * 2);

            state.Map.Add(key, value);
            state.Array[_initCount] = value;
            _initCount++;
         }
         else
         {
            var oldState = _currentState;
            var newMap = new Dictionary<TKey, TValue>(oldState.Map);

            var newArray = new TValue[oldState.Array.Length + 1];
            Array.Copy(oldState.Array, newArray, oldState.Array.Length);

            newMap.Add(key, value);
            newArray[oldState.Array.Length] = value;

            _currentState = new(newMap, newArray);
         }
   }

   /// <summary>
   /// Call this once after you have finished the initial bulk loading.
   /// It trims the internal array to the exact size and enables Snapshot mode.
   /// </summary>
   public void Freeze()
   {
      lock (_writeLock)
      {
         if (_isFrozen)
            return;

         var state = _currentState;

         if (_initCount < state.Array.Length)
         {
            var trimmedArray = new TValue[_initCount];
            Array.Copy(state.Array, trimmedArray, _initCount);

            // Atomic Swap to the clean, trimmed version
            _currentState = new(state.Map, trimmedArray);
         }

         _isFrozen = true;
      }
   }

   // --- READ OPERATIONS ---

   public TValue this[TKey key]
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _currentState.Map[key];
   }

   public bool TryGetValue(TKey key, out TValue value) => _currentState.Map.TryGetValue(key, out value);

   // --- ITERATION ---

   /// <summary>
   /// Returns the Span. 
   /// If called during Init, it returns the slice of items added so far.
   /// If called after Freeze, it returns the full trimmed array.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<TValue> AsSpan()
   {
      var state = _currentState;
      // ReSharper disable twice InconsistentlySynchronizedField
      if (_isFrozen)
         return new(state.Array);

      return new(state.Array, 0, _initCount);
   }

   public ReadOnlySpan<TValue>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();
}