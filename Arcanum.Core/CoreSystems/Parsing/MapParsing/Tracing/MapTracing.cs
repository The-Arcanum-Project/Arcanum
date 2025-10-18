using System.Drawing.Imaging;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

public sealed unsafe class MapTracing : IDisposable
{
   private const int ALPHA = 255 << 24;
   private const int OUTSIDE_COLOR = 0x000000;
   public int Width;
   public int Height;
   private int _stride;
   private IntPtr _scan0;
   private Bitmap _bitmap;
   private BitmapData _bitmapData;

   private Dictionary<Vector2I, Node> NodeCache { get; } = new();

   public MapTracing(Bitmap bmp)
   {
      _bitmap = bmp;
      _bitmapData = _bitmap.LockBits(new(0, 0, bmp.Width, bmp.Height),
                                     ImageLockMode.ReadOnly,
                                     PixelFormat.Format24bppRgb);
      Width = _bitmapData.Width;
      Height = _bitmapData.Height;
      _stride = _bitmapData.Stride;
      _scan0 = _bitmapData.Scan0;
   }

   private int GetColor(int x, int y)
   {
      var row = (byte*)_scan0 + y * _stride;
      var xTimesThree = x * 3;
      return ALPHA |
             (row[xTimesThree + 2] << 16) |
             (row[xTimesThree + 1] << 8) |
             row[xTimesThree];
   }

   private int GetColor(Vector2I pos)
   {
      return GetColor(pos.X, pos.Y);
   }

   private int GetColorWithOutsideCheck(int x, int y)
   {
      if (x < 0 || x >= Width || y < 0 || y >= Height)
      {
         return OUTSIDE_COLOR;
      }

      return GetColor(x, y);
   }

   private int GetColorWithOutsideCheck(Vector2I pos)
   {
      return GetColorWithOutsideCheck(pos.X, pos.Y);
   }

   private static void LinkNodes(Node a, Node b, BorderSegment segment)
   {
      a.Segments[2].Node = b;
      a.Segments[2].Segment = new(segment, true);
      b.Segments[0].Node = a;
      b.Segments[0].Segment = new(segment, false);
   }

   /// <summary>
   /// Finds nodes along the edges of the image and creates border segments between them.
   /// </summary>
   /// <exception cref="InvalidOperationException">Thrown if no nodes are found on the border of the image.</exception>
   private void TraceImageEdges()
   {
      var lastColor = GetColor(0, 0);

      Node? firstNode = null;
      var firstSegment = new BorderSegment();
      firstSegment.Points.Add(new(0, 0));

      Node? lastNode = null;

      var currentSegment = firstSegment;

      // Left Edge (top to bottom)
      for (var y = 1; y <= Height - 1; y++)
      {
         var color = GetColor(0, y);
         if (color != lastColor)
         {
            FinalizeSegment(0, y, color, Direction.East);
         }
      }

      // Bottom Left Corner
      currentSegment.Points.Add(new(0, Height));

      // Bottom Edge (left to right)
      for (var x = 1; x <= Width - 1; x++)
      {
         var color = GetColor(x, Height - 1);
         if (color != lastColor)
         {
            FinalizeSegment(x, Height, color, Direction.North);
         }
      }

      // Bottom Right Corner
      currentSegment.Points.Add(new(Width, Height));

      // Right Edge (bottom to top)
      for (var y = Height - 1; y > 0; y--)
      {
         var color = GetColor(Width - 1, y - 1);
         if (color != lastColor)
         {
            FinalizeSegment(Width, y, color, Direction.West);
         }
      }

      // Top Right Corner
      currentSegment.Points.Add(new(Width, 0));

      // Top Edge (right to left)
      for (var x = Width - 1; x > 0; x--)
      {
         var color = GetColor(x - 1, 0);
         if (color != lastColor)
         {
            FinalizeSegment(x, 0, color, Direction.South);
         }
      }

      currentSegment.Points.AddRange(firstSegment.Points);

      //TODO: @MelCo: This currently does not handle the case where there are no nodes on the border.
      if (lastNode == null || firstNode == null)
      {
         throw new
            InvalidOperationException("No nodes were on the border of the image. We currently do not support maps without nodes on the border.");
      }

      LinkNodes(lastNode, firstNode, currentSegment);

      return;

      void FinalizeSegment(int xPos, int yPos, int color, Direction direction)
      {
         // Color changed, finalize the current segment and start a new one

         var newSegment = new BorderSegment();

         Node node = new(xPos, yPos, direction, true);

         // First node found

         if (lastNode is null)
            firstNode = node;
         else
            LinkNodes(lastNode, node, currentSegment);

         // Update caches and references

         NodeCache.Add(new(xPos, yPos), node);
         lastNode = node;
         currentSegment = newSegment;
         lastColor = color;
      }
   }

