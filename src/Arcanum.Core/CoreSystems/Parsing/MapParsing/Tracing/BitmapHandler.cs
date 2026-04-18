using System.Runtime.CompilerServices;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

/// <summary>
/// Interface for color-getting strategies. Implemented by zero-size structs for JIT inlining.
/// </summary>
public interface IBitmapHandler
{
   public static ReadOnlySpan<byte> BitMasks => [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01,];
   protected const int ALPHA = 255 << 24;
   int GetColor(IntPtr scan0, int stride, int width, int height, int x, int y);
   void MarkVisited(IntPtr scan0, int stride, int width, int height, int x, int y);
}

/// <summary>
/// Gets pixel color without bounds checking. Use only when coordinates are guaranteed in-bounds.
/// </summary>
public readonly struct UncheckedBitmapHandler : IBitmapHandler
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public unsafe int GetColor(IntPtr scan0, int stride, int width, int height, int x, int y)
   {
      var pixel = (byte*)scan0 + y * stride + x * 3;
      return IBitmapHandler.ALPHA | pixel[2] | (pixel[1] << 8) | (pixel[0] << 16);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public unsafe void MarkVisited(IntPtr scan0, int stride, int width, int height, int x, int y)
   {
      var row = (byte*)scan0 + y * stride;
      row[x >> 3] |= IBitmapHandler.BitMasks[x & 7];
   }
}

/// <summary>
/// Gets pixel color with bounds checking. Returns OUTSIDE_COLOR for out-of-bounds coordinates.
/// </summary>
public readonly struct CheckedBitmapHandler : IBitmapHandler
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public unsafe int GetColor(IntPtr scan0, int stride, int width, int height, int x, int y)
   {
      if ((uint)x >= (uint)width || (uint)y >= (uint)height)
         return MapTracing.OUTSIDE_COLOR;

      var pixel = (byte*)scan0 + y * stride + x * 3;
      return IBitmapHandler.ALPHA | pixel[2] | (pixel[1] << 8) | (pixel[0] << 16);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public unsafe void MarkVisited(IntPtr scan0, int stride, int width, int height, int x, int y)
   {
      if ((uint)x >= (uint)width || (uint)y >= (uint)height)
         return;
      
      var row = (byte*)scan0 + y * stride;
      row[x >> 3] |= IBitmapHandler.BitMasks[x & 7];
   }
}