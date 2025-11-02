using System.Collections.Concurrent;
using System.Text;

namespace Arcanum.Core.CoreSystems.IO;

public readonly struct PooledStringBuilder(StringBuilder builder) : IDisposable
{
   public StringBuilder Builder { get; } = builder;

   public void Dispose()
   {
      Builder.Clear();
      StringBuilderPool.Return(Builder);
   }

   public override string ToString() => Builder.ToString();
}

public static class StringBuilderPool
{
   private static readonly ConcurrentBag<StringBuilder> Pool = [];
   private const int MAX_CAPACITY = 1 << 15; // 32 KB cap, adjust as needed

   public static PooledStringBuilder Get() => new(Pool.TryTake(out var sb) ? sb : new(1024));

   public static void Return(StringBuilder sb)
   {
      if (sb.Capacity <= MAX_CAPACITY)
         Pool.Add(sb);
   }
}