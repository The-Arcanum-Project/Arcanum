namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct Vector2(int x, int y) : IEquatable<Vector2>
{
    public int X = x;
    public int Y = y;

    public float Magnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (float)Math.Sqrt(X * X + Y * Y);
    }

    public int SqrMagnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => X * X + Y * Y;
    }

    public static Vector2 Zero { get; } = new Vector2(0, 0);
    public static Vector2 One { get; } = new Vector2(1, 1);
    public static Vector2 Up { get; } = new Vector2(0, 1);
    public static Vector2 Down { get; } = new Vector2(0, -1);
    public static Vector2 Left { get; } = new Vector2(-1, 0);
    public static Vector2 Right { get; } = new Vector2(1, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector2 a, Vector2 b)
    {
        int dx = b.X - a.X;
        int dy = b.Y - a.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Scale(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X * b.X, a.Y * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Scale(Vector2 scale)
    {
        X *= scale.X;
        Y *= scale.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clamp(Vector2 min, Vector2 max)
    {
        X = Math.Max(min.X, X);
        X = Math.Min(max.X, X);
        Y = Math.Max(min.Y, Y);
        Y = Math.Min(max.Y, Y);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X * b.X, a.Y * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(int a, Vector2 b)
    {
        return new Vector2(a * b.X, a * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 a, int b)
    {
        return new Vector2(a.X * b, a.Y * b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 a, int b)
    {
        return new Vector2(a.X / b, a.Y / b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return !(lhs == rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? other)
    {
        return other is Vector2 vector2 && Equals(vector2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ (Y.GetHashCode() << 2);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}