using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

public class CacheNodeInfo(Node? node, BorderSegmentDirectional? segment, Direction dir, bool visited = false)
{
    public Direction Dir = dir;
    public bool Visited = visited;
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
        return $"Node {_nodeId} at ({XPos}, {YPos})";
    }

#endif

    public void AddTo(List<Point> points)
    {
        points.Add(new Point(XPos, YPos));
    }

    public readonly Point Position;
    public int XPos => Position.X;
    public int YPos => Position.Y;

    public CacheNodeInfo[] Segments;
    

    public void SetDirection(Direction dir)
    {
        GetSegment(dir).Visited = true;
    }

    public bool TestDirection(Direction dir)
    {
        return GetSegment(dir).Visited;
    }

    /// <summary>
    /// Represents a node on the border of the image.
    /// Caches the segment used to approach the node and the segment parsed from it.
    /// </summary>
    public Node(CacheNodeInfo[] Segments, int xPos,
        int yPos)
    {
#if DEBUG
        _nodeId = _totalNodes;
        _totalNodes++;
#endif

        this.Segments = Segments;
        Position = new(xPos, yPos);
    }

    public CacheNodeInfo GetSegment(Direction dir)
    {
        foreach (var segment in Segments)
        {
            if (segment.Dir == dir)
                return segment;
        }
        throw new InvalidOperationException($"Node does not have a segment in direction {dir}");
    }

    public bool TryGetSegment(Direction dir, [MaybeNullWhen(false)]out CacheNodeInfo segment)
    {
        foreach (var cache in Segments)
        {
            if (cache.Dir != dir) continue;
            segment = cache;
            return true;
        }
        segment = null;
        return false;
    }

    /// <summary>
    /// Checks given the direction if any segment is cached and returns it with the node it leads to.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="segment"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool Visit(ref Direction direction, out BorderSegmentDirectional segment,
        [MaybeNullWhen(false)] out Node node)
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
            segment = cache.Segment.Value;
            node = cache.Node!;
            return true;
        }

        segment = default;
        node = null;
        return false;
    }

    public bool Visit(ref Direction direction, Node inputNode, BorderSegmentDirectional input,
        out BorderSegmentDirectional segment, [MaybeNullWhen(false)] out Node node)
    {
        Point point;
        if (input.Segment.Points.Count == 0)
        {
            return Visit(ref direction, out segment, out node);
        }

        if (input.IsForward)
            point = input.Segment.Points[^1];
        else
            point = input.Segment.Points[0];

        return Visit(ref direction, (inputNode.XPos, inputNode.YPos), point.X, point.Y, out segment, out node);
    }

    public bool Visit(ref Direction direction, (int, int) nodepos, int x, int y, out BorderSegmentDirectional segment,
        [MaybeNullWhen(false)] out Node node)
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
        return Visit(ref direction, out segment, out node);
    }
}