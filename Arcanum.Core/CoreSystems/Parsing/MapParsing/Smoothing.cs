namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public static class Smoothing
{
    public static List<PointF> ChaikinSmooth(List<Point> points, int iterations = 1)
    {
        if (points.Count < 2) return points.Select(p => new PointF(p.X, p.Y)).ToList();

        var resultPoints = new List<PointF>(points.Count * 2);
        if (points.Count > 0) resultPoints.Add(new(points[0]));

        for (int j = 0; j < points.Count - 1; j++)
        {
            PointF p1 = new(points[j]);
            PointF p2 = new(points[j + 1]);

            PointF q = 0.75f * p1 + 0.25f * p2;
            PointF r = 0.25f * p1 + 0.75f * p2;

            resultPoints.Add(q);
            resultPoints.Add(r);
        }
        
        if (points.Count > 1) resultPoints.Add(resultPoints[^1]);

        for (int i = 1; i < iterations; i++)
        {
            if (resultPoints.Count < 2) return resultPoints;

            var newPoints = new List<PointF>(resultPoints.Count * 2);
            if (resultPoints.Count > 0) newPoints.Add(resultPoints[0]);

            for (int j = 0; j < resultPoints.Count - 1; j++)
            {
                PointF p1 = resultPoints[j];
                PointF p2 = resultPoints[j + 1];

                PointF q = 0.75f * p1 + 0.25f * p2;
                PointF r = 0.25f * p1 + 0.75f * p2;

                newPoints.Add(q);
                newPoints.Add(r);
            }
            
            if (resultPoints.Count > 1) newPoints.Add(resultPoints[resultPoints.Count - 1]);
            resultPoints = newPoints;
        }

        return resultPoints;
    }

    public static List<PointF> ChaikinSmooth(List<PointF> points, int iterations = 1)
    {
        if (points.Count > 1) points.Add(points[^1]);

        for (int i = 0; i < iterations; i++)
        {
            if (points.Count < 2) return points;

            var newPoints = new List<PointF>(points.Count * 2);
            if (points.Count > 0) newPoints.Add(points[0]);

            for (int j = 0; j < points.Count - 1; j++)
            {
                PointF p1 = points[j];
                PointF p2 = points[j + 1];

                PointF q = 0.75f * p1 + 0.25f * p2;
                PointF r = 0.25f * p1 + 0.75f * p2;

                newPoints.Add(q);
                newPoints.Add(r);
            }
            
            if (points.Count > 1) newPoints.Add(points[points.Count - 1]);
            points = newPoints;
        }

        return points;
    }

    /// <summary>
    /// Calculates the perpendicular distance from a point to a line segment.
    /// </summary>
    private static double PerpendicularDistance(PointF point, PointF lineStart, PointF lineEnd)
    {
        if (lineStart == lineEnd)
        {
            return (point - lineStart).Magnitude();
        }
        return Math.Abs((lineEnd.X - lineStart.X) * (lineStart.Y - point.Y) - (lineStart.X - point.X) * (lineEnd.Y - lineStart.Y)) /
               (lineEnd - lineStart).Magnitude();
    }
    
    /// <summary>
    /// Simplifies a polyline using the Ramer-Douglas-Peucker algorithm.
    /// </summary>
    public static List<PointF> RamerDouglasPeucker(List<PointF> points, double epsilon)
    {
        if (points == null || points.Count < 2) return points;
    
        double dmax = 0.0;
        int index = 0;
        int end = points.Count - 1;
    
        for (int i = 1; i < end; i++)
        {
            double d = PerpendicularDistance(points[i], points[0], points[end]);
            if (d > dmax)
            {
                index = i;
                dmax = d;
            }
        }
    
        if (dmax > epsilon)
        {
            List<PointF> recResults1 = RamerDouglasPeucker(points.GetRange(0, index + 1), epsilon);
            List<PointF> recResults2 = RamerDouglasPeucker(points.GetRange(index, points.Count - index), epsilon);
    
            var result = new List<PointF>(recResults1.Take(recResults1.Count - 1));
            result.AddRange(recResults2);
            return result;
        }
        else
        {
            return new List<PointF> { points[0], points[end] };
        }
    }
    
    public static PointF[] SmoothSegment(List<Point> segment, Node start, Node end)
    {
        segment.Insert(0, start.Position);
        segment.Add(end.Position);
        var simplified = ChaikinSmooth(segment, 2);
        simplified = RamerDouglasPeucker( simplified,1);
        simplified = ChaikinSmooth(simplified, 6);
        simplified = RamerDouglasPeucker(simplified, 0.1);
        simplified.RemoveAt(0);
        simplified.RemoveAt(simplified.Count - 1);
        return simplified.ToArray();
        return segment.Select(p => new PointF(p.X, p.Y)).ToArray();
    }
}