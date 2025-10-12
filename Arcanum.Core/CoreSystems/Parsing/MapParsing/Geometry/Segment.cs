namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class BorderSegment
{
    public List<Vector2I> Points { get; } = [];
}

public readonly struct BorderSegmentDirectional : ICoordinateAdder
{
    public readonly BorderSegment Segment;
    public readonly bool IsForward;

    public BorderSegmentDirectional(BorderSegment segment, bool isForward)
    {
        Segment = segment;
        IsForward = isForward;
    }

    public void AddTo(List<Vector2I> points)
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

    public override string ToString()
    {
        return string.Join(", ", IsForward ? Segment.Points : Segment.Points.AsEnumerable().Reverse());
    }
}