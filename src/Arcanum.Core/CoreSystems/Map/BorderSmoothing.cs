using System.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Map;

public static class BorderSmoothing
{
    public static void SmoothBorders(this BorderSegmentDirectional segmentDirectional)
    {
        // Concurrency Check needs to be reversed because of the holes
        if (!segmentDirectional.IsForward)
            return;

        var borderPoints = segmentDirectional.Segment.Points;

        if (borderPoints.Count < 4)
            return;

        // 2. Iterate forward using a while loop to handle index shifting
        // We look at 4 points at a time: p0 -> p1 -> p2 -> p3
        var i = 0;
        
        while (i <= borderPoints.Count - 4)
        {
            var p0 = borderPoints[i];
            var p1 = borderPoints[i + 1];
            var p2 = borderPoints[i + 2];
            var p3 = borderPoints[i + 3];

            // Calculate "Unit" vectors (Directions)
            // We use Math.Sign because the magnitude doesn't matter, only direction.
            // This also prevents crashes if a diagonal is processed.
            var dx1 = Math.Sign(p1.X - p0.X);
            var dy1 = Math.Sign(p1.Y - p0.Y);

            var dx2 = Math.Sign(p2.X - p1.X);
            var dy2 = Math.Sign(p2.Y - p1.Y);

            var dx3 = Math.Sign(p3.X - p2.X);
            var dy3 = Math.Sign(p3.Y - p2.Y);

            // 3. Check for the "Staircase" pattern (Zig-Zag)
            // Pattern: The first segment and the third segment point in the exact same direction.
            // Example: North (0, 1) -> East (1, 0) -> North (0, 1)
            var isStaircase = (dx1 == dx3 && dy1 == dy3);

            // Ensure the middle segment is actually a 90-degree turn (not a straight line)
            // If dx1 == dx2 && dy1 == dy2, it's just a straight line of 3 points (redundant points),
            // which we can also clean up, but assuming "CornerPoints only" input, this checks for the turn.
            var isTurn = (dx1 != dx2 || dy1 != dy2);

            if (isStaircase && isTurn)
            {
                // We found a step: p0 -> p1 -> p2 -> p3
                // Smooth it by removing p1 and p2, connecting p0 directly to p3.
                
                borderPoints.RemoveAt(i + 1); // Removes p1
                borderPoints.RemoveAt(i + 1); // Removes p2 (which shifted to i+1)

                // 4. Backtracking
                // After removing, we might have created a new continuous stair with the previous segment.
                // Step back one index to check the newly formed connection against the previous point.
                if (i > 0) 
                    i--;
            }
            else
            {
                // No pattern found, move to the next point
                i++;
            }
        }
    }
}