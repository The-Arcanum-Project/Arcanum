using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

[StructLayout(LayoutKind.Explicit, Size = 8)]
[Serializable]
[SkipLocalsInit] // skipping zero-init of stackallocs
public struct Vector2I
   : IEquatable<Vector2I>,
     ISpanFormattable
{
   // X and Y to specific offsets. 
   [FieldOffset(0)]
   public int X;

   [FieldOffset(4)]
   public int Y;

   // read / write with only one 64-bit integer
   // This long is overlapping with the two ints above and thus represents their packed form and the entire struct is 8 bytes in size.
   [FieldOffset(0)]
   private readonly long _packed;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   // Making this a primary constructor will cause issues.
   // ReSharper disable once ConvertToPrimaryConstructor
   public Vector2I(int x, int y)
   {
      _packed = 0;
      X = x;
      Y = y;
   }

   public readonly float Magnitude
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => MathF.Sqrt(X * X + Y * Y);
   }

   public readonly int SqrMagnitude
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => X * X + Y * Y;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Set(int x, int y)
   {
      X = x;
      Y = y;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static float Distance(Vector2I a, Vector2I b)
   {
      float dx = b.X - a.X;
      float dy = b.Y - a.Y;
      return MathF.Sqrt(dx * dx + dy * dy);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool operator ==(Vector2I lhs, Vector2I rhs) => lhs._packed == rhs._packed;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static bool operator !=(Vector2I lhs, Vector2I rhs) => lhs._packed != rhs._packed;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public override bool Equals(object? other) => other is Vector2I vector2 && this == vector2;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Equals(Vector2I other) => this == other;

   // The Golden Ratio (phi) constant for 2^64: 11400714819323198485
   // This scatters the bits across the range to maximize entropy in hash tables.
   private const long HASH_MULTIPLIER = -7046029254386353131;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public readonly override int GetHashCode()
   {
      var v = _packed;
      v *= HASH_MULTIPLIER;
      return (int)(v ^ (v >> 32));
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I Min(Vector2I a, Vector2I b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I Max(Vector2I a, Vector2I b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I Scale(Vector2I a, Vector2I b) => new(a.X * b.X, a.Y * b.Y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Scale(Vector2I scale)
   {
      X *= scale.X;
      Y *= scale.Y;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Clamp(Vector2I min, Vector2I max)
   {
      X = Math.Clamp(X, min.X, max.X);
      Y = Math.Clamp(Y, min.Y, max.Y);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator +(Vector2I a, Vector2I b) => new(a.X + b.X, a.Y + b.Y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator -(Vector2I a, Vector2I b) => new(a.X - b.X, a.Y - b.Y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator *(Vector2I a, Vector2I b) => new(a.X * b.X, a.Y * b.Y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator *(int a, Vector2I b) => new(a * b.X, a * b.Y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator *(Vector2I a, int b) => new(a.X * b, a.Y * b);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector2I operator /(Vector2I a, int b) => new(a.X / b, a.Y / b);

   public override string ToString() => $"({X}, {Y})";

   public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

   public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
   {
      var currentPos = 0;
      if (destination.Length < 1)
      {
         charsWritten = 0;
         return false;
      }

      destination[currentPos++] = '(';

      if (!X.TryFormat(destination[currentPos..], out var xWritten, format, provider))
      {
         charsWritten = 0;
         return false;
      }

      currentPos += xWritten;

      if (destination.Length < currentPos + 2)
      {
         charsWritten = 0;
         return false;
      }

      destination[currentPos++] = ',';
      destination[currentPos++] = ' ';

      if (!Y.TryFormat(destination[currentPos..], out var yWritten, format, provider))
      {
         charsWritten = 0;
         return false;
      }

      currentPos += yWritten;

      if (destination.Length < currentPos + 1)
      {
         charsWritten = 0;
         return false;
      }

      destination[currentPos++] = ')';

      charsWritten = currentPos;
      return true;
   }
}