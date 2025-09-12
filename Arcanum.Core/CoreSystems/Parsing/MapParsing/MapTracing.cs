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

public readonly record struct Point(int X, int Y)
{
   public static Point Empty => new(int.MinValue, int.MinValue);

   public override string ToString()
   {
      return $"({X}, {Y})";
   }

   public static Point operator +(Point p1, Point p2)
   {
      return new(p1.X + p2.X, p1.Y + p2.Y);
   }

   public static Point operator -(Point p1, Point p2)
   {
      return new(p1.X - p2.X, p1.Y - p2.Y);
   }

   public static Point operator *(Point p, int scalar)
   {
      return new(p.X * scalar, p.Y * scalar);
   }

   public static Point operator /(Point p, int scalar)
   {
      if (scalar == 0)
         throw new DivideByZeroException("Cannot divide by zero.");

      return new(p.X / scalar, p.Y / scalar);
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

public readonly struct BorderSegmentDirectional(BorderSegment segment, bool isForward)
{
   public readonly BorderSegment Segment = segment;
   public readonly bool IsForward = isForward;

   public void AddToList(List<Point> points)
   {
      if (IsForward)
         points.AddRange(Segment.Points);
      else
         for (var i = Segment.Points.Count - 1; i >= 0; i--)
            points.Add(Segment.Points[i]);
   }

   public BorderSegmentDirectional Invert()
   {
      return new(Segment, !IsForward);
   }
}

public class Polygon(int color)
{
   public int Color { get; } = color;
   public List<BorderSegmentDirectional> Segments { get; } = [];
   public List<Polygon> Holes { get; } = [];

   public List<Point> GetAllPoints()
   {
      var points = new List<Point>();
      foreach (var segment in Segments)
         segment.AddToList(points);

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
public class Node
{
#if DEBUG
   private static int _totalNodes;
   private int _nodeId;

   public override string ToString()
   {
      return $"Node {_nodeId}";
   }

#endif

   public CacheNodeInfo CachedSegment1;
   public CacheNodeInfo CachedSegment2;
   public CacheNodeInfo CachedSegment3;

   /// <summary>
   /// Represents a node on the border of the image.
   /// Caches the segment used to approach the node and the segment parsed from it.
   /// </summary>
   public Node(CacheNodeInfo cachedSegment1, CacheNodeInfo cachedSegment2, CacheNodeInfo cachedSegment3)
   {
#if DEBUG
      _nodeId = _totalNodes;
      _totalNodes++;
#endif
      CachedSegment1 = cachedSegment1;
      CachedSegment2 = cachedSegment2;
      CachedSegment3 = cachedSegment3;
   }

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
            throw new
               InvalidOperationException($"Node was not visited in the direction {direction}. Cached segments: {CachedSegment1.Dir}, {CachedSegment2.Dir}, {CachedSegment3.Dir}");
         }
      }
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
         return OUTSIDE_COLOR;

      return GetColor(x, y);
   }

   /// <summary>
   /// Traces the edges of the image in a counter-clockwise manner.
   /// Finds all the nodes on the border of the image.
   /// </summary>
   /// <returns></returns>
   public List<((int, int), Node)> TraceImageEdge()
   {
      var nodes = new List<((int, int), Node)>();

      var lastColor = GetColor(0, 0);
      var lastRow = height - 1;
      var lastColumn = width - 1;

      // The first segment starts at the origin.
      var firstSegment = new BorderSegment();
      Node firstNode = null;

      Node lastNode = null;
      BorderSegment lastSegment = null!;

      // Top Left Corner
      firstSegment.Points.Add(new(0, 0));
      var currentSegment = firstSegment;

      // Local function to process a completed segment when a new node is found.
      void FinalizeSegmentAndStartNew(int x, int y, int color, Direction d)
      {
         // Add the new node's location to complete the current segment's path.
         currentSegment.Points.Add(new(x, y));

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
         newSegment.Points.Add(new(x, y));
         Node node;
         if (lastNode != null)
         {
            node = new(new(lastNode, new BorderSegmentDirectional(currentSegment, false), d.RotateLeft()),
                       new(null, null, d),
                       new(null, null, d.RotateLeft()));
            lastNode.CachedSegment2.Node = node;
            lastNode.CachedSegment2.Segment = new(currentSegment, true);
         }
         else
         {
            node = new(new(null, null, d.RotateLeft()),
                       new(null, null, d),
                       new(null, null, d.RotateLeft()));
            firstNode = node;
         }

         nodes.Add(((x, y), node));
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
            FinalizeSegmentAndStartNew(0, y, color, Direction.East);
      }

      // Bottom Left Corner
      currentSegment.Points.Add(new(0, height));

      // Bottom Edge (left to right)
      for (var x = 1; x <= lastColumn; x++)
      {
         var color = GetColor(x, lastRow);
         if (color != lastColor)
            FinalizeSegmentAndStartNew(x, height, color, Direction.North);
      }

      // Bottom Right Corner
      currentSegment.Points.Add(new(width, height));

      // Right Edge (bottom to top)
      for (var y = lastRow; y > 0; y--)
      {
         var color = GetColor(lastColumn, y - 1);
         if (color != lastColor)
            FinalizeSegmentAndStartNew(width, y, color, Direction.West);
      }

      // Top Right Corner
      currentSegment.Points.Add(new(width, 0));

      // Top Edge (right to left)
      for (var x = lastColumn; x > 0; x--)
      {
         var color = GetColor(x - 1, 0);
         if (color != lastColor)
            FinalizeSegmentAndStartNew(x, 0, color, Direction.South);
      }

      currentSegment.Points.AddRange(firstSegment.Points);
      firstSegment = currentSegment;

      if (lastNode == null)
         throw new
            InvalidOperationException("No nodes were on the border of the image. We currently do not support maps without nodes on the border.");

      lastNode.CachedSegment2.Node = firstNode;
      lastNode.CachedSegment2.Segment = new(currentSegment, true);
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

      // Check if each node has the following node in seg1 and the previous node in seg2.

      return nodes;
   }

   private bool MoveDirWithCheck(bool dir,
                                 bool sign,
                                 ref int xl,
                                 ref int yl,
                                 ref int xr,
                                 ref int yr,
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

   private void MoveDir(bool dir,
                        bool sign,
                        ref int xl,
                        ref int yl,
                        ref int xr,
                        ref int yr,
                        out int cache)
   {
      if (dir)
      {
         if (sign)
         {
            // Move right
            cache = xl++;
            xr = xl;
         }

         // Move left
         cache = xl--;
         xr = xl;
      }

      if (sign)
      {
         // Move down
         cache = yl++;
         yr = yl;
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
            // Move right
            x++;
         else
            // Move left
            x--;
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

      var polygon = new Polygon(rColor);

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
               // Horizontal movement
               xr = cache;
            else
               // Vertical movement
               yr = cache;

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
               // Horizontal movement
               xl = cache;
            else
               // Vertical movement
               yl = cache;

            currentDirection = currentDirection.RotateLeft();
            (dir, sign) = currentDirection.GetDeltaMove();
            segment.Points.Add(new(x, y));
            rColor = lTest;
         }
         else if (lTest == lColor || rTest == rColor)
         {
            var dir1 = currentDirection.Invert();
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
               node = new(cache1, cache2, cache3);
            }
            else
            {
               node.CachedSegment1.Segment = new BorderSegmentDirectional(segment, false);
               //node.CachedSegment1.Node = lastNode; Not the case since we start without a node
            }

            BorderSegmentDirectional? newSegment;
            var newNode = node;
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
   }

   public (int x, int y) TraceSingleEdge(int x, int y, Direction d)
   {
      var segment = new BorderSegment();

      segment.Points.Add(new(x, y));
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
            segment.Points.Add(new(x, y));
            // We have reached the edge of the image
            if (!NodeCache.TryGetValue((x, y), out var node))
               // We have a node at this position, so we can add the segment to it.
               //node = new Node(new BorderSegmentDirectional(segment, true), currentDirection);
               NodeCache[(x, y)] = node;

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
               // Horizontal movement
               xr = cache;
            else
               // Vertical movement
               yr = cache;

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
               // Horizontal movement
               xl = cache;
            else
               // Vertical movement
               yl = cache;

            currentDirection = currentDirection.RotateLeft();
            (dir, sign) = currentDirection.GetDeltaMove();
            segment.Points.Add(new(x, y));
            rColor = lTest;
         }
         else
         {
            segment.Points.Add(new(x, y));
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
   public void TraceEdgeStubs(List<((int, int), Node)> nodes)
   {
      for (var index = nodes.Count - 1; index >= 0; index--)
      {
         var (pos, node) = nodes[index];
         //var (x, y) = TraceSingleEdge(pos.Item1, pos.Item2, node.Direction);
         //if (x == -1) continue;
         //var i = nodes.FindIndex(p => p.Item1.Item1 == x && p.Item1.Item2 == y);
         nodes.RemoveAt(index);
         index--;
      }
   }

   public void LoadLocations(string filePath, IDebugDrawer mw)
   {
      var bmp = new Bitmap(filePath);
      var bmpData = bmp.LockBits(new(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.ReadOnly,
                                 PixelFormat.Format24bppRgb);
      width = bmp.Width;
      height = bmp.Height;
      stride = bmpData.Stride;
      scan0 = bmpData.Scan0;
      drawer = mw;
      var sw = new Stopwatch();
      sw.Start();
      var nodes = TraceImageEdge();
      sw.Stop();
      Console.WriteLine($"Traced image in {sw.ElapsedMilliseconds} ms.");
      sw.Restart();
      //TraceEdgeStubs(nodes);
      sw.Stop();
      Console.WriteLine($"Traced edge stubs in {sw.ElapsedMilliseconds} ms.");
   }
}