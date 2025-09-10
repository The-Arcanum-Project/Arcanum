using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public readonly record struct Point(int X, int Y) : ICoordinateAdder
{
    public static Point Empty => new(int.MinValue, int.MinValue);

    public (int, int) ToTuple() => (X, Y);

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public void AddTo(List<Point> points)
    {
        points.Add(this);
    }

    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.X + p2.X, p1.Y + p2.Y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static Point operator *(Point p, int scalar)
    {
        return new Point(p.X * scalar, p.Y * scalar);
    }

    public static Point operator /(Point p, int scalar)
    {
        if (scalar == 0) throw new DivideByZeroException("Cannot divide by zero.");
        return new Point(p.X / scalar, p.Y / scalar);
    }

    public static int operator *(Point p1, Point p2)
    {
        return p1.X * p2.X + p1.Y * p2.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}