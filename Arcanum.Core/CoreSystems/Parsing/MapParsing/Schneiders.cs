// SchneidersFitterVortice.cs

using System.Numerics;
// Use the Vector2 from Vortice or System.Numerics

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public static class SchneidersFitterVortice
{
    private static float GetApproximateBezierLength(Vector2[] p, int resolution = 100)
    {
        float length = 0.0f;
        Vector2 lastPoint = GetPointOnBezier(0, p);

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector2 currentPoint = GetPointOnBezier(t, p);
            length += Vector2.Distance(currentPoint, lastPoint);
            lastPoint = currentPoint;
        }

        return length;
    }
    
    /// <summary>
    /// Fits a cubic Bézier curve, samples it, and simplifies the result.
    /// This implementation has NO EXTERNAL DEPENDENCIES beyond System.Numerics or Vortice.
    /// </summary>
    /// <param name="points">The list of original points.</param>
    /// <param name="desiredAverageDistance">The number of points to sample from the fitted Bézier curve.</param>
    /// <param name="epsilon">The tolerance for the Ramer-Douglas-Peucker simplification.</param>
    /// <returns>A simplified list of points representing the fitted curve.</returns>
    public static List<Vector2> FitAndSimplify(IReadOnlyList<Vector2> points, float desiredAverageDistance, float epsilon)
    {
        if (points.Count < 2)
        {
            return points?.ToList() ?? new List<Vector2>();
        }

        // 1. Fit a Cubic Bézier curve using a direct least-squares implementation
        Vector2[] controlPoints = FitCubicBezier(points);
        
        // 2. Calculate the number of points needed based on the curve's length and desired density
        float curveLength = GetApproximateBezierLength(controlPoints);
        int numSampledPoints = (int)Math.Max(2, Math.Round(curveLength / desiredAverageDistance));

        // 3. Sample new, smooth points from the fitted curve
        var sampledPoints = new List<Vector2>(numSampledPoints);
        // Ensure the loop runs correctly even if numSampledPoints is 2
        for (int i = 0; i < numSampledPoints; i++)
        {
            float t = (numSampledPoints > 1) ? (float)i / (numSampledPoints - 1) : 0;
            sampledPoints.Add(GetPointOnBezier(t, controlPoints));
        }

        // 3. Simplify the newly sampled points
        return RamerDouglasPeucker(sampledPoints, epsilon);
    }

    /// <summary>
    /// Finds the four control points for a cubic Bézier curve that best fits the given points.
    /// Solves the least-squares problem directly without a matrix library.
    /// </summary>
    private static Vector2[] FitCubicBezier(IReadOnlyList<Vector2> points)
    {
        var p0 = points[0];
        var p3 = points[points.Count - 1];

        // Parameterize the points based on their chord length
        float[] t = Parameterize(points);

        // Build the components for the (A^T * A) matrix and (A^T * B) vector
        float c00 = 0.0f, c01 = 0.0f, c11 = 0.0f;
        float x0 = 0.0f, x1 = 0.0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            float ti = t[i];
            float oneMinusT = 1.0f - ti;
            
            float b1 = 3 * ti * oneMinusT * oneMinusT;
            float b2 = 3 * ti * ti * oneMinusT;

            c00 += b1 * b1;
            c01 += b1 * b2;
            c11 += b2 * b2;
            
            Vector2 p_i_term = points[i] - (oneMinusT * oneMinusT * oneMinusT * p0 + ti * ti * ti * p3);
            
            x0 += b1 * p_i_term.X;
            x1 += b2 * p_i_term.X;
        }

        // Now do the same for the Y components
        float y0 = 0.0f, y1 = 0.0f;
        for (int i = 1; i < points.Count - 1; i++)
        {
            float ti = t[i];
            float oneMinusT = 1.0f - ti;
            
            float b1 = 3 * ti * oneMinusT * oneMinusT;
            float b2 = 3 * ti * ti * oneMinusT;

            Vector2 p_i_term = points[i] - (oneMinusT * oneMinusT * oneMinusT * p0 + ti * ti * ti * p3);
            
            y0 += b1 * p_i_term.Y;
            y1 += b2 * p_i_term.Y;
        }
        
        // Solve the 2x2 system of linear equations using Cramer's rule
        float det = c00 * c11 - c01 * c01;
        
        Vector2 p1, p2;
        if (Math.Abs(det) > 1e-6)
        {
            float detInv = 1.0f / det;
            p1 = new Vector2((c11 * x0 - c01 * x1) * detInv, (c11 * y0 - c01 * y1) * detInv);
            p2 = new Vector2((c00 * x1 - c01 * x0) * detInv, (c00 * y1 - c01 * y0) * detInv);
        }
        else
        {
            // If the matrix is singular (e.g., collinear points), use a simple heuristic
            p1 = p0 + (p3 - p0) * (1.0f / 3.0f);
            p2 = p0 + (p3 - p0) * (2.0f / 3.0f);
        }

        return new[] { p0, p1, p2, p3 };
    }
    
    // --- Helper functions for Bézier sampling and RDP simplification ---
    // (These are optimized and use squared distances to avoid Sqrt)

    private static float[] Parameterize(IReadOnlyList<Vector2> points)
    {
        float[] t = new float[points.Count];
        var lengths = new float[points.Count];
        lengths[0] = 0;
        
        for (int i = 1; i < points.Count; i++)
        {
            lengths[i] = lengths[i-1] + Vector2.Distance(points[i], points[i-1]);
        }

        float totalLength = lengths[lengths.Length - 1];
        if (totalLength > 0)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                t[i] = lengths[i] / totalLength;
            }
        }
        return t;
    }

    private static Vector2 GetPointOnBezier(float t, Vector2[] p)
    {
        float oneMinusT = 1.0f - t;
        float oneMinusT_sq = oneMinusT * oneMinusT;
        float t_sq = t * t;
        
        return oneMinusT_sq * oneMinusT * p[0] +
               3 * oneMinusT_sq * t * p[1] +
               3 * oneMinusT * t_sq * p[2] +
               t_sq * t * p[3];
    }

    private static List<Vector2> RamerDouglasPeucker(IReadOnlyList<Vector2> points, float epsilon)
    {
        if (points.Count < 3) return points.ToList();
        
        var finalResult = new List<Vector2>();
        // Using squared epsilon to avoid Sqrt in the loop
        RdpRecursive(points, 0, points.Count - 1, epsilon * epsilon, finalResult);
        finalResult.Add(points[points.Count-1]); // Add the last point
        return finalResult;
    }
    
    private static void RdpRecursive(IReadOnlyList<Vector2> points, int first, int last, float epsilonSq, List<Vector2> result)
    {
        float maxDistSq = 0;
        int index = 0;

        for (int i = first + 1; i < last; i++)
        {
            float dSq = PerpendicularDistanceSq(points[i], points[first], points[last]);
            if (dSq > maxDistSq)
            {
                index = i;
                maxDistSq = dSq;
            }
        }

        if (maxDistSq > epsilonSq)
        {
            RdpRecursive(points, first, index, epsilonSq, result);
            RdpRecursive(points, index, last, epsilonSq, result);
        }
        else
        {
            result.Add(points[first]);
        }
    }

    private static float PerpendicularDistanceSq(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        float dx = lineEnd.X - lineStart.X;
        float dy = lineEnd.Y - lineStart.Y;
        
        if (dx == 0 && dy == 0)
        {
            return Vector2.DistanceSquared(point, lineStart);
        }

        float numerator = dy * point.X - dx * point.Y - lineStart.X * lineEnd.Y + lineEnd.X * lineStart.Y;
        return (numerator * numerator) / (dx * dx + dy * dy);
    }
}