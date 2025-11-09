using System.Numerics;
using System.Runtime.InteropServices;
using Arcanum.Core.Utils.Geometry;

namespace Arcanum.Core.CoreSystems.Map;

public sealed class Polygon
{
    public Vector2[] Vertices { get; }
    public int[] TriangleIndices { get; } // [0,1,2, 0,2,3,...]
    public RectangleF Bounds { get; }

    public int ColorIndex;

    public Polygon(Vector2[] vertices, int[] triangleIndices)
    {
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        TriangleIndices = triangleIndices ?? throw new ArgumentNullException(nameof(triangleIndices));
        this.ColorIndex = ColorIndex;

#if DEBUG
        if (Vertices.Length < 3)
            throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertices));

        if (TriangleIndices.Length % 3 != 0)
            throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(triangleIndices));
#endif

        Bounds = CalculateBounds_SIMD();
    }

    private RectangleF CalculateBounds()
    {
        if (Vertices.Length == 0)
            return RectangleF.Empty;

        float minX = Vertices[0].X,
            maxX = Vertices[0].X;
        float minY = Vertices[0].Y,
            maxY = Vertices[0].Y;

        for (var i = 1; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            if (vertex.X < minX)
                minX = vertex.X;
            if (vertex.X > maxX)
                maxX = vertex.X;
            if (vertex.Y < minY)
                minY = vertex.Y;
            if (vertex.Y > maxY)
                maxY = vertex.Y;
        }

        return new(minX, minY, maxX - minX, maxY - minY);
    }

    private RectangleF CalculateBounds_SIMD()
    {
        if (Vertices.Length == 0)
            return RectangleF.Empty;

        // Get the number of Vector2s we can process at once.
        // Vector<float>.Count is the number of floats in a SIMD register (e.g., 4 or 8).
        // So we process (Vector<float>.Count / 2) Vector2s at a time.
        var vectorSize = Vector<float>.Count;
        var vector2Count = vectorSize / 2;

        var minValues = new Vector<float>(float.PositiveInfinity);
        var maxValues = new Vector<float>(float.NegativeInfinity);

        var i = 0;
        // Process the main part of the array in large, vectorized chunks
        for (; i <= Vertices.Length - vector2Count; i += vector2Count)
        {
            // Load a chunk of vertices into a SIMD vector.
            // The data layout needs to be [X1, Y1, X2, Y2, X3, Y3, X4, Y4] for this to work.
            // We can achieve this with a Span and MemoryMarshal.
            var span = new Span<Vector2>(Vertices, i, vector2Count);
            var vector = MemoryMarshal.Cast<Vector2, float>(span);

            minValues = Vector.Min(minValues, new(vector));
            maxValues = Vector.Max(maxValues, new(vector));
        }

        // Process the remaining elements that didn't fit into a full vector chunk
        float minX = float.PositiveInfinity,
            maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity,
            maxY = float.NegativeInfinity;

        for (; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            if (vertex.X < minX)
                minX = vertex.X;
            if (vertex.X > maxX)
                maxX = vertex.X;
            if (vertex.Y < minY)
                minY = vertex.Y;
            if (vertex.Y > maxY)
                maxY = vertex.Y;
        }

        // Reduce the SIMD vectors to single min/max values
        // minValues vector might look like [minX1, minY1, minX2, minY2]
        // We need to find the minimum of all X's and all Y's.
        for (var j = 0; j < vectorSize; j++)
            if (j % 2 == 0) // Even indices are X
            {
                if (minValues[j] < minX)
                    minX = minValues[j];
                if (maxValues[j] > maxX)
                    maxX = maxValues[j];
            }
            else // Odd indices are Y
            {
                if (minValues[j] < minY)
                    minY = minValues[j];
                if (maxValues[j] > maxY)
                    maxY = maxValues[j];
            }

        return new(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Determines if the polygon intersects with a given rectangle using SIMD-accelerated checks.
    /// </summary>
    /// <param name="rect">The rectangle to check for intersection.</param>
    /// <returns>True if the polygon intersects with the rectangle, false otherwise.</returns>
    public bool Intersects(RectangleF rect)
    {
        // 1. Broad Phase: Check if the bounding boxes intersect. If not, we can exit early.
        if (!Bounds.IntersectsWith(rect))
        {
            return false;
        }

        var rectMin = new Vector2(rect.Left, rect.Top);
        var rectMax = new Vector2(rect.Right, rect.Bottom);
        var rectCenter = rectMin + (rectMax - rectMin) * 0.5f;

        // 2. Narrow Phase: Check if any of the polygon's triangles intersect the rectangle.
        for (var i = 0; i < TriangleIndices.Length; i += 3)
        {
            var v0 = Vertices[TriangleIndices[i]];
            var v1 = Vertices[TriangleIndices[i + 1]];
            var v2 = Vertices[TriangleIndices[i + 2]];

            if (TriangleIntersectsAabb(v0, v1, v2, rectMin, rectMax, rectCenter))
            {
                return true;
            }
        }

        // 3. Final Check: Test if the rectangle is fully contained within the polygon.
        // This is necessary for cases where no edges intersect, but one shape is inside the other.
        if (Contains(rectMin))
        {
            return true;
        }

        return false;
    }

    private static bool TriangleIntersectsAabb(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 rectMin, Vector2 rectMax,
        Vector2 rectCenter)
    {
        // Use SIMD-accelerated Min/Max to find the triangle's AABB
        var triMin = Vector2.Min(v0, Vector2.Min(v1, v2));
        var triMax = Vector2.Max(v0, Vector2.Max(v1, v2));

        // Check if the AABBs of the triangle and the rectangle overlap
        if (triMax.X < rectMin.X || triMin.X > rectMax.X || triMax.Y < rectMin.Y || triMin.Y > rectMax.Y)
        {
            return false;
        }

        // Separating Axis Theorem (SAT)
        // We need to test the normals of the triangle's edges as separating axes.

        // Edge 0: v1 - v0
        var edge0 = v1 - v0;
        var axis0 = new Vector2(-edge0.Y, edge0.X);
        if (IsSeparatingAxis(axis0, v0, v1, v2, rectCenter, rectMax - rectCenter))
        {
            return false;
        }

        // Edge 1: v2 - v1
        var edge1 = v2 - v1;
        var axis1 = new Vector2(-edge1.Y, edge1.X);
        if (IsSeparatingAxis(axis1, v0, v1, v2, rectCenter, rectMax - rectCenter))
        {
            return false;
        }

        // Edge 2: v0 - v2
        var edge2 = v0 - v2;
        var axis2 = new Vector2(-edge2.Y, edge2.X);
        if (IsSeparatingAxis(axis2, v0, v1, v2, rectCenter, rectMax - rectCenter))
        {
            return false;
        }

        return true;
    }

    private static bool IsSeparatingAxis(Vector2 axis, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 rectCenter,
        Vector2 rectHalfExtents)
    {
        // Project the triangle's vertices onto the axis
        var p0 = Vector2.Dot(v0, axis);
        var p1 = Vector2.Dot(v1, axis);
        var p2 = Vector2.Dot(v2, axis);

        // Project the rectangle onto the axis
        var rProj = rectHalfExtents.X * Math.Abs(axis.X) + rectHalfExtents.Y * Math.Abs(axis.Y);
        var cProj = Vector2.Dot(rectCenter, axis);

        // Get the min/max projections of the triangle
        var triMin = Math.Min(p0, Math.Min(p1, p2));
        var triMax = Math.Max(p0, Math.Max(p1, p2));

        // Check for separation
        return triMax < cProj - rProj || triMin > cProj + rProj;
    }

    public bool Contains(Vector2 point)
    {
        if (!Bounds.ContainsVec2(point))
            return false;

        for (var i = 0; i < TriangleIndices.Length; i += 3)
        {
            var a = Vertices[TriangleIndices[i]];
            var b = Vertices[TriangleIndices[i + 1]];
            var c = Vertices[TriangleIndices[i + 2]];
            if (PointInTriangle(point, a, b, c))
                return true;
        }

        return false;
    }

    public bool Contains(RectangleF rect)
    {
        if (!Bounds.Contains(rect))
            return false;

        var corners = new[]
        {
            new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom),
            new Vector2(rect.Left, rect.Bottom),
        };

        foreach (var corner in corners)
            if (!Contains(corner))
                return false;

        return true;
    }

    public bool Intersects(Polygon b)
    {
        if (!Bounds.IntersectsWith(b.Bounds))
            return false;

        foreach (var vertex in Vertices)
            if (b.Contains(vertex))
                return true;

        foreach (var vertex in b.Vertices)
            if (Contains(vertex))
                return true;

        return false;
    }

    public bool Intersects(IEnumerable<Polygon> others)
    {
        foreach (var other in others)
            if (Intersects(other))
                return true;

        return false;
    }

    public bool Contains(Polygon other)
    {
        foreach (var vertex in other.Vertices)
            if (!Contains(vertex))
                return false;

        return true;
    }

    public bool Intersects(Vector2 circleCenter, float circleRadius)
    {
        // 1. Broad Phase: Polygon AABB vs Circle AABB check (cheap and effective).
        var circleBounds = new RectangleF(
            circleCenter.X - circleRadius,
            circleCenter.Y - circleRadius,
            circleRadius * 2,
            circleRadius * 2);

        if (!Bounds.IntersectsWith(circleBounds))
        {
            return false;
        }

        // 2. Narrow Phase
        var radiusSq = circleRadius * circleRadius;

        // 2a: Check if any polygon vertex is inside the circle.
        // This is a very fast check that catches many intersection cases.
        foreach (var vertex in Vertices)
        {
            if (Vector2.DistanceSquared(vertex, circleCenter) <= radiusSq)
            {
                return true;
            }
        }

        // 2b: Check if the circle's center is inside the polygon.
        // This handles cases where the circle is fully contained within the polygon.
        if (IsPointInPolygon(circleCenter, Vertices))
        {
            return true;
        }

        // 2c: Check if any polygon edge intersects the circle.
        // This is the most expensive check, so it's done last.
        for (var i = 0; i < Vertices.Length; i++)
        {
            var p1 = Vertices[i];
            var p2 = Vertices[(i + 1) % Vertices.Length]; // Wrap around for the last edge

            if (DistanceToLineSegmentSq(circleCenter, p1, p2) <= radiusSq)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates the squared distance from a point to a line segment.
    /// </summary>
    private static float DistanceToLineSegmentSq(Vector2 point, Vector2 edgeA, Vector2 edgeB)
    {
        var edgeDir = edgeB - edgeA;
        var pointDir = point - edgeA;

        var edgeLengthSq = Vector2.Dot(edgeDir, edgeDir);
        if (edgeLengthSq < 1e-9f) // Avoid division by zero for zero-length edges.
        {
            return Vector2.Dot(pointDir, pointDir);
        }

        // Project pointDir onto edgeDir. The result 't' is the interpolation factor.
        var t = Vector2.Dot(pointDir, edgeDir) / edgeLengthSq;

        // Clamp t to the [0, 1] range to stay on the line segment.
        t = Math.Max(0, Math.Min(1, t));

        // Find the closest point on the segment and calculate the squared distance.
        var closestPoint = edgeA + t * edgeDir;
        return Vector2.DistanceSquared(point, closestPoint);
    }

    /// <summary>
    /// Checks if a point is inside a polygon using the Ray Casting algorithm.
    /// </summary>
    private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool isInside = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) +
                    polygon[i].X))
            {
                isInside = !isInside;
            }

            j = i;
        }

        return isInside;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        var v0 = new Vector2(c.X - a.X, c.Y - a.Y);
        var v1 = new Vector2(b.X - a.X, b.Y - a.Y);
        var v2 = new Vector2(p.X - a.X, p.Y - a.Y);

        var dot00 = v0.X * v0.X + v0.Y * v0.Y;
        var dot01 = v0.X * v1.X + v0.Y * v1.Y;
        var dot02 = v0.X * v2.X + v0.Y * v2.Y;
        var dot11 = v1.X * v1.X + v1.Y * v1.Y;
        var dot12 = v1.X * v2.X + v1.Y * v2.Y;

        var invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return u >= 0 && v >= 0 && u + v <= 1;
    }
}