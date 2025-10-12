namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct Vector2I(int x, int y) : IEquatable<Vector2I>
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

    public static Vector2I Zero { get; } = new Vector2I(0, 0);
    public static Vector2I One { get; } = new Vector2I(1, 1);
    public static Vector2I Up { get; } = new Vector2I(0, 1);
    public static Vector2I Down { get; } = new Vector2I(0, -1);
    public static Vector2I Left { get; } = new Vector2I(-1, 0);
    public static Vector2I Right { get; } = new Vector2I(1, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector2I a, Vector2I b)
    {
        int dx = b.X - a.X;
        int dy = b.Y - a.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I Min(Vector2I a, Vector2I b)
    {
        return new Vector2I(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I Max(Vector2I a, Vector2I b)
    {
        return new Vector2I(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I Scale(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X * b.X, a.Y * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Scale(Vector2I scale)
    {
        X *= scale.X;
        Y *= scale.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clamp(Vector2I min, Vector2I max)
    {
        X = Math.Max(min.X, X);
        X = Math.Min(max.X, X);
        Y = Math.Max(min.Y, Y);
        Y = Math.Min(max.Y, Y);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator +(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X + b.X, a.Y + b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator -(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X - b.X, a.Y - b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator *(Vector2I a, Vector2I b)
    {
        return new Vector2I(a.X * b.X, a.Y * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator *(int a, Vector2I b)
    {
        return new Vector2I(a * b.X, a * b.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator *(Vector2I a, int b)
    {
        return new Vector2I(a.X * b, a.Y * b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator /(Vector2I a, int b)
    {
        return new Vector2I(a.X / b, a.Y / b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2I lhs, Vector2I rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2I lhs, Vector2I rhs)
    {
        return !(lhs == rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? other)
    {
        return other is Vector2I vector2 && Equals(vector2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2I other)
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