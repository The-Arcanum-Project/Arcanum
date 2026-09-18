using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

public sealed unsafe class MapTracing : IDisposable
{
   private const int ALPHA = 255 << 24;
   public const int OUTSIDE_COLOR = 0x000000;
   private readonly int _width;
   private readonly int _height;
   private readonly int _stride;
   private readonly IntPtr _scan0;
   private readonly Bitmap _bitmap;
   private readonly BitmapData _bitmapData;
   private readonly Bitmap _visitedBitmap;
   private readonly BitmapData _visitedBitmapData;
   private readonly IntPtr _visitedScan0;
   private readonly int _visitedStride;

   private readonly UncheckedBitmapHandler _handler = default;

   private readonly Queue<Node> _nodeQueue = new();

   private Dictionary<Vector2I, Node> NodeCache { get; } = new();

   public MapTracing(Bitmap bmp)
   {
      _bitmap = bmp;
      _bitmapData = _bitmap.LockBits(new(0, 0, bmp.Width, bmp.Height),
                                     ImageLockMode.ReadOnly,
                                     PixelFormat.Format24bppRgb);
      _width = _bitmapData.Width;
      _height = _bitmapData.Height;
      _stride = _bitmapData.Stride;
      _scan0 = _bitmapData.Scan0;
      _visitedBitmap = new(_bitmap.Width, _bitmap.Height, PixelFormat.Format1bppIndexed);
      _visitedBitmapData = _visitedBitmap.LockBits(new(0, 0, bmp.Width, bmp.Height),
                                                   ImageLockMode.ReadWrite,
                                                   PixelFormat.Format1bppIndexed);
      _visitedScan0 = _visitedBitmapData.Scan0;
      _visitedStride = _visitedBitmapData.Stride;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private int GetColor(int x, int y) => _handler.GetColor(_scan0, _stride, _width, _height, x, y);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static int GetColorRowPtr(byte* pixelPtr) => ALPHA |
                                                        pixelPtr[2] |
                                                        (pixelPtr[1] << 8) |
                                                        (pixelPtr[0] << 16);

   // ReSharper disable once UnusedMember.Local
   private bool IsPixelCleared(int i, int i1)
   {
      var row = (byte*)_visitedScan0 + i1 * _visitedStride;
      return (row[i / 8] & (byte)(0x80 >> (i % 8))) != 0;
   }

   private void MarkVisited(int x, int y)
   {
      _handler.MarkVisited(_visitedScan0, _visitedStride, _width, _height, x, y);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void LinkNodes(Node a, Direction aDir, Node b, Direction bDir, BorderSegment segment)
   {
      ref var aCache = ref a.GetSegmentRef(aDir);
      aCache.Node = b;
      aCache.Segment = new BorderSegmentDirectional(segment, true);

      ref var bCache = ref b.GetSegmentRef(bDir);
      bCache.Node = a;
      bCache.Segment = new BorderSegmentDirectional(segment, false);
   }

   /// <summary>
   /// Finds nodes along the edges of the image and creates border segments between them.
   /// </summary>
   /// <exception cref="InvalidOperationException">Thrown if no nodes are found on the border of the image.</exception>
   private void TraceImageEdges()
   {
      var lastColor = GetColor(0, 0);

      Node? firstNode = null;
      var firstNodeLeftDir = Direction.North;
      var lastNodeRightDir = Direction.North;
      var firstSegment = new BorderSegment();
      firstSegment.Points.Add(new(0, 0));

      Node? lastNode = null;

      var currentSegment = firstSegment;

      MarkVisited(0, 0);

      // Left Edge (top to bottom)
      for (var y = 1; y < _height; y++)
      {
         var color = GetColor(0, y);
         if (color != lastColor)
            FinalizeSegment(0, y, color, Direction.East);

         MarkVisited(0, y);
      }

      // Bottom Left Corner
      currentSegment.Points.Add(new(0, _height));

      // Bottom Edge (left to right)
      for (var x = 1; x < _width; x++)
      {
         var color = GetColor(x, _height - 1);
         if (color != lastColor)
            FinalizeSegment(x, _height, color, Direction.North);

         MarkVisited(x, _height - 1);
      }

      // Bottom Right Corner
      currentSegment.Points.Add(new(_width, _height));

      // Right edge (bottom to top)
      for (var y = _height - 2; y >= 0; y--)
      {
         var color = GetColor(_width - 1, y);
         if (color != lastColor)
            FinalizeSegment(_width, y + 1, color, Direction.West);
         MarkVisited(_width - 1, y);
      }

      // Top Right Corner
      currentSegment.Points.Add(new(_width, 0));

      // Top Edge (right to left)
      for (var x = _width - 2; x >= 0; x--)
      {
         var color = GetColor(x, 0);
         if (color != lastColor)
            FinalizeSegment(x + 1, 0, color, Direction.South);
         MarkVisited(x, 0);
      }

      // Close the loop
      currentSegment.Points.AddRange(firstSegment.Points);

      if (lastNode == null || firstNode == null)
         throw new
            InvalidOperationException("No nodes were on the border of the image. We currently do not support maps without nodes on the border.");

      LinkNodes(lastNode, lastNodeRightDir, firstNode, firstNodeLeftDir, currentSegment);

      return;

      void FinalizeSegment(int xPos, int yPos, int color, Direction direction)
      {
         // Color changed, finalize the current segment and start a new one

         var newSegment = new BorderSegment();
         var node = Node.CreateBorderNode(xPos, yPos, direction);

         var nodeLeftDir = direction.RotateLeft();
         var nodeRightDir = direction.RotateRight();

         // First node found
         if (lastNode is null)
         {
            firstNode = node;
            firstNodeLeftDir = nodeLeftDir;
         }
         else
            LinkNodes(lastNode, lastNodeRightDir, node, nodeLeftDir, currentSegment);

         NodeCache.Add(new(xPos, yPos), node);
         _nodeQueue.Enqueue(node);

         lastNode = node;
         lastNodeRightDir = nodeRightDir;
         currentSegment = newSegment;
         lastColor = color;
      }
   }

   /// <summary>
   /// Traces from a given start node to the next node in the given direction.
   /// </summary>
   /// <param name="startNode"></param>
   /// <param name="startDirection"></param>
   private Node TraceEdgeFromNode<THandler>(Node startNode, Direction startDirection)
      where THandler : struct, IBitmapHandler
   {
      TraceEdge<THandler>(startNode.Position, startDirection, out var node, out var arriveDir);

      ref var endCache = ref node.GetSegmentRef(arriveDir);
      endCache.Node = startNode;

      ref var startCache = ref startNode.GetSegmentRef(startDirection);
      startCache.Node = node;
      startCache.Segment = endCache.Segment?.Invert();

      return node;
   }

   private void TraceEdge<THandler>(Vector2I startPos, Direction startDirection, out Node resultNode, out Direction arriveDir, bool loopCheck = false)
      where THandler : struct, IBitmapHandler
   {
      THandler handler = default;
      var points = DirectionHelper.GetStartPos(startPos.X, startPos.Y, startDirection);
      // Cache these to avoid repeated property access

      var lColor = handler.GetColor(_scan0, _stride, _width, _height, points.Xl, points.Yl);
      var rColor = handler.GetColor(_scan0, _stride, _width, _height, points.Xr, points.Yr);

      var currentDirection = startDirection;
      var currentSegment = new BorderSegment(); // Still allocated (necessary)

      var startPointX = points.Xpos;
      var startPointY = points.Ypos;

      while (true)
      {
         handler.MarkVisited(_visitedScan0, _visitedStride, _width, _height, points.Xl, points.Yl);
         handler.MarkVisited(_visitedScan0, _visitedStride, _width, _height, points.Xr, points.Yr);

         currentDirection.Move(ref points, out var cachePos, out var xaxis);

         var lTest = handler.GetColor(_scan0, _stride, _width, _height, points.Xl, points.Yl);
         var rTest = handler.GetColor(_scan0, _stride, _width, _height, points.Xr, points.Yr);
         var arriveDirection = currentDirection.Invert();

         if (loopCheck && points.Xpos == startPointX && points.Ypos == startPointY)
         {
            // Need to check first if it is a complex node or just a simple node
            // E.g. in an edge case this might be a three or even 4 way node!

            Node loopNode;
            // L line inverted L
            if ((lTest == lColor && rTest == lColor) ||
                (lTest == lColor && rTest == rColor) ||
                (lTest == rColor && rTest == rColor))
            {
               currentSegment.Points.Add(points.GetPosition());
               loopNode = Node.CreateOneWayNode(points.Xpos, points.Ypos, startDirection);
               arriveDir = startDirection;
            }
            else
            {
               loopNode = CreateNode(arriveDirection,
                                     rTest,
                                     lTest,
                                     lColor,
                                     currentDirection,
                                     rColor,
                                     points,
                                     currentSegment,
                                     true);
               arriveDir = currentDirection;
            }

            MarkVisited(points.Xl, points.Yl);
            MarkVisited(points.Xr, points.Yr);

            ref var loopCache = ref loopNode.GetSegmentRef(startDirection);
            loopCache.Segment = new BorderSegmentDirectional(currentSegment, true);
            loopCache.Node = loopNode;

            resultNode = loopNode;
            return;
         }

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

            // Inlined RotateRight: (d + 1) & 3
            currentDirection = (Direction)(((int)currentDirection + 1) & 3);
            lColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         // Left turn
         if (lTest == rColor && rTest == rColor)
         {
            points.Xr = points.Xl;
            points.Yr = points.Yl;
            if (xaxis)
               points.Xl = cachePos;
            else
               points.Yl = cachePos;

            // Inlined RotateLeft: (d - 1) & 3
            currentDirection = (Direction)(((int)currentDirection - 1) & 3);
            rColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         // Node found
         if (!NodeCache.TryGetValue(points.GetPosition(), out var node))
         {
            node = CreateNode(arriveDirection, rTest, lTest, lColor, currentDirection, rColor, points, currentSegment);
            NodeCache.Add(points.GetPosition(), node);
            _nodeQueue.Enqueue(node); // Keep our queue logic
         }
         else
         {
            ref var cache = ref node.GetSegmentRef(arriveDirection);
            cache.Segment = new BorderSegmentDirectional(currentSegment, false);
         }

         handler.MarkVisited(_visitedScan0, _visitedStride, _width, _height, points.Xl, points.Yl);
         handler.MarkVisited(_visitedScan0, _visitedStride, _width, _height, points.Xr, points.Yr);

         resultNode = node;
         arriveDir = arriveDirection;
         return;
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static Node CreateNode(Direction arriveDirection,
                                  int rTest,
                                  int lTest,
                                  int lColor,
                                  Direction currentDirection,
                                  int rColor,
                                  DirectionHelper.PointSet points,
                                  BorderSegment currentSegment,
                                  bool setNode = false)
   {
      var dir = arriveDirection;
      var isThreeWayNode = rTest == lTest;
      if (lTest == lColor)
      {
         dir = (Direction)(((int)currentDirection + 1) & 3);
         isThreeWayNode = true;
      }
      else if (rTest == rColor)
      {
         dir = (Direction)(((int)currentDirection - 1) & 3);
         isThreeWayNode = true;
      }

      var loopNode = isThreeWayNode
                        ? Node.CreateThreeWayNode(points.Xpos, points.Ypos, dir)
                        : Node.CreateFourWayNode(points.Xpos, points.Ypos);

      ref var startCache = ref loopNode.GetSegmentRef(arriveDirection);
      startCache.Segment = new BorderSegmentDirectional(currentSegment, false);
      if (setNode)
         startCache.Node = loopNode;
      return loopNode;
   }

   /// <summary>
   /// Traces from every edge node into the image
   /// </summary>
   private void TraceEdgeStubs()
   {
      var nodes = NodeCache.Values.ToArray().AsSpan();
      for (var i = 0; i < nodes.Length; i++)
      {
         var node = nodes[i];

         for (var d = 0; d < 4; d++)
         {
            var dir = (Direction)d;
            if (!node.HasDirection(dir))
               continue;

            ref var cache = ref node.GetSegmentRef(dir);
            if (cache.Node != null)
               continue;

            // Stubs trace INTO the map - use unchecked getter
            TraceEdgeFromNode<CheckedBitmapHandler>(node, dir);
            break;
         }
      }
   }

   private PolygonParsing TraceFromNode(Node startNode, Direction startDirection)
   {
      var currentNode = startNode;
      var currentDirection = startDirection;

      ref var firstCache = ref startNode.GetSegmentRef(currentDirection);

      var rightPixel = DirectionHelper.GetRightPixel(startNode.XPos, startNode.YPos, currentDirection);

      var polygon = new PolygonParsing(GetColor(rightPixel.Item1, rightPixel.Item2));

      startNode.SetVisited(currentDirection);
      polygon.Segments.Add(currentNode);

      BorderSegmentDirectional currentSegment;

      if (!firstCache.Segment.HasValue)
      {
         // Need to trace - use unchecked since we're tracing interior
         currentNode = TraceEdgeFromNode<UncheckedBitmapHandler>(startNode, currentDirection);
         currentSegment = startNode.GetSegmentRef(currentDirection).Segment!.Value;
      }
      else
      {
         currentNode = firstCache.Node;
         currentSegment = firstCache.Segment.Value;
      }

      polygon.Segments.Add(currentSegment);

      while (true)
         if (currentNode!.Visit(ref currentDirection, currentSegment, out var newSegment, out var nextNode))
         {
            if (currentNode.IsVisited(currentDirection))
               break;

            currentNode.SetVisited(currentDirection);
            polygon.Segments.Add(currentNode);
            polygon.Segments.Add(newSegment);
            currentNode = nextNode;
            currentSegment = newSegment;
         }
         else
         {
            polygon.Segments.Add(currentNode);
            currentNode.SetVisited(currentDirection);
            var newNode = TraceEdgeFromNode<UncheckedBitmapHandler>(currentNode, currentDirection);
            currentSegment = currentNode.GetSegmentRef(currentDirection).Segment!.Value;
            polygon.Segments.Add(currentSegment);
            currentNode = newNode;
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
      for (var i = 0; i < 4; i++)
      {
         var direction = (Direction)i;
         if (!node.HasDirection(direction) || node.IsVisited(direction))
            continue;

         polygons.Add(TraceFromNode(node, direction));
      }
   }

   private void VisitNode(Node node, Dictionary<int, List<PolygonParsing>> polygons)
   {
      for (var i = 0; i < 4; i++)
      {
         var direction = (Direction)i;
         if (!node.HasDirection(direction) || node.IsVisited(direction))
            continue;

         var polygon = TraceFromNode(node, direction);
         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygons, polygon.Color, out var exists);

         if (!exists)
            listRef = [];

         listRef!.Add(polygon);
      }
   }

   private PolygonParsing TraceIsland(Node node,
                                      Dictionary<int, List<PolygonParsing>> polygons,
                                      Direction outsideDirection)
   {
      // Trace the island into every direction, if outside direction save it for return and do not add to dictionary
      var hole = TraceFromNode(node, outsideDirection);

      for (var i = 0; i < 4; i++)
      {
         var direction = (Direction)i;
         if (direction == outsideDirection || !node.HasDirection(direction) || node.IsVisited(direction))
            continue;

         var polygon = TraceFromNode(node, direction);

         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygons, polygon.Color, out var exists);
         if (!exists)
            listRef = [];
         listRef!.Add(polygon);
      }

      return hole;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="position">Position of the new color on the right of the previous color</param>
   /// <param name="polygons">List of polygons where new ones are added to</param>
   /// <param name = "parent" ></param>
   private void HandleIsland(Vector2I position,
                             Dictionary<int, List<PolygonParsing>> polygons,
                             PolygonParsing parent)
   {
      // Instead of parsing from a node, we parse from a segment by finding the start node and tracing from there.
      // The edge case that no node exists will have to be taken into account.
      // First, find the nodes at the start and end of the segment.

      // We have a border on the left of the position

      Debug.Assert(NodeCache.Count == 0);
      Debug.Assert(_nodeQueue.Count == 0);

      var startPosition = new Vector2I(position.X, position.Y + 1);

      TraceEdge<UncheckedBitmapHandler>(startPosition, Direction.North, out var startNode, out var startArriveDir, true);

      PolygonParsing hole;

      // Single-direction loop (simple island)
      if (startNode.ActiveDirectionCount == 1)
      {
         // Loop detected so directly creates a polygon
         var islandColor = GetColor(position.X, position.Y);
         var polygon = new PolygonParsing(islandColor);
         ref var loopCache = ref startNode.GetSegmentRef(startArriveDir);
         var invertedSeg = loopCache.Segment!.Value.Invert();
         polygon.Segments.Add(invertedSeg);

         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygons, islandColor, out var exists);
         if (!exists)
            listRef = [];
         listRef!.Add(polygon);
         
         hole = new(islandColor);
         hole.Segments.Add(loopCache.Segment!.Value);
         parent.Holes.Add(hole);

         _nodeQueue.Clear();
         NodeCache.Clear();
         return;
      }

      if (startNode.Position == startPosition)
         // Complex island with existing loopNode
         NodeCache.Add(startPosition, startNode);
      else
      {
         // Complex island with multiple nodes
         TraceEdge<UncheckedBitmapHandler>(position, Direction.South, out var endNode, out var endArriveDir);
         var segment = new BorderSegment();
         ref var startCache = ref startNode.GetSegmentRef(startArriveDir);
         startCache.Segment!.Value.AddTo(segment.Points);

         ref var endCache = ref endNode.GetSegmentRef(endArriveDir);
         endCache.Segment!.Value.Invert().AddTo(segment.Points);

         startCache.Segment = new BorderSegmentDirectional(segment, true);
         endCache.Segment = new BorderSegmentDirectional(segment, false);
         startCache.Node = endNode;
         endCache.Node = startNode;
      }

      hole = TraceIsland(startNode, polygons, startArriveDir);

      while (_nodeQueue.TryDequeue(out var node))
         VisitNode(node, polygons);

      NodeCache.Clear();
      parent.Holes.Add(hole);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static bool GetParent(Dictionary<int, List<PolygonParsing>> polygons, int parentColor, Vector2I borderPos, ref PolygonParsing? parent)
   {
      if (!polygons.TryGetValue(parentColor, out var parentPolygonCandidates))
         return false;

      // quick check the last polygon to see if it is still the parent
      if (parent != null && parent.Color == parentColor && parent.IsOnBorder(borderPos))
         return true;

      var count = parentPolygonCandidates.Count;
      for (var i = 0; i < count; i++)
      {
         var candidate = parentPolygonCandidates[i];
         if (!candidate.IsOnBorder(borderPos))
            continue;

         parent = candidate;
         break;
      }

      return parent != null;
   }

   public Dictionary<int, List<PolygonParsing>> Trace()
   {
#if DEBUG
      var sw = Stopwatch.StartNew();
#endif
      TraceImageEdges();
#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"TraceImageEdges in {sw.ElapsedMilliseconds} ms");
      sw.Restart();
#endif
      TraceEdgeStubs();
#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"TraceEdgeStubs in {sw.ElapsedMilliseconds} ms");
      sw.Restart();
#endif
      List<PolygonParsing> polygons = [];
      while (_nodeQueue.TryDequeue(out var node))
         VisitNode(node, polygons);
#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"VisitNodes in {sw.ElapsedMilliseconds} ms");
#endif

      NodeCache.Clear();

#if DEBUG
      sw.Restart();
#endif

      // go through the entire visited bitmap and find borders which have not been visited yet
      Dictionary<int, List<PolygonParsing>> polygonsDict = new();
      var polySpan = CollectionsMarshal.AsSpan(polygons);
      for (var i = 0; i < polySpan.Length; i++)
      {
         var poly = polySpan[i];
         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygonsDict, poly.Color, out var exists);
         if (!exists)
            listRef = [];
         listRef!.Add(poly);
      }

      var counter = 0;

      var lastSwitchX = 0;
      var lastSwitchY = 0;
      PolygonParsing? lastPolygon = null;
      fixed (byte* bitMaskPtr = IBitmapHandler.BitMasks)
         for (var y = 0; y < _height; y++)
         {
            var row = (byte*)_scan0 + y * _stride;
            var visitedRow = (byte*)_visitedScan0 + y * _visitedStride;

            var lastColor = OUTSIDE_COLOR;

            for (var x = 0; x < _width; x++)
            {
               var color = GetColorRowPtr(row + x * 3);

               var byteIndex = x >> 3;
               var bitIndex = x & 7;
               var mask = bitMaskPtr[bitIndex];

               if (color != lastColor)
               {
                  var isVisited = (visitedRow[byteIndex] & mask) != 0;

                  if (!isVisited)
                  {
                     if (GetParent(polygonsDict, lastColor, new(lastSwitchX, lastSwitchY), ref lastPolygon))
                        HandleIsland(new(x, y), polygonsDict, lastPolygon!);
                     counter++;
                  }

                  lastSwitchX = x;
                  lastSwitchY = y;
                  lastColor = color;
               }
               else
                  visitedRow[byteIndex] |= mask;
            }
         }

#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("MPT", LogLevel.DBG, $"Found and traced {counter} islands in {sw.ElapsedMilliseconds} ms.");
#endif

      return polygonsDict;
   }

   #region Disposable

   public void Dispose()
   {
      Dispose(true);
   }

   private bool _disposed;

   private void Dispose(bool disposing)
   {
      if (_disposed)
         return;

      if (disposing)
      {
         _bitmap.UnlockBits(_bitmapData);
         _visitedBitmap.UnlockBits(_visitedBitmapData);
         _visitedBitmap.Dispose();
      }

      _disposed = true;
   }

   #endregion
}