   /// <summary>
   /// Traces from a given start node to the next node in the given direction.
   /// </summary>
   /// <param name="startNode"></param>
   /// <param name="startDirection"></param>
   private Node TraceEdgeStartNodeWithOutsideCheck(Node startNode,
                                                   Direction startDirection)
   {
      // Get pixel positions to the left and right of the start node in the given direction
      var points = DirectionHelper.GetStartPos(startNode.XPos, startNode.YPos, startDirection);
      var lColor = GetColorWithOutsideCheck(points.Xl, points.Yl);
      var rColor = GetColorWithOutsideCheck(points.Xr, points.Yr);

      var currentDirection = startDirection;
      var currentSegment = new BorderSegment();

      while (true)
      {
         currentDirection.Move(ref points, out var cachePos, out var xaxis);
         var lTest = GetColorWithOutsideCheck(points.Xl, points.Yl);
         var rTest = GetColorWithOutsideCheck(points.Xr, points.Yr);
         if (lTest == lColor && rTest == rColor)
            continue;

         // Right turn
         if (lTest == lColor && rTest == lColor)
         {
            points.Xl = points.Xr;
            points.Yl = points.Yr;
            if (xaxis)
               points.Xr = cachePos;
            else
               points.Yr = cachePos;
            currentDirection = currentDirection.RotateRight();
            lColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         } // Left turn

         if (lTest == rColor && rTest == rColor)
         {
            points.Xr = points.Xl;
            points.Yr = points.Yl;
            if (xaxis)
               points.Xl = cachePos;
            else
               points.Yl = cachePos;
            currentDirection = currentDirection.RotateLeft();
            rColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         // Node found
         var arriveDirection = currentDirection.Invert();

         if (!NodeCache.TryGetValue(points.GetPosition(), out var node))
         {
            var dir = arriveDirection;
            var isThreeWayNode = rTest == lTest;

            if (lTest == lColor)
            {
               dir = currentDirection.RotateRight();
               isThreeWayNode = true;
            }
            else if (rTest == rColor)
            {
               dir = currentDirection.RotateLeft();
               isThreeWayNode = true;
            }

            node = isThreeWayNode
                      ? Node.GetThreeWayNode(points.Xpos, points.Ypos, dir)
                      : Node.GetFourWayNode(points.Xpos, points.Ypos, currentDirection);

            var segment = node.GetSegment(arriveDirection);
            segment.Node = startNode;
            segment.Segment = new(currentSegment, false);

            NodeCache.Add(points.GetPosition(), node);
         }
         else
         {
            var cache = node.GetSegment(arriveDirection);
            cache.Node = startNode;
            cache.Segment = new(currentSegment, false);
         }

         var startCache = startNode.GetSegment(startDirection);
         startCache.Node = node;
         startCache.Segment = new(currentSegment, true);
         return node;
      }
   }

   /// <summary>
   /// Traces from every edge node into the image
   /// </summary>
   private void TraceEdgeStubs()
   {
      var nodes = NodeCache.Values.ToList();
      foreach (var node in nodes)
      {
         // No node present, so try to find it
         if (node.Segments[1].Node != null)
            continue;

         var dir = node.Segments[1].Dir;
         TraceEdgeStartNodeWithOutsideCheck(node, dir);
      }
   }

   private PolygonParsing TraceFromNode(Node startNode, Direction startDirection)
   {
      var currentNode = startNode;
      var currentDirection = startDirection;

      var firstCache = startNode.GetSegment(currentDirection);

      var rightPixel = DirectionHelper.GetRightPixel(startNode.XPos, startNode.YPos, currentDirection);

      var polygon = new PolygonParsing(GetColor(rightPixel.Item1, rightPixel.Item2));
      firstCache.Visited = true;

      BorderSegmentDirectional currentSegment;

      polygon.Segments.Add(currentNode);
      if (firstCache.Segment == null)
      {
         var newNode = TraceEdgeStartNodeWithOutsideCheck(currentNode, currentDirection);
         currentSegment = currentNode.GetSegment(currentDirection).Segment!.Value;
         currentNode = newNode;
      }
      else
      {
         currentNode = firstCache.Node;
         currentSegment = firstCache.Segment.Value;
      }

      polygon.Segments.Add(currentSegment);
      while (true)
      {
         // We have arrived at the start node, so we are done
         if (currentNode == startNode)
            break;

         // Now we are at a new node, so we can check if we have a cached segment in the current direction
         if (currentNode!.Visit(ref currentDirection,
                                currentSegment,
                                out var newSegment,
                                out var nextNode))
         {
            // TODO: @MelCo: Move to Visit method
            newSegment.Visited = true;
            polygon.Segments.Add(currentNode);
            polygon.Segments.Add(newSegment.Segment!.Value);
            currentNode = nextNode;
            currentSegment = newSegment.Segment.Value;
         }
         else
         {
            polygon.Segments.Add(currentNode);
            currentNode.SetDirection(currentDirection);
            var newNode = TraceEdgeStartNodeWithOutsideCheck(currentNode, currentDirection);
            currentSegment = currentNode.GetSegment(currentDirection).Segment!.Value;
            currentNode = newNode;
            polygon.Segments.Add(currentSegment);
         }
      }

      return polygon;
   }

   /// <summary>
   /// Goes through all directions of the given node and starts tracing if the direction has not been visited yet.
   /// Adds all found polygons to the given list.
   /// </summary>
   /// <param name="node"></param>
   /// <param name="polygons"></param>
   private void VisitNode(Node node, List<PolygonParsing> polygons)
   {
      var dirs = node.Segments.Select(s => s.Dir);

      foreach (var direction in dirs)
      {
         if (node.TestDirection(direction))
            continue;

         var polygon = TraceFromNode(node, direction);
         polygons.Add(polygon);
      }
   }

   /*=>
   polygons.AddRange(from direction in node.Segments.Select(s => s.Dir)
       where !node.TestDirection(direction)
       select TraceFromNode(node, direction));
*/
   public List<PolygonParsing> Trace()
   {
      TraceImageEdges();
      TraceEdgeStubs();

      List<PolygonParsing> polygons = [];
      while (NodeCache.Count > 0)
      {
         var node = NodeCache.First();
         VisitNode(node.Value, polygons);
         NodeCache.Remove(node.Key);
      }

      return polygons;
   }

   #region Disposable

   public void Dispose()
   {
      Dispose(true);
      GC.SuppressFinalize(this);
   }

   private bool _disposed;

   private void Dispose(bool disposing)
   {
      if (_disposed)
         return;

      if (disposing)
      {
         _bitmap.UnlockBits(_bitmapData);
         _bitmapData = null!;
         _bitmap = null!;
      }

      // If you had unmanaged memory that YOU allocated manually,
      // you'd release it here, regardless of the value of 'disposing'.

      _disposed = true;
   }

   #endregion
}