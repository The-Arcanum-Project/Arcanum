namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class Polygon(int color)
{
    public int Color { get; } = color;
    private List<BorderSegmentDirectional> Segments { get; } = [];
    public List<Polygon> Holes { get; } = [];

    public List<Vector2> GetAllPoints()
    {
        var points = new List<Vector2>();
        foreach (var segment in Segments)
            segment.AddToList(points);

        return points;
    }
}