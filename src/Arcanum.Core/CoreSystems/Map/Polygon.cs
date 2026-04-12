using System.Numerics;
using System.Runtime.CompilerServices;
using Arcanum.Core.Utils.Geometry;
using Common.UI.Map;

namespace Arcanum.Core.CoreSystems.Map;

public static class PolygonExtensions
{
    extension(Polygon polygon)
    {
        /// <summary>
        /// Determines if the polygon intersects with a given rectangle using SIMD-accelerated checks.
        /// </summary>
        /// <param name="rect">The rectangle to check for intersection.</param>
        /// <returns>True if the polygon intersects with the rectangle, false otherwise.</returns>
        public bool Intersects(RectangleF rect)
        {
            // 1. Broad Phase: Check if the bounding boxes intersect. If not, we can exit early.
            if (!polygon.Bounds.IntersectsWith(rect))
                return false;

            var rectMin = new Vector2(rect.Left, rect.Top);
            var rectMax = new Vector2(rect.Right, rect.Bottom);
            var rectCenter = rectMin + (rectMax - rectMin) * 0.5f;

            // 2. Narrow Phase: Check if any of the polygon's triangles intersect the rectangle.
            for (var i = 0; i < polygon.TriangleIndices.Length; i += 3)
            {
                var v0 = polygon.Vertices[polygon.TriangleIndices[i]];
                var v1 = polygon.Vertices[polygon.TriangleIndices[i + 1]];
                var v2 = polygon.Vertices[polygon.TriangleIndices[i + 2]];

                if (TriangleIntersectsAabb(v0, v1, v2, rectMin, rectMax, rectCenter))
                    return true;
            }

            // 3. Final Check: Test if the rectangle is fully contained within the polygon.
            // This is necessary for cases where no edges intersect, but one shape is inside the other.
            if (polygon.Contains(rectMin))
                return true;

            return false;
        }

        private static bool TriangleIntersectsAabb(Vector2 v0,
            Vector2 v1,
            Vector2 v2,
            Vector2 rectMin,
            Vector2 rectMax,
            Vector2 rectCenter)
        {
            // Use SIMD-accelerated Min/Max to find the triangle's AABB
            var triMin = Vector2.Min(v0, Vector2.Min(v1, v2));
            var triMax = Vector2.Max(v0, Vector2.Max(v1, v2));

            // Check if the AABBs of the triangle and the rectangle overlap
            if (triMax.X < rectMin.X || triMin.X > rectMax.X || triMax.Y < rectMin.Y || triMin.Y > rectMax.Y)
                return false;

            // Separating Axis Theorem (SAT)
            // We need to test the normals of the triangle's edges as separating axes.

            // Edge 0: v1 - v0
            var edge0 = v1 - v0;
            var axis0 = new Vector2(-edge0.Y, edge0.X);
            if (IsSeparatingAxis(axis0, v0, v1, v2, rectCenter, rectMax - rectCenter))
                return false;

            // Edge 1: v2 - v1
            var edge1 = v2 - v1;
            var axis1 = new Vector2(-edge1.Y, edge1.X);
            if (IsSeparatingAxis(axis1, v0, v1, v2, rectCenter, rectMax - rectCenter))
                return false;

            // Edge 2: v0 - v2
            var edge2 = v0 - v2;
            var axis2 = new Vector2(-edge2.Y, edge2.X);
            if (IsSeparatingAxis(axis2, v0, v1, v2, rectCenter, rectMax - rectCenter))
                return false;

            return true;
        }

        private static bool IsSeparatingAxis(Vector2 axis,
            Vector2 v0,
            Vector2 v1,
            Vector2 v2,
            Vector2 rectCenter,
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
            if (!polygon.Bounds.ContainsVec2(point))
                return false;

            for (var i = 0; i < polygon.TriangleIndices.Length; i += 3)
            {
                var a = polygon.Vertices[polygon.TriangleIndices[i]];
                var b = polygon.Vertices[polygon.TriangleIndices[i + 1]];
                var c = polygon.Vertices[polygon.TriangleIndices[i + 2]];
                if (Polygon.PointInTriangle(point, a, b, c))
                    return true;
            }

            return false;
        }

        public bool Contains(RectangleF rect)
        {
            if (!polygon.Bounds.Contains(rect))
                return false;

            var corners = new[]
            {
                new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top),
                new Vector2(rect.Right, rect.Bottom),
                new Vector2(rect.Left, rect.Bottom),
            };

            foreach (var corner in corners)
                if (!polygon.Contains(corner))
                    return false;

            return true;
        }

        public bool Intersects(Polygon b)
        {
            if (!polygon.Bounds.IntersectsWith(b.Bounds))
                return false;

            foreach (var vertex in polygon.Vertices)
                if (b.Contains(vertex))
                    return true;

            foreach (var vertex in b.Vertices)
                if (polygon.Contains(vertex))
                    return true;

            return false;
        }

        public bool Intersects(IEnumerable<Polygon> others)
        {
            foreach (var other in others)
                if (polygon.Intersects(other))
                    return true;

            return false;
        }

        public bool Contains(Polygon other)
        {
            foreach (var vertex in other.Vertices)
                if (!polygon.Contains(vertex))
                    return false;

            return true;
        }

