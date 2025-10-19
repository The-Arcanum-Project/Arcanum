using System.Numerics;
using Arcanum.Core.CoreSystems.Map;
using LibTessDotNet;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class PolygonParsing(int color)
{
    public readonly int Color = color;
    public List<ICoordinateAdder> Segments { get; } = [];
    public List<PolygonParsing> Holes { get; } = [];

    private List<Vector2I> GetAllPoints()
    {
        var points = new List<Vector2I>();
        foreach (var segment in Segments)
            segment.AddTo(points);

        return points;
    }
    
    public Polygon Tessellate()
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

        return new(vertices, tess.Elements);
    }
    
    public Rectangle GetBoundingBox()
    {
        var points = GetAllPoints();
        if (points.Count == 0)
            return new Rectangle(0, 0, 0, 0);
        
        var minX = points[0].X;
        var maxX = points[0].X;
        var minY = points[0].Y;
        var maxY = points[0].Y;

        foreach (var point in points)
        {
            if (point.X < minX) minX = point.X;
            if (point.X > maxX) maxX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.Y > maxY) maxY = point.Y;
        }

        return new (minX, minY, maxX - minX, maxY - minY);
    }
}

public interface ICoordinateAdder
{
    public void AddTo(List<Vector2I> points);
}