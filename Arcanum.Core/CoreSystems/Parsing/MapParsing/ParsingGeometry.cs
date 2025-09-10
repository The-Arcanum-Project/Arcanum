using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public class BorderSegment
{
    public List<Point> Points { get; } = [];
}

public readonly struct BorderSegmentDirectional(BorderSegment segment, bool isForward) : ICoordinateAdder
{
    public readonly BorderSegment Segment = segment;
    public readonly bool IsForward = isForward;

    public void AddTo(List<Point> points)
    {
        if (IsForward)
        {
            points.AddRange(Segment.Points);
        }
        else
        {
            for (var i = Segment.Points.Count - 1; i >= 0; i--)
            {
                points.Add(Segment.Points[i]);
            }
        }
    }

    public BorderSegmentDirectional Invert()
    {
        return new(Segment, !IsForward);
    }
}

public class Polygon(int color)
{
    public int Color { get; } = color;
    public List<ICoordinateAdder> Segments { get; } = [];
    public List<Polygon> Holes { get; } = [];

    public List<Point> GetAllPoints()
    {
        var points = new List<Point>();
        foreach (var segment in Segments)
            segment.AddTo(points);

        return points;
    }
}