#define ENABLE_VISUAL_TRACING

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Threading;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing;

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
                //drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }

            //drawer.DrawNode(x, y);
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
                node.Visited3 = true;
                lastNode.CachedSegment3.Node = node;
                lastNode.CachedSegment3.Segment = new(currentSegment, true);
            }
            else
            {
                node = new(
                    new CacheNodeInfo(null, null, d.RotateLeft()),
                    new CacheNodeInfo(null, null, d),
                    new CacheNodeInfo(null, null, d.RotateRight()), x, y);
                node.Visited3 = true;
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
            //drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
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
                    //drawer.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
                }

                if (segment.Points.Count > 0)
                {
                    //drawer.DrawLine(startx, starty, segment.Points[0].X, segment.Points[0].Y);
                    //drawer.DrawLine(segment.Points[^1].X, segment.Points[^1].Y, x, y);
                }
                //else
                    //drawer.DrawLine(startx, starty, x, y);

                //drawer.DrawNode(x, y);
#endif
                break;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Encountered a four-edge node, which is not supported yet. ({x}, {y})");
                // FourEdgeNode
                parsedSegment = new BorderSegmentDirectional();
                break;
            }
        }

        return (node, parsedSegment, found);
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
                }
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
            if (currentNode == startNode)
                break;
            // Now we are at a new node, so we can check if we have a cached segment in the current direction
            if (currentNode.Visit(ref currentDirection, currentNode!, currentSegment, out var newSegment,
                    out var nextNode))
            {
                currentNode.SetDirection(currentDirection);
                // Found a cached segment
                polygon.Segments.Add(currentNode);
                polygon.Segments.Add(newSegment);
                currentNode = nextNode;
                currentSegment = newSegment;
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
        System.Windows.Application.Current.Dispatcher.BeginInvoke(
            new Action(() => drawer.DrawPolygon(polygon)),
            System.Windows.Threading.DispatcherPriority.Background
        );
#endif

        return polygon;
    }

    public void VisitNode(Node node)
    {
        Direction[] dirs = [node.CachedSegment1.Dir, node.CachedSegment2.Dir, node.CachedSegment3.Dir];

        foreach (var direction in dirs)
        {
            if (node.TestDirection(direction))
                continue;
            VisitNodes(node, direction);
        }
    }

    public void ParseEverything()
    {
        while (NodeCache.Count > 0)
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
        Console.WriteLine($"Traced edge stubs in {sw.ElapsedMilliseconds} ms.");
        sw.Restart();
        ParseEverything();
        sw.Stop();
        Console.WriteLine($"Traced all polygons in {sw.ElapsedMilliseconds} ms.");
    }
}