        public bool Intersects(Vector2 circleCenter, float circleRadius)
        {
            // 1. Broad Phase: Polygon AABB vs Circle AABB check (cheap and effective).
            var circleBounds = new RectangleF(circleCenter.X - circleRadius,
                circleCenter.Y - circleRadius,
                circleRadius * 2,
                circleRadius * 2);

            if (!polygon.Bounds.IntersectsWith(circleBounds))
                return false;

            // 2. Narrow Phase
            var radiusSq = circleRadius * circleRadius;

            // 2a: Check if any polygon vertex is inside the circle.
            // This is a very fast check that catches many intersection cases.
            foreach (var vertex in polygon.Vertices)
                if (Vector2.DistanceSquared(vertex, circleCenter) <= radiusSq)
                    return true;

            // 2b: Check if the circle's center is inside the polygon.
            // This handles cases where the circle is fully contained within the polygon.
            if (IsPointInPolygon(circleCenter, polygon.Vertices))
                return true;

            // 2c: Check if any polygon edge intersects the circle.
            // This is the most expensive check, so it's done last.
            for (var i = 0; i < polygon.Vertices.Length; i++)
            {
                var p1 = polygon.Vertices[i];
                var p2 = polygon.Vertices[(i + 1) % polygon.Vertices.Length]; // Wrap around for the last edge

                if (DistanceToLineSegmentSq(circleCenter, p1, p2) <= radiusSq)
                    return true;
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
                return Vector2.Dot(pointDir, pointDir);

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
        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygons)
        {
            var isInside = false;
            var j = polygons.Length - 1;
            for (var i = 0; i < polygons.Length; i++)
            {
                if ((polygons[i].Y > point.Y) != (polygons[j].Y > point.Y) &&
                    (point.X <
                     (polygons[j].X - polygons[i].X) * (point.Y - polygons[i].Y) / (polygons[j].Y - polygons[i].Y) +
                     polygons[i].X))
                    isInside = !isInside;

                j = i;
            }

            return isInside;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            double pX = p.X,
                pY = p.Y;
            double aX = a.X,
                aY = a.Y;
            double bX = b.X,
                bY = b.Y;
            double cX = c.X,
                cY = c.Y;

            var cp1 = (bX - aX) * (pY - aY) - (bY - aY) * (pX - aX);
            var cp2 = (cX - bX) * (pY - bY) - (cY - bY) * (pX - bX);
            var cp3 = (aX - cX) * (pY - cY) - (aY - cY) * (pX - cX);

            var hasNeg = (cp1 < 0) || (cp2 < 0) || (cp3 < 0);
            var hasPos = (cp1 > 0) || (cp2 > 0) || (cp3 > 0);

            return !(hasNeg && hasPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Rasterize<TAction>(ref TAction action, int clipMinY, int clipMaxY)
            where TAction : struct, IPixelAction
        {
            var indices = polygon.TriangleIndices;
            var vertices = polygon.Vertices;

            for (var i = 0; i < indices.Length; i += 3)
            {
                var v0 = vertices[indices[i]];
                var v1 = vertices[indices[i + 1]];
                var v2 = vertices[indices[i + 2]];

                var triMinY = (int)MathF.Floor(MathF.Min(v0.Y, MathF.Min(v1.Y, v2.Y)));
                var triMaxY = (int)MathF.Ceiling(MathF.Max(v0.Y, MathF.Max(v1.Y, v2.Y)));

                var startY = Math.Max(triMinY, clipMinY);
                var endY = Math.Min(triMaxY, clipMaxY);

                if (startY > endY)
                    continue;

                var minX = (int)MathF.Floor(MathF.Min(v0.X, MathF.Min(v1.X, v2.X)));
                var maxX = (int)MathF.Ceiling(MathF.Max(v0.X, MathF.Max(v1.X, v2.X)));

                // Precompute Edge Functions 
                double edge1Y = v1.Y - v0.Y;
                double edge1X = v1.X - v0.X;
                double edge2Y = v2.Y - v1.Y;
                double edge2X = v2.X - v1.X;
                double edge3Y = v0.Y - v2.Y;
                double edge3X = v0.X - v2.X;
                
                var startYCenter = startY + 0.51;
                var minXCenter = minX + 0.51;
                
                // Calculate rowVal based on startY (the clipped top), not the triangle top.
                var rowVal1 = edge1X * (startYCenter - v0.Y) - edge1Y * (minXCenter - v0.X);
                var rowVal2 = edge2X * (startYCenter - v1.Y) - edge2Y * (minXCenter - v1.X);
                var rowVal3 = edge3X * (startYCenter - v2.Y) - edge3Y * (minXCenter - v2.X);

                for (var y = startY; y <= endY; y++)
                {
                    var w1 = rowVal1;
                    var w2 = rowVal2;
                    var w3 = rowVal3;

                    for (var x = minX; x <= maxX; x++)
                    {
                        var neg = (w1 < 0) | (w2 < 0) | (w3 < 0);
                        var pos = (w1 > 0) | (w2 > 0) | (w3 > 0);

                        if (!(neg && pos))
                            action.Invoke(x, y);

                        w1 -= edge1Y;
                        w2 -= edge2Y;
                        w3 -= edge3Y;
                    }

                    rowVal1 += edge1X;
                    rowVal2 += edge2X;
                    rowVal3 += edge3X;
                }
            }
        }
    }
}

public interface IPixelAction
{
    void Invoke(int x, int y);
}