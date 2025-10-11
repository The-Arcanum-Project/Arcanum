namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class BorderSegment
{
    public List<Vector2> Points { get; } = [];
}

public readonly struct BorderSegmentDirectional(BorderSegment segment, bool isForward)
{
    public readonly BorderSegment Segment = segment;
    public readonly bool IsForward = isForward;

    public void AddToList(List<Vector2> points)
    {
        if (IsForward)
            points.AddRange(Segment.Points);
        else
            for (var i = Segment.Points.Count - 1; i >= 0; i--)
                points.Add(Segment.Points[i]);
    }

    public BorderSegmentDirectional Invert()
    {
        return new(Segment, !IsForward);
    }
}