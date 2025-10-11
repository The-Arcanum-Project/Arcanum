namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class Polygon(int color)
{
    public int Color { get; } = color;
    public List<ICoordinateAdder> Segments { get; } = [];
    public List<Polygon> Holes { get; } = [];
}

public interface ICoordinateAdder
{
    public void AddTo(List<Vector2> points);
}