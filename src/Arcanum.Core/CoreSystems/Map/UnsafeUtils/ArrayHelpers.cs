using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arcanum.Core.CoreSystems.Map.UnsafeUtils;

public static class ArrayHelpers
{
   public static unsafe void CopyTo<T>(T[] source, T[] destination, int length) where T : unmanaged
   {
      ref var src = ref MemoryMarshal.GetArrayDataReference(source);
      ref var dest = ref MemoryMarshal.GetArrayDataReference(destination);

      Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref dest),
                                ref Unsafe.As<T, byte>(ref src),
                                (uint)(length * sizeof(T)));
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static void CopyInts(int[] source, int[] destination, int length)
   {
      ref var src = ref Unsafe.As<int, byte>(ref MemoryMarshal.GetArrayDataReference(source));
      ref var dest = ref Unsafe.As<int, byte>(ref MemoryMarshal.GetArrayDataReference(destination));

      Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)length << 2);
   }
}