using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;
using LibTessDotNet;

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
    
    public Point GetCentroid()
    {
        var points = GetAllPoints();
        if (points.Count == 0) return Point.Empty;

        var sumX = 0;
        var sumY = 0;

        foreach (var point in points)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        return new Point(sumX / points.Count, sumY / points.Count);
    }
    
    public (int, int) GetSize()
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var segment in Segments)
        {
            if (segment is Point p)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            else if (segment is BorderSegmentDirectional bsd)
            {
                foreach (var point in bsd.Segment.Points)
                {
                    if (point.X < minX) minX = point.X;
                    if (point.Y < minY) minY = point.Y;
                    if (point.X > maxX) maxX = point.X;
                    if (point.Y > maxY) maxY = point.Y;
                }
            }
        }

        return (maxX - minX + 1, maxY - minY + 1);
    }
    
    public List<Point> GetAllPoints()
    {
        var points = new List<Point>();
        foreach (var segment in Segments)
            segment.AddTo(points);

        return points;
    }
    
    public (List<Point> vertices, List<int> indices) Tesselate()
    {
        var tess = new Tess();
        var points = GetAllPoints();
        var contour = new ContourVertex[points.Count];

        for (var i = 0; i < points.Count; i++)
        {
            contour[i] = new ContourVertex
            {
                Position = new Vec3 { X = points[i].X, Y = points[i].Y, Z = 0 },
                Data = points[i]
            };
        }

        tess.AddContour(contour, ContourOrientation.Original);

        tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

        var vertices = new List<Point>();
        for (int i = 0; i < tess.VertexCount; i++)
        {
            var pos = tess.Vertices[i].Position;
            vertices.Add(new Point((int)pos.X, (int)pos.Y));
        }

        var indices = new List<int>();
        for (int i = 0; i < tess.ElementCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                indices.Add(tess.Elements[i * 3 + j]);
            }
        }

        return (vertices, indices);
    }
}