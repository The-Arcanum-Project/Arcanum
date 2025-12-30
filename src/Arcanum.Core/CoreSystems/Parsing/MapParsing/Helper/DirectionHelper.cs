using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

public static class DirectionHelper
{
   // Ensures fields are packed: Xl, Yl, Xr, Yr, Xpos, Ypos (0, 1, 2, 3, 4, 5)
   [StructLayout(LayoutKind.Sequential)]
   public struct PointSet
   {
      public int Xl,
                 Yl,
                 Xr,
                 Yr,
                 Xpos,
                 Ypos;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public PointSet(int xl, int yl, int xr, int yr, int xpos, int ypos)
      {
         Xl = xl;
         Yl = yl;
         Xr = xr;
         Yr = yr;
         Xpos = xpos;
         Ypos = ypos;
      }

      public override string ToString() => $"L:({Xl},{Yl}) R:({Xr},{Yr}) P:({Xpos},{Ypos})";

      public string ToString(string? format, IFormatProvider? formatProvider) => ToString();
   }

   public static Vector2I GetPosition(this PointSet ps) => new(ps.Xpos, ps.Ypos);

   extension(Direction d)
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Direction RotateRight() => (Direction)(((int)d + 1) & 3);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Direction RotateLeft() => (Direction)(((int)d - 1) & 3);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public Direction Invert() => (Direction)(((int)d + 2) & 3);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Move(ref PointSet ps, out int cachePos, out bool xaxis)
      {
         // Since struct is Sequential:
         // Offset 0 targets: Xl, Xr, Xpos
         // Offset 1 targets: Yl, Yr, Ypos

         var dirIndex = (int)d;
         int delta = MoveDeltas[dirIndex];
         int offset = MoveOffsets[dirIndex];

         // Get reference to (Xl)
         ref var basePtr = ref ps.Xl;

         // Shift pointer to Xl or Yl based on direction
         ref var target1 = ref Unsafe.Add(ref basePtr, offset);

         cachePos = target1;

         // Update First Pair -> (Xl or Yl)
         target1 += delta;

         // Update Second Pair -> +2 ints away in memory
         Unsafe.Add(ref target1, 2) += delta;

         // Update Third Pair (Xpos or Ypos) -> +4 ints away in memory
         Unsafe.Add(ref target1, 4) += delta;

         xaxis = AxisXLookup[dirIndex];
      }
   }

   // LOOKUP TABLES for Move()
   // N(0), E(1), S(2), W(3)
   // Delta: N(-1), E(+1), S(+1), W(-1)
   private static ReadOnlySpan<sbyte> MoveDeltas => [-1, 1, 1, -1];

   // Offset: N(Y=1), E(X=0), S(Y=1), W(X=0) -> Which memory offset to start at?
   private static ReadOnlySpan<byte> MoveOffsets => [1, 0, 1, 0];

   // IsAxisX: N(false), E(true), S(false), W(true)
   private static ReadOnlySpan<bool> AxisXLookup => [false, true, false, true];

   // --- Data Tables for StartPos ---

   // @formatter:off
   // Packed data: Xl, Yl, Xr, Yr, Xpos, Ypos offsets
   //           N: -1, -1,  0, -1,    0,    0
   //           E:  0, -1,  0,  0,    0,    0
   //           S:  0,  0, -1,  0,    0,    0
   //           W: -1,  0, -1, -1,    0,    0
   private static ReadOnlySpan<sbyte> StartPosOffsets =>
   [
      -1, -1,  0, -1, 0, 0, // North (0 - 5)
       0, -1,  0,  0, 0, 0, // East  (6 -11)
       0,  0, -1,  0, 0, 0, // South (12-17)
      -1,  0, -1, -1, 0, 0, // West  (18-23)
   ];
   // @formatter:on

   /// <summary>
   /// Given the current position in Grid Coordinates and a direction,
   /// calculates the pixel coordinates to the left and right of the current position.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static PointSet GetStartPos(int xGrid, int yGrid, Direction d)
   {
      var baseIndex = (int)d * 6;
      var offsets = StartPosOffsets;

      return new(xGrid + offsets[baseIndex],
                 yGrid + offsets[baseIndex + 1],
                 xGrid + offsets[baseIndex + 2],
                 yGrid + offsets[baseIndex + 3],
                 xGrid + offsets[baseIndex + 4],
                 yGrid + offsets[baseIndex + 5]);
   }

   // --- Optimized Data Tables for RightPixel ---

   // @formatter:off
   //    X offset, Y offset
   // N:  0,        -1
   // E:  0,         0
   // S: -1,         0
   // W: -1,        -1
   private static ReadOnlySpan<sbyte> RightPixelOffsets =>
   [
       0, -1, // North
       0,  0, // East
      -1,  0, // South
      -1, -1, // West
   ];
   // @formatter:on

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static (int, int) GetRightPixel(int x, int y, Direction d)
   {
      var i = (int)d << 1;
      ref var basePtr = ref MemoryMarshal.GetReference(RightPixelOffsets);
      int dx = Unsafe.Add(ref basePtr, i);
      int dy = Unsafe.Add(ref basePtr, i + 1);

      return (x + dx, y + dy);
   }
}