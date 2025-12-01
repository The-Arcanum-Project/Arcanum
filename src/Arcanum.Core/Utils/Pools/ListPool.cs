using System.Collections.Concurrent;

namespace Arcanum.Core.Utils.Pools;

/// <summary>
/// A thread-safe static pool for caching List{T} instances to reduce GC allocations.
/// </summary>
public static class ListPool<T>
{
   private static readonly ConcurrentBag<List<T>> Pool = [];

   /// <summary>
   /// The maximum capacity of a list that will be retained in the pool.
   /// Lists larger than this will be discarded to prevent holding onto massive arrays.
   /// Default is 4096 items.
   /// </summary>
   private const int MAX_RETAINED_CAPACITY = 128;

   private const int MAX_POOL_SIZE = 12;

   /// <summary>
   /// Gets a List from the pool or creates a new one if the pool is empty.
   /// </summary>
   public static List<T> Get()
   {
      if (Pool.TryTake(out var list))
         return list;

      return [];
   }

   /// <summary>
   /// Gets a List from the pool and provides a Disposable handle.
   /// Usage: using var handle = ListPool{in}.Get(out var list);
   /// </summary>
   public static PooledListHandle Get(out List<T> list)
   {
      list = Get();
      return new(list);
   }

   /// <summary>
   /// Returns the list to the pool and Clears it.
   /// </summary>
   public static void Return(List<T> list)
   {
      if (list == null!)
         return;

      // Clear the list so it is empty for the next user.
      // Note: Clear() resets Count to 0 but keeps the internal array Capacity.
      list.Clear();

      // If the list is massive (e.g. used for a huge one-off operation), 
      // let the GC reclaim it instead of holding that memory forever.
      if (list.Capacity > MAX_RETAINED_CAPACITY)
         return;

      if (Pool.Count >= MAX_POOL_SIZE)
         return;

      Pool.Add(list);
   }

   /// <summary>
   /// A struct-based disposable handle to ensure the list is returned.
   /// </summary>
   public readonly struct PooledListHandle(List<T> list) : IDisposable
   {
      public void Dispose()
      {
         Return(list);
      }
   }
}