using System.Numerics;
using LibTessDotNet;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class PolygonParsing(int color)
{
    public int Color { get; } = color;
    public List<ICoordinateAdder> Segments { get; } = [];
    public List<PolygonParsing> Holes { get; } = [];
    
    public List<Vector2I> GetAllPoints()
    {
        var points = new List<Vector2I>();
        foreach (var segment in Segments)
            segment.AddTo(points);

        return points;
    }
    
    public Polygon Tesselate()
    {
        var tess = new Tess();
        var points = GetAllPoints();
        var contour = new ContourVertex[points.Count];

        for (var i = 0; i < points.Count; i++)
            contour[i] = new ContourVertex(new(points[i].X, points[i].Y, 0));
        

        tess.AddContour(contour);

        tess.Tessellate();

        var vertices = new Vector2[tess.VertexCount];
        
        for (var i = 0; i < tess.VertexCount; i++)
        {
            var pos = tess.Vertices[i].Position;
            vertices[i] = new (pos.X, pos.Y);
        }
        
        Console.WriteLine("Polygon:");
        Console.WriteLine(string.Join(", ", vertices));
        Console.WriteLine(string.Join(", ", tess.Elements));

        return new(vertices, tess.Elements);
    }
}

public interface ICoordinateAdder
{
    public void AddTo(List<Vector2I> points);
}