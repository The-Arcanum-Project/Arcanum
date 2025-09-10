#define ENABLE_VISUAL_TRACING

using System.Diagnostics;
using System.Drawing.Imaging;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public enum Direction
{
    North,
    East,
    South,
    West,
}

public interface ICoordinateAdder
{
    public void AddTo(List<Point> points);
}

public readonly record struct Point(int X, int Y) : ICoordinateAdder
{
    public static Point Empty => new(int.MinValue, int.MinValue);

    public (int, int) ToTuple() => (X, Y);

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public void AddTo(List<Point> points)
    {
        points.Add(this);
    }

    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.X + p2.X, p1.Y + p2.Y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static Point operator *(Point p, int scalar)
    {
        return new Point(p.X * scalar, p.Y * scalar);
    }

    public static Point operator /(Point p, int scalar)
    {
        if (scalar == 0) throw new DivideByZeroException("Cannot divide by zero.");
        return new Point(p.X / scalar, p.Y / scalar);
    }

    public static int operator *(Point p1, Point p2)
    {
        return p1.X * p2.X + p1.Y * p2.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}

public class BorderSegment
{
    public List<Point> Points { get; } = [];
}

public readonly struct BorderSegmentDirectional(BorderSegment segment, bool isForward) : ICoordinateAdder
{
    public readonly BorderSegment Segment = segment;
    public readonly bool IsForward = isForward;

    public void AddToList(List<Point> points)
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

    public void AddTo(List<Point> points)
    {
        AddToList(points);
    }
}

public class Polygon(int color)
{
    public int Color { get; } = color;
    public List<ICoordinateAdder> Segments { get; } = [];
    public List<Polygon> Holes { get; } = [];

    public List<Point> GetAllPoints()
    {
        var points = new List<Point>();
        foreach (var segment in Segments)
            segment.AddTo(points);

        return points;
    }
}

// There are 4 types of nodes:
// 1. A node that was not discovered yet
// 2. A node that wasn't discovered yet and has two discovered segments, and we approach it from an undiscovered segment
// 3. Same as 2, but we approach it from a discovered segment and leave it from the undiscovered segment
// 4. A node where all segments are discovered

// So what information can we cache to speed up the tracing?
// When discovering a node, we can cache the segment that would be used if case 2 happens.
//  This would be the same segment we have used to approach the node but reversed.
// For case 3, we cannot really cache a segment directly but maybe a direction or a point?
//   Depends on the implementation of the following algorithm.
// For case 4, we have a similar situation as in case 2. However, we cache the segment that was used to approach the node in case 2.
//   We only need to cache a single segment and just update it when we approach the node from a different segment.
//
// So for a 3-edge node:
// When discovered, we cache the segment that was used to approach the node as well as the direction we do not continue to trace.
// If we find the node again, we know if the segment was already discovered or not, since we went here through the previous node.
// So case 1 we approach through an undiscovered segment:
// We can use the cached segment to continue tracing in the direction we came from.
// But we cache the segment that was used to approach the node.

// Case 2 we approach through a discovered segment:
// We use the direction of the segment that was used to approach the node unless the node was already visited 2 times, since case 1 happened previously and therefore we can use the cached segment.

// Are the border nodes different from the inner nodes?
// The problem is that we trace the first node from the edge of the image and not in the clockwise fashion.
// They are different since both paths to it are undiscovered.
// Therefore, if we hit such a node, we first have to decide which direction to take (clockwise).
// Case 1: We move to the edge: Use the cached segment to continue tracing

// Case 2: We trace in the other undiscovered direction

// Case 3: We approach the node from the edge of the image, and both segments are not discovered yet.

// Case 4: We approach the node from the edge

public class CacheNodeInfo(Node? node, BorderSegmentDirectional? segment, Direction dir)
{
    public Direction Dir = dir;
    public BorderSegmentDirectional? Segment = segment;
    public Node? Node = node;
}

/// <summary>
/// Represents a node on the border of the image.
/// Caches the segment used to approach the node and the segment parsed from it.
/// </summary>
public class Node : ICoordinateAdder
{
#if DEBUG
    private static int _totalNodes = 0;
    private int _nodeId;

    public override string ToString()
    {
        return $"Node {_nodeId}";
    }

    public void AddTo(List<Point> points)
    {
        points.Add(new Point(XPos, YPos));
    }

#endif

    public readonly Point Position;
    public int XPos => Position.X;
    public int YPos => Position.Y;


    public CacheNodeInfo CachedSegment1;
    public CacheNodeInfo CachedSegment2;
    public CacheNodeInfo CachedSegment3;
    public bool Visited1 = false;
    public bool Visited2 = false;
    public bool Visited3 = false;

    public void SetDirection(Direction dir)
    {
        if (CachedSegment1.Dir == dir)
            Visited1 = true;
        else if (CachedSegment2.Dir == dir)
            Visited2 = true;
        else if (CachedSegment3.Dir == dir)
            Visited3 = true;
        else
            throw new InvalidOperationException($"Node does not have a segment in direction {dir}");
    }

    public bool TestDirection(Direction dir)
    {
        if (CachedSegment1.Dir == dir)
            return Visited1;
        else if (CachedSegment2.Dir == dir)
            return Visited2;
        else if (CachedSegment3.Dir == dir)
            return Visited3;
        else
            throw new InvalidOperationException($"Node does not have a segment in direction {dir}");
    }

    /// <summary>
    /// Represents a node on the border of the image.
    /// Caches the segment used to approach the node and the segment parsed from it.
    /// </summary>
    public Node(CacheNodeInfo cachedSegment1, CacheNodeInfo cachedSegment2, CacheNodeInfo cachedSegment3, int xPos,
        int yPos)
    {
#if DEBUG
        _nodeId = _totalNodes;
        _totalNodes++;
#endif
        
        CachedSegment1 = cachedSegment1;
        CachedSegment2 = cachedSegment2;
        CachedSegment3 = cachedSegment3;
        Position = new(xPos, yPos);
    }

    public CacheNodeInfo GetSegment(Direction dir)
    {
        if (CachedSegment1.Dir == dir)
            return CachedSegment1;
        if (CachedSegment2.Dir == dir)
            return CachedSegment2;
        if (CachedSegment3.Dir == dir)
            return CachedSegment3;
        throw new InvalidOperationException($"Node does not have a segment in direction {dir}");
    }
    
    public bool TryGetSegment(Direction dir, out CacheNodeInfo? segment)
    {
        segment = null;
        if (CachedSegment1.Dir == dir)
            segment = CachedSegment1;
        else if (CachedSegment2.Dir == dir)
            segment = CachedSegment2;
        else if (CachedSegment3.Dir == dir)
            segment = CachedSegment3;
        else
            return false;
        return true;
    }

    public void Visit(ref Direction direction, Node inputNode, BorderSegmentDirectional input,
        out BorderSegmentDirectional? segment, out Node? node)
    {
        Point point;
        if (input.Segment.Points.Count == 0)
        {
            Visit(ref direction, out segment, out node);
            return;
        }

        if (input.IsForward)
            point = input.Segment.Points[^1];
        else
            point = input.Segment.Points[0];

        Visit(ref direction, (inputNode.XPos, inputNode.YPos), point.X, point.Y, out segment, out node);
    }

    public void Visit(ref Direction direction, (int, int) nodepos, int x, int y, out BorderSegmentDirectional? segment,
        out Node? node)
    {
        // Get direction based of the difference between the two points
        var dx = nodepos.Item1 - x;
        var dy = nodepos.Item2 - y;
        if (dx > 0 && dy == 0)
            direction = Direction.East;
        else if (dx < 0 && dy == 0)
            direction = Direction.West;
        else if (dx == 0 && dy > 0)
            direction = Direction.South;
        else if (dx == 0 && dy < 0)
            direction = Direction.North;
        else
            throw new InvalidOperationException($"Invalid movement from {nodepos} to ({x}, {y})");
        Visit(ref direction, out segment, out node);
    }
    
    //TODO Clean up
    public void Visit(ref Direction direction, out BorderSegmentDirectional? segment, out Node? node)
    {
        var newDirection = direction.RotateRight();
        if (CachedSegment1.Dir == newDirection)
        {
            segment = CachedSegment1.Segment;
            node = CachedSegment1.Node;
            direction = newDirection;
        }
        else if (CachedSegment2.Dir == newDirection)
        {
            segment = CachedSegment2.Segment;
            node = CachedSegment2.Node;
            direction = newDirection;
        }
        else if (CachedSegment3.Dir == newDirection)
        {
            segment = CachedSegment3.Segment;
            node = CachedSegment3.Node;
            direction = newDirection;
        }
        else
        {
            if (CachedSegment1.Dir == direction)
            {
                segment = CachedSegment1.Segment;
                node = CachedSegment1.Node;
            }
            else if (CachedSegment2.Dir == direction)
            {
                segment = CachedSegment2.Segment;
                node = CachedSegment2.Node;
            }
            else if (CachedSegment3.Dir == direction)
            {
                segment = CachedSegment3.Segment;
                node = CachedSegment3.Node;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Node was not visited in the direction {direction}. Cached segments: {CachedSegment1.Dir}, {CachedSegment2.Dir}, {CachedSegment3.Dir}");
            }
        }
    }

    /// <summary>
    /// Checks given the direction if any segment is cached and returns it with the node it leads to.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="segment"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool VisitNew(ref Direction direction, out BorderSegmentDirectional? segment, out Node? node)
    {
        // Not only check the right direction, since it is a T-shaped intersection a possible path is straight ahead.
        var newDirection = direction.RotateRight();
        if (TryGetSegment(newDirection, out var cache))
        {
            direction = newDirection;
        }
        else
        {
            cache = GetSegment(direction);
        }

        if (cache!.Segment.HasValue)
        {
            segment = cache.Segment;
            node = cache.Node;
            return true;
        }

        segment = null;
        node = null;
        return false;
    }
    
    public bool VisitNew(ref Direction direction, Node inputNode, BorderSegmentDirectional input,
        out BorderSegmentDirectional? segment, out Node? node)
    {
        Point point;
        if (input.Segment.Points.Count == 0)
        {
            return VisitNew(ref direction, out segment, out node);
        }

        if (input.IsForward)
            point = input.Segment.Points[^1];
        else
            point = input.Segment.Points[0];

        return VisitNew(ref direction, (inputNode.XPos, inputNode.YPos), point.X, point.Y, out segment, out node);
    }

    public bool VisitNew(ref Direction direction, (int, int) nodepos, int x, int y, out BorderSegmentDirectional? segment,
        out Node? node)
    {
        // Get direction based of the difference between the two points
        var dx = nodepos.Item1 - x;
        var dy = nodepos.Item2 - y;
        if (dx > 0 && dy == 0)
            direction = Direction.East;
        else if (dx < 0 && dy == 0)
            direction = Direction.West;
        else if (dx == 0 && dy > 0)
            direction = Direction.South;
        else if (dx == 0 && dy < 0)
            direction = Direction.North;
        else
            throw new InvalidOperationException($"Invalid movement from {nodepos} to ({x}, {y})");
        return VisitNew(ref direction, out segment, out node);
    }
}

/// <summary>
/// A node which represents one with a connection to the border of the image.
/// Has to be different due to the way we trace those nodes first and need to cache two segments.
/// </summary>
/// <param name="borderSegmentCache"></param>
/// <param name="direction"></param>
/// <summary>
/// Class for tracing of the map.
/// Uses a hybrid scanline and contour tracing algorithm.
/// </summary>
public unsafe class MapTracing
{
    private const int ALPHA = 255 << 24;
    private IDebugDrawer drawer = null!; // This will be set later, so we can draw the traced edges.

    public const int OUTSIDE_COLOR = 0x000000;

    private int width;
    private int height;
    private int stride;
    private IntPtr scan0;

    public Dictionary<(int, int), Node> NodeCache { get; } = new();

    public int GetColor(int x, int y)
    {
        var row = (byte*)scan0 + y * stride;
        var xTimesThree = x * 3;
        return ALPHA // Alpha: Fully opaque
               |
               (row[xTimesThree + 2] << 16) // Red
               |
               (row[xTimesThree + 1] << 8) // Green
               |
               row[xTimesThree];
    }

    public int GetColorWithOutsideCheck(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return OUTSIDE_COLOR;
        }

        return GetColor(x, y);
    }

    /// <summary>
    /// Traces the edges of the image in a counter-clockwise manner.
    /// Finds all the nodes on the border of the image.
    /// </summary>
    /// <returns></returns>
    public void TraceImageEdge()
    {
        var lastColor = GetColor(0, 0);
        var lastRow = height - 1;
        var lastColumn = width - 1;

        // The first segment starts at the origin.
        var firstSegment = new BorderSegment();
        Node firstNode = null;

        Node lastNode = null;
        BorderSegment lastSegment = null!;

        // Top Left Corner
        firstSegment.Points.Add(new Point(0, 0));
        var currentSegment = firstSegment;


        // Local function to process a completed segment when a new node is found.
        void FinalizeSegmentAndStartNew(int x, int y, int color, Direction d)
        {
            // Add the new node's location to complete the current segment's path.
            //currentSegment.Points.Add(new Point(x, y));

#if ENABLE_VISUAL_TRACING
            // Draw the complete path of the segment that just ended.
            for (var i = 0; i < currentSegment.Points.Count - 1; i++)
            {
                var p1 = currentSegment.Points[i];
                var p2 = currentSegment.Points[i + 1];
                drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }

            drawer.DrawNode(x, y);
#endif

            // Create the new node and a new segment starting at this node.
            var newSegment = new BorderSegment();
            //newSegment.Points.Add(new Point(x, y));
            Node node;
            if (lastNode != null)
            {
                node = new(
                    new CacheNodeInfo(lastNode, new BorderSegmentDirectional(currentSegment, false), d.RotateLeft()),
                    new CacheNodeInfo(null, null, d),
                    new CacheNodeInfo(null, null, d.RotateRight()), x, y);
                lastNode.CachedSegment3.Node = node;
                lastNode.CachedSegment3.Segment = new(currentSegment, true);
            }
            else
            {
                node = new(
                    new CacheNodeInfo(null, null, d.RotateLeft()),
                    new CacheNodeInfo(null, null, d),
                    new CacheNodeInfo(null, null, d.RotateRight()), x, y);
                firstNode = node;
            }


            NodeCache.Add((x, y), node);
            lastSegment = currentSegment;
            lastNode = node;
            currentSegment = newSegment;
            lastColor = color;
        }

        // --- Traverse Counter-Clockwise ---

        // Left Edge (top to bottom)
        for (var y = 1; y <= lastRow; y++)
        {
            var color = GetColor(0, y);
            if (color != lastColor)
            {
                FinalizeSegmentAndStartNew(0, y, color, Direction.East);
            }
        }

        // Bottom Left Corner
        currentSegment.Points.Add(new Point(0, height));

        // Bottom Edge (left to right)
        for (var x = 1; x <= lastColumn; x++)
        {
            var color = GetColor(x, lastRow);
            if (color != lastColor)
            {
                FinalizeSegmentAndStartNew(x, height, color, Direction.North);
            }
        }

        // Bottom Right Corner
        currentSegment.Points.Add(new Point(width, height));

        // Right Edge (bottom to top)
        for (var y = lastRow; y > 0; y--)
        {
            var color = GetColor(lastColumn, y - 1);
            if (color != lastColor)
            {
                FinalizeSegmentAndStartNew(width, y, color, Direction.West);
            }
        }

        // Top Right Corner
        currentSegment.Points.Add(new Point(width, 0));

        // Top Edge (right to left)
        for (var x = lastColumn; x > 0; x--)
        {
            var color = GetColor(x - 1, 0);
            if (color != lastColor)
            {
                FinalizeSegmentAndStartNew(x, 0, color, Direction.South);
            }
        }

        currentSegment.Points.AddRange(firstSegment.Points);
        firstSegment = currentSegment;

        if (lastNode == null)
        {
            throw new InvalidOperationException(
                "No nodes were on the border of the image. We currently do not support maps without nodes on the border.");
        }

        lastNode.CachedSegment3.Node = firstNode;
        lastNode.CachedSegment3.Segment = new(currentSegment, true);
        firstNode!.CachedSegment1.Node = lastNode;
        firstNode.CachedSegment1.Segment = new(currentSegment, false);
#if ENABLE_VISUAL_TRACING
        // Draw the complete path of the segment that just ended.
        for (var i = 0; i < currentSegment.Points.Count - 1; i++)
        {
            var p1 = currentSegment.Points[i];
            var p2 = currentSegment.Points[i + 1];
            drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
        }
#endif
    }

    private bool MoveDirWithCheck(bool dir, bool sign, ref int xl, ref int yl, ref int xr, ref int yr,
        out int cache)
    {
        if (dir)
        {
            if (sign)
            {
                // Move right
                cache = xl++;
                xr = xl;
                return xr < width;
            }

            // Move left
            cache = xl--;
            xr = xl;
            return xr >= 0;
        }

        if (sign)
        {
            // Move down
            cache = yl++;
            yr = yl;
            return yr < height;
        }

        // Move up
        cache = yl--;
        yr = yl;
        return yr >= 0;
    }

    private void MoveDir(bool dir, bool sign, ref int xl, ref int yl, ref int xr, ref int yr,
        out int cache)
    {
        if (dir)
        {
            if (sign)
            {
                // Move right
                cache = xl++;
                xr = xl;
                return;
            }

            // Move left
            cache = xl--;
            xr = xl;
            return;
        }

        if (sign)
        {
            // Move down
            cache = yl++;
            yr = yl;
            return;
        }

        // Move up
        cache = yl--;
        yr = yl;
    }

    private void MoveGridPoint(bool dir, bool sign, ref int x, ref int y)
    {
        if (dir)
        {
            if (sign)
            {
                // Move right
                x++;
            }
            else
            {
                // Move left
                x--;
            }
        }

        else if (sign)
        {
            // Move down
            y++;
        }
        else
        {
            // Move up
            y--;
        }
    }

    public void DrawLine(ref int lastX, ref int lastY, int x, int y)
    {
        drawer.DrawLine(lastX, lastY, x, y);
        lastX = x;
        lastY = y;
    }


    public void TraceEdgeWithoutStartNode(int x, int y, Direction currentDirection)
    {
        BorderSegment segment = new();
        // Start without any node
        var (xl, yl, xr, yr) = DirectionHelper.GetStartPos(x, y, currentDirection);

        var lColor = GetColorWithOutsideCheck(xl, yl);
        var rColor = GetColorWithOutsideCheck(xr, yr);

        Polygon polygon = new Polygon(rColor);

        var (dir, sign) = currentDirection.GetDeltaMove();
        while (true)
        {
            MoveGridPoint(dir, sign, ref x, ref y);
            MoveDir(dir, sign, ref xl, ref yl, ref xr, ref yr, out var cache);
            var lTest = GetColorWithOutsideCheck(xl, yl);
            var rTest = GetColorWithOutsideCheck(xr, yr);

            if (lTest == lColor && rTest == rColor)
            {
                // Nothing should change
            }
            // Turn right
            else if (lTest == lColor && rTest == lColor)
            {
                // When we turn right, the current right pixel is the new left pixel
                xl = xr;
                yl = yr;
                // The old right pixel is the new right pixel
                if (dir)
                {
                    // Horizontal movement
                    xr = cache;
                }
                else
                {
                    // Vertical movement
                    yr = cache;
                }

                currentDirection = currentDirection.RotateRight();
                (dir, sign) = currentDirection.GetDeltaMove();
                //Probably not needed, but just to be sure
                lColor = rTest;
                segment.Points.Add(new Point(x, y));
            }
            // Turn left
            else if (lTest == rColor && rTest == rColor)
            {
                // When we turn left, the current left pixel is the new right pixel
                xr = xl;
                yr = yl;
                // The old left pixel is the new left pixel
                if (dir)
                {
                    // Horizontal movement
                    xl = cache;
                }
                else
                {
                    // Vertical movement
                    yl = cache;
                }

                currentDirection = currentDirection.RotateLeft();
                (dir, sign) = currentDirection.GetDeltaMove();
                segment.Points.Add(new Point(x, y));
                rColor = lTest;
            }
            else if (lTest == lColor || rTest == rColor)
            {
                var dir1 = currentDirection.Invert();
                Direction dir2 = currentDirection;
                Direction dir3 = currentDirection;
                if (lTest == lColor)
                {
                    dir2 = currentDirection.RotateRight();
                    //dir3 = currentDirection;
                }
                else if (rTest == rColor)
                {
                    //dir2 = currentDirection;
                    dir3 = currentDirection.RotateLeft();
                    ;
                }
                else if (rTest == lTest)
                {
                    dir2 = currentDirection.RotateRight();
                    dir3 = currentDirection.RotateLeft();
                }

                // ThreeEdgeNode
                if (!NodeCache.TryGetValue((x, y), out var node))
                {
                    var cache1 = new CacheNodeInfo(null, new BorderSegmentDirectional(segment, false), dir1);
                    var cache2 = new CacheNodeInfo(null, null, dir2);
                    var cache3 = new CacheNodeInfo(null, null, dir3);
                    node = new Node(cache1, cache2, cache3, x, y);
                }
                else
                {
                    node.CachedSegment1.Segment = new BorderSegmentDirectional(segment, false);
                    //node.CachedSegment1.Node = lastNode; Not the case since we start without a node
                }

                BorderSegmentDirectional? newSegment;
                Node? newNode = node;
                while (true)
                {
                    newNode!.Visit(ref currentDirection, out newSegment, out newNode);
                    if (newSegment.HasValue)
                    {
                        // We have already visited this node in the same direction, so we can continue using cache.
                    }
                    else
                    {
                        break;
                    }
                }

                (dir, sign) = currentDirection.GetDeltaMove();
            }
            else
            {
                // FourEdgeNode
            }
        }
        // TODO not finished need to rework. Logic to find start position again is not implemented yet.
    }

    public (Node, BorderSegmentDirectional, bool) TraceEdgeStartNode(int startx, int starty, Node startNode,
        Direction startDirection)
    {
        BorderSegment segment = new();
        Direction currentDirection = startDirection;
        BorderSegmentDirectional parsedSegment;
        // Start without any node
        var (xl, yl, xr, yr) = DirectionHelper.GetStartPos(startx, starty, currentDirection);
        var node = startNode;
        var lColor = GetColorWithOutsideCheck(xl, yl);
        var rColor = GetColorWithOutsideCheck(xr, yr);
        var found = true;
        var x = startx;
        var y = starty;
        var (dir, sign) = currentDirection.GetDeltaMove();
        while (true)
        {
            MoveGridPoint(dir, sign, ref x, ref y);
            MoveDir(dir, sign, ref xl, ref yl, ref xr, ref yr, out var cache);
            var lTest = GetColorWithOutsideCheck(xl, yl);
            var rTest = GetColorWithOutsideCheck(xr, yr);

            if (lTest == lColor && rTest == rColor)
            {
                // Nothing should change
            }
            // Turn right
            else if (lTest == lColor && rTest == lColor)
            {
                // When we turn right, the current right pixel is the new left pixel
                xl = xr;
                yl = yr;
                // The old right pixel is the new right pixel
                if (dir)
                {
                    // Horizontal movement
                    xr = cache;
                }
                else
                {
                    // Vertical movement
                    yr = cache;
                }

                currentDirection = currentDirection.RotateRight();
                (dir, sign) = currentDirection.GetDeltaMove();
                //Probably not needed, but just to be sure
                lColor = rTest;
                segment.Points.Add(new(x, y));
            }
            // Turn left
            else if (lTest == rColor && rTest == rColor)
            {
                // When we turn left, the current left pixel is the new right pixel
                xr = xl;
                yr = yl;
                // The old left pixel is the new left pixel
                if (dir)
                {
                    // Horizontal movement
                    xl = cache;
                }
                else
                {
                    // Vertical movement
                    yl = cache;
                }

                currentDirection = currentDirection.RotateLeft();
                (dir, sign) = currentDirection.GetDeltaMove();
                segment.Points.Add(new(x, y));
                rColor = lTest;
            }
            else if (lTest == lColor || rTest == rColor || rTest == lTest)
            {
                var dir1 = currentDirection.Invert();
                // ThreeEdgeNode
                if (!NodeCache.TryGetValue((x, y), out node))
                {
                    var dir2 = currentDirection;
                    var dir3 = currentDirection;
                    if (lTest == lColor)
                    {
                        dir2 = currentDirection.RotateRight();
                        //dir3 = currentDirection;
                    }
                    else if (rTest == rColor)
                    {
                        //dir2 = currentDirection;
                        dir3 = currentDirection.RotateLeft();
                    }
                    else if (rTest == lTest)
                    {
                        dir2 = currentDirection.RotateRight();
                        dir3 = currentDirection.RotateLeft();
                    }

                    found = false;

                    var cache1 = new CacheNodeInfo(startNode, new BorderSegmentDirectional(segment, false), dir1);
                    var cache2 = new CacheNodeInfo(null, null, dir2);
                    var cache3 = new CacheNodeInfo(null, null, dir3);
                    node = new Node(cache1, cache2, cache3, x, y);
                }
                else
                {
                    var cacheNodeInfo = node.GetSegment(dir1);
                    cacheNodeInfo.Node = startNode;
                    cacheNodeInfo.Segment = new BorderSegmentDirectional(segment, false);
                }

                var cacheNodeInfoStart = startNode.GetSegment(startDirection);
                cacheNodeInfoStart.Node = node;
                cacheNodeInfoStart.Segment = parsedSegment = new BorderSegmentDirectional(segment, true);

#if ENABLE_VISUAL_TRACING

                for (var i = 0; i < segment.Points.Count - 1; i++)
                {
                    var p1 = segment.Points[i];
                    var p2 = segment.Points[i + 1];
                    drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
                }

                if (segment.Points.Count > 0)
                {
                    drawer.DrawLine(startx, starty, segment.Points[0].X, segment.Points[0].Y);
                    drawer.DrawLine(segment.Points[^1].X, segment.Points[^1].Y, x, y);
                }
                else
                    drawer.DrawLine(startx, starty, x, y);

                drawer.DrawNode(x, y);
#endif
                break;
            }
            else
            {
                throw new InvalidOperationException($"Encountered a four-edge node, which is not supported yet. ({x}, {y})");
                // FourEdgeNode
                parsedSegment = new BorderSegmentDirectional();
                break;
            }
        }

        return (node, parsedSegment, found);
    }


    public (int x, int y) TraceSingleEdgeOld(int x, int y, Direction d)
    {
        var segment = new BorderSegment();

        segment.Points.Add(new Point(x, y));
        var currentDirection = d;

        var (xl, yl, xr, yr) = DirectionHelper.GetStartPos(x, y, d);

        var lColor = GetColor(xl, yl);
        var rColor = GetColor(xr, yr);

        // Maybe optimize to a bool with x or y and inc or dec
        var (dir, sign) = d.GetDeltaMove();
        // We start the loop with the first two pixels, which we know are valid for a border.
        // We do not need to cache the position any longer, so we take a step into the direction of the edge.
        while (true)
        {
            //Console.WriteLine($"Tracing edge at ({x}, {y}) ({xl}, {yl}, {xr}, {yr}) with direction {currentDirection} and colors ({lColor}, {rColor})");
            // Move a step in the current direction
            MoveGridPoint(dir, sign, ref x, ref y);
            if (!MoveDirWithCheck(dir, sign, ref xl, ref yl, ref xr, ref yr, out var cache))
            {
                segment.Points.Add(new Point(x, y));
                // We have reached the edge of the image
                if (!NodeCache.TryGetValue((x, y), out var node))
                {
                    // We have a node at this position, so we can add the segment to it.
                    //node = new Node(new BorderSegmentDirectional(segment, true), currentDirection);
                    NodeCache[(x, y)] = node;
                }

                //NodeCache[(x, y)] = new Node(new BorderSegmentDirectional(segment, true), currentDirection);
#if ENABLE_VISUAL_TRACING
                for (var i = 0; i < segment.Points.Count - 1; i++)
                {
                    var p1 = segment.Points[i];
                    var p2 = segment.Points[i + 1];
                    drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
                }

                drawer.DrawNode(x, y, unchecked((int)0xFFFFFF00));
#endif
                // TODO: Remove the node so it is not traced again.
                // Need to figure out a good way while iterating over the nodes
                return (x, y);
            }

            var lTest = GetColor(xl, yl);
            var rTest = GetColor(xr, yr);
            // Go straight
            if (lTest == lColor && rTest == rColor)
            {
                // Nothing should change
            }
            // Turn right
            else if (lTest == lColor && rTest == lColor)
            {
                // When we turn right, the current right pixel is the new left pixel
                xl = xr;
                yl = yr;
                // The old right pixel is the new right pixel
                if (dir)
                {
                    // Horizontal movement
                    xr = cache;
                }
                else
                {
                    // Vertical movement
                    yr = cache;
                }

                currentDirection = currentDirection.RotateRight();
                (dir, sign) = currentDirection.GetDeltaMove();
                //Probably not needed, but just to be sure
                lColor = rTest;
                segment.Points.Add(new Point(x, y));
            }
            // Turn left
            else if (lTest == rColor && rTest == rColor)
            {
                // When we turn left, the current left pixel is the new right pixel
                xr = xl;
                yr = yl;
                // The old left pixel is the new left pixel
                if (dir)
                {
                    // Horizontal movement
                    xl = cache;
                }
                else
                {
                    // Vertical movement
                    yl = cache;
                }

                currentDirection = currentDirection.RotateLeft();
                (dir, sign) = currentDirection.GetDeltaMove();
                segment.Points.Add(new Point(x, y));
                rColor = lTest;
            }
            else
            {
                segment.Points.Add(new Point(x, y));
                // We have found a node, so we can stop tracing this edge.
                // Add the segment to the node cache.
                // Still need a good method to get the node position.
                // Check if we have a node at this position
                if (!NodeCache.TryGetValue((x, y), out var node))
                {
                    // We have a node at this position, so we can add the segment to it.
                    //node = new Node(new BorderSegmentDirectional(segment, true), currentDirection);
                    //NodeCache[(x, y)] = node;
                }

                //NodeCache[(x, y)] = new Node(new BorderSegmentDirectional(segment, true), currentDirection);
#if ENABLE_VISUAL_TRACING
                for (var i = 0; i < segment.Points.Count - 1; i++)
                {
                    var p1 = segment.Points[i];
                    var p2 = segment.Points[i + 1];
                    drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
                }

                drawer.DrawNode(x, y);
#endif
                return (-1, -1);
            }
        }
    }

    // TODO: needs to be counter clockwise
    public void TraceEdgeStubs()
    {
        var nodeList = NodeCache.ToList();
        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            if (node.Value.CachedSegment2.Node == null)
            {
                var direction = node.Value.CachedSegment2.Dir;
                var (newNode, dir, found) =
                    TraceEdgeStartNode(node.Key.Item1, node.Key.Item2, node.Value, direction);
                var x = newNode.XPos;
                var y = newNode.YPos;
                if (!found)
                {
                    NodeCache[(x, y)] = newNode;
                    continue;
                }

                continue;
                // Ignore first
                Console.WriteLine("Found node:");

                Polygon polygon;

                //newNode.Visit(ref dir, out var segment, out var nextNode);
                
                // TODO fix errors
                // In a case where a overaching node has a path which is not completable on the inside we do not want to trace it
                // Therefore we can either check if it is possible or simply check if all containing nodes can be parsed
                
                
                
                // There are two possibilities: Either on border or not
                if (x == 0 || x == width || y == 0 || y == height)
                {
                    try
                    {
                        //polygon = VisitTillEnd(node.Value, node.Value.CachedSegment2.Dir.Invert());
                       // polygon = VisitTillEnd(newNode, dir);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        polygon = new Polygon(0);
                    }
                    
                }
                else
                    try
                    {
                        polygon = VisitTillEnd(node.Value, node.Value.CachedSegment2.Dir.Invert());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        polygon = new Polygon(0);
                    }
                

#if ENABLE_VISUAL_TRACING
                drawer.DrawPolygon(polygon);

#endif

                // Options: 1. Connect to newNode 2. Connect to known node 2.5 Connect to border
            }
        }
    }
    
    /// <summary>
    /// Starts at a node and walks along the border. If a cache is present in the direction it will be used. Otherwise a new edge will be traced.
    /// At every node the visited bool is updated to prevent infinite loops. Only the outgoing direction is marked as visited, since it is the important one.
    /// When a node has been visited three times, the node can be delted from the cache, since all edges have been traced.
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Polygon VisitNodes(Node startNode, Direction startDirection)
    {
        var polygon = new Polygon(0);
        var currentNode = startNode;
        var currentDirection = startDirection;
        
        // First need is to check if we have a cached segment in the direction
        // If not we need to trace a new edge
        // First visit is a special case since we know the direction to trace in
        var firstCache = startNode.GetSegment(startDirection);
        
        startNode.SetDirection(currentDirection);
        
        BorderSegmentDirectional currentSegment;
        if (firstCache.Segment is null)
        {
            // Call tracing function to next node
            (currentNode, currentSegment, var found) =
                TraceEdgeStartNode(startNode.XPos, startNode.YPos, startNode, startDirection);

            if (!found)
            {
                NodeCache.Add((currentNode.XPos, currentNode.YPos), currentNode);
            }

        }
        else
        {
            // We have already visited this node in the same direction, so we can continue using cache.
            polygon.Segments.Add(currentNode);
            polygon.Segments.Add(firstCache.Segment.Value);
            currentNode = firstCache.Node!;
            currentSegment = firstCache.Segment.Value;
        }
        
       
        
        // TODO check visited state
        while (true)
        {
            if(currentNode == startNode)
                break;
            // Now we are at a new node, so we can check if we have a cached segment in the current direction
            if (currentNode.VisitNew(ref currentDirection, currentNode!, currentSegment, out var newSegment,
                    out var nextNode))
            {
                currentNode.SetDirection(currentDirection);
                // Found a cached segment
                polygon.Segments.Add(currentNode);
                polygon.Segments.Add(newSegment!.Value);
                currentNode = nextNode!;
                currentSegment = newSegment.Value;
            }
            else
            {
                // TODO use the found bool to skip additional checks
                // Need to trace a new edge
                polygon.Segments.Add(currentNode);
                currentNode.SetDirection(currentDirection);
                (currentNode, currentSegment, var found) =
                    TraceEdgeStartNode(currentNode.XPos, currentNode.YPos, currentNode, currentDirection);
                
                if (!found)
                {
                    NodeCache.Add((currentNode.XPos, currentNode.YPos), currentNode);
                }
                
                polygon.Segments.Add(currentSegment);
            }
        }
        
        #if ENABLE_VISUAL_TRACING
        drawer.DrawPolygon(polygon);
        #endif

        return polygon;
    }


    public Polygon VisitTillEnd(Node startNode, Direction startDirection)
    {
        var polygon = new Polygon(0);
        var currentNode = startNode;
        var currentDirection = startDirection;

        bool Add(Node node, BorderSegmentDirectional? seg)
        {
            if (seg.HasValue)
            {
                // We have already visited this node in the same direction, so we can continue using cache.
                polygon.Segments.Add(currentNode);
                polygon.Segments.Add(seg.Value);
                if (node == startNode)
                {
                    return true;
                }

                currentNode = node;
            }
            else
            {
                throw new InvalidOperationException("Encountered an unvisited node while tracing a polygon.");
            }

            return false;
        }

        currentNode.Visit(ref currentDirection, out var segment, out var next);
        if (Add(next!, segment))
            return polygon;

        while (true)
        {
            currentNode.Visit(ref currentDirection, currentNode!, segment!.Value, out segment, out next);
            if (Add(next!, segment))
                return polygon;
        }
    }

    public void VisitNode(Node node)
    {
        Direction[] dirs = [node.CachedSegment1.Dir, node.CachedSegment2.Dir, node.CachedSegment3.Dir];

        foreach (var direction in dirs)
        {
            //if(node.TestDirection(direction))
             //   continue;
            VisitNodes(node, direction);
        }
    }

    public void ParseEverything()
    {
        while(NodeCache.Count > 0)
        {
            var node = NodeCache.First();
            VisitNode(node.Value);
            NodeCache.Remove(node.Key);
        }
    }

    public void LoadLocations(string filePath, IDebugDrawer mw)
    {
        var bmp = new Bitmap(filePath);
        var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
        width = bmp.Width;
        height = bmp.Height;
        stride = bmpData.Stride;
        scan0 = bmpData.Scan0;
        drawer = mw;
        var sw = new Stopwatch();
        sw.Start();
        TraceImageEdge();
        sw.Stop();
        Console.WriteLine($"Traced image in {sw.ElapsedMilliseconds} ms.");
        sw.Restart();
        TraceEdgeStubs();
        sw.Stop();
        ParseEverything();
        Console.WriteLine($"Traced edge stubs in {sw.ElapsedMilliseconds} ms.");
    }
}