using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingBoxF : IEquatable<BoundingBoxF>
{
   public Vector3 Min;
   public Vector3 Max;

   // Computed properties inlined to avoid call overhead
   public readonly float Width
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Max.X - Min.X;
   }

   public readonly float Height
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Max.Y - Min.Y;
   }

   public readonly float Depth
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Max.Z - Min.Z;
   }

   public readonly Vector3 Center
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (Min + Max) * 0.5f;
   }

   public readonly Vector3 Size
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Max - Min;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public BoundingBoxF(Vector3 min, Vector3 max)
   {
      Min = min;
      Max = max;
   }

   /// <summary>
   /// Massive optimization: Uses ReadOnlySpan to avoid array allocation/copying.
   /// Uses SIMD Min/Max to process boundaries parallelly.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveOptimization)]
   public static BoundingBoxF CreateFromPoints(ReadOnlySpan<Vector3> points)
   {
      if (points.IsEmpty)
         return default;

      var min = new Vector3(float.MaxValue);
      var max = new Vector3(float.MinValue);

      var i = 0;
      var len = points.Length;

      for (; i < len; i++)
      {
         var pt = points[i];
         min = Vector3.Min(min, pt);
         max = Vector3.Max(max, pt);
      }

      return new(min, max);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static BoundingBoxF CreateFromCenterSize(Vector3 center, Vector3 size)
   {
      var half = size * 0.5f;
      return new(center - half, center + half);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public readonly bool Contains(Vector3 point)
   {
      if (Sse.IsSupported)
      {
         var vPoint = point.AsVector128();
         var vMin = Min.AsVector128();
         var vMax = Max.AsVector128();

         var geMin = Vector128.GreaterThanOrEqual(vPoint, vMin); // X>=minX, Y>=minY ...
         var leMax = Vector128.LessThanOrEqual(vPoint, vMax); // X<=maxX, Y<=maxY ...

         // Combine
         var combined = Vector128.BitwiseAnd(geMin, leMax);
         var mask = Sse.MoveMask(combined);
         return (mask & 0b0111) == 0b0111;
      }

      // Fallback (Branchless logic)
      // (a >= b) is 1 or 0.
      return (point.X >= Min.X) &
             (point.X <= Max.X) &
             (point.Y >= Min.Y) &
             (point.Y <= Max.Y) &
             (point.Z >= Min.Z) &
             (point.Z <= Max.Z);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public readonly bool Intersects(BoundingBoxF other)
   {
      if (Sse.IsSupported)
      {
         var vMinA = Min.AsVector128();
         var vMaxA = Max.AsVector128();
         var vMinB = other.Min.AsVector128();
         var vMaxB = other.Max.AsVector128();

         // MaxA >= MinB  AND  MinA <= MaxB
         var c1 = Vector128.GreaterThanOrEqual(vMaxA, vMinB);
         var c2 = Vector128.LessThanOrEqual(vMinA, vMaxB);

         var combined = Vector128.BitwiseAnd(c1, c2);

         var mask = Sse.MoveMask(combined);
         // Check X, Y, Z (bits 0, 1, 2)
         return (mask & 0b0111) == 0b0111;
      }

      return (Max.X >= other.Min.X) &
             (Min.X <= other.Max.X) &
             (Max.Y >= other.Min.Y) &
             (Min.Y <= other.Max.Y) &
             (Max.Z >= other.Min.Z) &
             (Min.Z <= other.Max.Z);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public readonly bool Equals(BoundingBoxF other) => Min.Equals(other.Min) && Max.Equals(other.Max);

   public override bool Equals(object? obj) => obj is BoundingBoxF other && Equals(other);
   public override int GetHashCode() => HashCode.Combine(Min, Max);
   public static bool operator ==(BoundingBoxF left, BoundingBoxF right) => left.Equals(right);
   public static bool operator !=(BoundingBoxF left, BoundingBoxF right) => !left.Equals(right);
}