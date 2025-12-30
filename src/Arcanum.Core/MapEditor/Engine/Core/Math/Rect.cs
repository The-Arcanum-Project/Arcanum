using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

namespace Arcanum.Core.MapEditor.Engine.Core.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct RectF(float x, float y, float width, float height)
   : IEquatable<RectF>
{
   public float X = x;
   public float Y = y;
   public float Width = width;
   public float Height = height;

   public float Left
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => X;
   }
   public float Right
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => X + Width;
   }
   public float Top
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Y;
   }
   public float Bottom
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Y + Height;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Contains(Vector2I point)
   {
      float px = point.X;
      float py = point.Y;

      var dx = px - X;
      var dy = py - Y;

      // Interpret the float bits directly as unsigned integers.
      // If 'dx' is negative, the Sign Bit (MSB) is 1.
      // When interpreted as uint, it becomes a number > 2^31, which is > Width.

      return (BitConverter.SingleToUInt32Bits(dx) <= BitConverter.SingleToUInt32Bits(Width)) &
             (BitConverter.SingleToUInt32Bits(dy) <= BitConverter.SingleToUInt32Bits(Height));
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public readonly bool Intersects(RectF other) => (other.X < X + Width) &
                                                   (X < other.X + other.Width) &
                                                   (other.Y < Y + Height) &
                                                   (Y < other.Y + other.Height);

   [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
   public bool Equals(RectF other)
   {
      if (!Sse.IsSupported)
         return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

      var v1 = Unsafe.As<RectF, Vector128<float>>(ref this);
      var v2 = Unsafe.As<RectF, Vector128<float>>(ref other);
      var diff = Sse.CompareEqual(v1, v2);

      return Sse.MoveMask(diff) == 0b1111;
   }

   public override bool Equals(object? obj) => obj is RectF other && Equals(other);

   public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

   public static bool operator ==(RectF left, RectF right) => left.Equals(right);
   public static bool operator !=(RectF left, RectF right) => !left.Equals(right);
}