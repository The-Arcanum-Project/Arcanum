
using System.Diagnostics;
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
   private readonly nint _scan0;
   private readonly Bitmap _bitmap;
   private readonly BitmapData _bitmapData;
   private readonly Bitmap _visitedBitmap;
   private readonly BitmapData _visitedBitmapData;
   private readonly nint _visitedScan0;
   private readonly int _visitedStride;

   private readonly Queue<Node> _nodeQueue = new();
   private Dictionary<Vector2I, Node> NodeCache { get; } = new();

   private static ReadOnlySpan<byte> BitMasks => [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

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

   #region Pixel Access

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private int GetColor(int x, int y)
   {
      var pixel = (byte*)_scan0 + y * _stride + x * 3;
      return ALPHA | pixel[2] | (pixel[1] << 8) | (pixel[0] << 16);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static int GetColorRowPtr(byte* pixelPtr)
      => ALPHA | pixelPtr[2] | (pixelPtr[1] << 8) | (pixelPtr[0] << 16);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void MarkVisited(int x, int y)
   {
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
         return;
      var row = (byte*)_visitedScan0 + y * _visitedStride;
      row[x >> 3] |= BitMasks[x & 7];
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void MarkVisitedUnchecked(int x, int y)
   {
      var row = (byte*)_visitedScan0 + y * _visitedStride;
      row[x >> 3] |= BitMasks[x & 7];
   }

   #endregion

   #region Segment Color Assignment

   /// <summary>
   /// Sets the polygon color on the appropriate side of the segment.
   /// When traversing forward (IsForward=true), the polygon is on the RIGHT.
   /// When traversing backward (IsForward=false), the polygon is on the LEFT.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static void SetSegmentColor(BorderSegmentDirectional directional, int color)
   {
      if (directional.IsForward)
         directional.Segment.ColorRight = color;
      else
         directional.Segment.ColorLeft = color;
   }

   #endregion

   #region Node Linking

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

   #endregion

   #region Image Edge Tracing

   private void TraceImageEdges()
   {
      var lastColor = GetColor(0, 0);

      Node? firstNode = null;
      Direction firstNodeLeftDir = Direction.North;
      var firstSegment = new BorderSegment();
      firstSegment.Points.Add(new(0, 0));

      Node? lastNode = null;
      Direction lastNodeRightDir = Direction.North;

      var currentSegment = firstSegment;

      MarkVisitedUnchecked(0, 0);

      // Left edge (top to bottom)
      for (var y = 1; y < _height; y++)
      {
         var color = GetColor(0, y);
         if (color != lastColor)
            FinalizeSegment(0, y, color, Direction.East);
         MarkVisitedUnchecked(0, y);
      }

      currentSegment.Points.Add(new(0, _height));

      // Bottom edge (left to right)
      for (var x = 1; x < _width; x++)
      {
         var color = GetColor(x, _height - 1);
         if (color != lastColor)
            FinalizeSegment(x, _height, color, Direction.North);
         MarkVisitedUnchecked(x, _height - 1);
      }

      currentSegment.Points.Add(new(_width, _height));

      // Right edge (bottom to top)
      for (var y = _height - 2; y >= 0; y--)
      {
         var color = GetColor(_width - 1, y);
         if (color != lastColor)
            FinalizeSegment(_width, y + 1, color, Direction.West);
         MarkVisitedUnchecked(_width - 1, y);
      }

      currentSegment.Points.Add(new(_width, 0));

      // Top edge (right to left)
      for (var x = _width - 2; x >= 0; x--)
      {
         var color = GetColor(x, 0);
         if (color != lastColor)
            FinalizeSegment(x + 1, 0, color, Direction.South);
         MarkVisitedUnchecked(x, 0);
      }

      currentSegment.Points.AddRange(firstSegment.Points);

      if (lastNode == null || firstNode == null)
         throw new InvalidOperationException("No nodes were on the border of the image.");

      LinkNodes(lastNode, lastNodeRightDir, firstNode, firstNodeLeftDir, currentSegment);

      return;

      void FinalizeSegment(int xPos, int yPos, int color, Direction direction)
      {
         var newSegment = new BorderSegment();
         var node = Node.CreateBorderNode(xPos, yPos, direction);

         var nodeLeftDir = direction.RotateLeft();
         var nodeRightDir = direction.RotateRight();

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

   #endregion

   #region Edge Tracing with Generic Color Getter

   private Node TraceEdgeFromNode<TGetter>(Node startNode, Direction startDirection)
      where TGetter : struct, IColorGetter
   {
      TraceEdge<TGetter>(startNode.Position, startDirection, out var node, out var arriveDir, false);

      ref var endCache = ref node.GetSegmentRef(arriveDir);
      endCache.Node = startNode;

      ref var startCache = ref startNode.GetSegmentRef(startDirection);
      startCache.Node = node;
      startCache.Segment = endCache.Segment?.Invert();

      return node;
   }

   private void TraceEdge<TGetter>(Vector2I startPos, Direction startDirection, out Node resultNode, out Direction resultArriveDir, bool loopCheck)
      where TGetter : struct, IColorGetter
   {
      TGetter getter = default;
      var points = DirectionHelper.GetStartPos(startPos.X, startPos.Y, startDirection);

      var lColor = getter.GetColor(_scan0, _stride, _width, _height, points.Xl, points.Yl);
      var rColor = getter.GetColor(_scan0, _stride, _width, _height, points.Xr, points.Yr);

      var currentDirection = startDirection;
      var currentSegment = new BorderSegment();

      var startPointX = points.Xpos;
      var startPointY = points.Ypos;

      while (true)
      {
         MarkVisited(points.Xl, points.Yl);
         MarkVisited(points.Xr, points.Yr);

         currentDirection.Move(ref points, out var cachePos, out var xaxis);
         
         var lTest = getter.GetColor(_scan0, _stride, _width, _height, points.Xl, points.Yl);
         var rTest = getter.GetColor(_scan0, _stride, _width, _height, points.Xr, points.Yr);
         var arriveDirection = currentDirection.Invert();
         
         if (loopCheck && points.Xpos == startPointX && points.Ypos == startPointY)
         {
            // Need to check first if it is a complex node or just a simple node
            // E.g. in an edge case this might be a three or even 4 way node!
            
            currentSegment.Points.Add(points.GetPosition());
            Node loopNode;
            
            // L line inverted L
            if ((lTest == lColor && rTest == lColor) || (lTest == lColor && rTest == rColor) ||
                (lTest == rColor && rTest == rColor))
            {
               loopNode = Node.CreateOneWayNode(points.Xpos, points.Ypos, startDirection);
               resultArriveDir = startDirection;
            }
            else
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

               loopNode = isThreeWayNode
                  ? Node.CreateThreeWayNode(points.Xpos, points.Ypos, dir)
                  : Node.CreateFourWayNode(points.Xpos, points.Ypos);
               
               ref var startCache = ref loopNode.GetSegmentRef(arriveDirection);
               startCache.Segment = new BorderSegmentDirectional(currentSegment, false); 
               startCache.Node = loopNode;
               
               resultArriveDir = currentDirection;
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
            if (xaxis) points.Xr = cachePos; else points.Yr = cachePos;

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
            if (xaxis) points.Xl = cachePos; else points.Yl = cachePos;

            currentDirection = (Direction)(((int)currentDirection - 1) & 3);
            rColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         // Node found

         if (!NodeCache.TryGetValue(points.GetPosition(), out var node))
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

            node = isThreeWayNode
                      ? Node.CreateThreeWayNode(points.Xpos, points.Ypos, dir)
                      : Node.CreateFourWayNode(points.Xpos, points.Ypos);

            ref var cache = ref node.GetSegmentRef(arriveDirection);
            cache.Segment = new BorderSegmentDirectional(currentSegment, false);

            NodeCache.Add(points.GetPosition(), node);
            _nodeQueue.Enqueue(node);
         }
         else
         {
            ref var cache = ref node.GetSegmentRef(arriveDirection);
            cache.Segment = new BorderSegmentDirectional(currentSegment, false);
         }

         MarkVisited(points.Xl, points.Yl);
         MarkVisited(points.Xr, points.Yr);

         resultNode = node;
         resultArriveDir = arriveDirection;
         return;
      }
   }

   #endregion

   #region Stub Tracing

   private void TraceEdgeStubs()
   {
      var nodes = NodeCache.Values.ToArray();
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
            TraceEdgeFromNode<CheckedColorGetter>(node, dir);
            break;
         }
      }
   }

   #endregion

   #region Polygon Tracing

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
         currentNode = TraceEdgeFromNode<UncheckedColorGetter>(startNode, currentDirection);
         currentSegment = startNode.GetSegmentRef(currentDirection).Segment!.Value;
      }
      else
      {
         currentNode = firstCache.Node!;
         currentSegment = firstCache.Segment.Value;
      }

      polygon.Segments.Add(currentSegment);
      SetSegmentColor(currentSegment, polygon.Color);
      
      // False check. I need to check if the direction i want to go in is already visited not if the node is fine
      while (true)
      {
         if (currentNode.Visit(ref currentDirection, currentSegment, out var newSegment, out var nextNode))
         {
            if (currentNode.IsVisited(currentDirection))
               break;
            currentNode.SetVisited(currentDirection);
            polygon.Segments.Add(currentNode);
            polygon.Segments.Add(newSegment);
            SetSegmentColor(newSegment, polygon.Color);
            currentNode = nextNode;
            currentSegment = newSegment;
            
         }
         else
         {
            polygon.Segments.Add(currentNode);
            currentNode.SetVisited(currentDirection);
            var newNode = TraceEdgeFromNode<UncheckedColorGetter>(currentNode, currentDirection);
            currentSegment = currentNode.GetSegmentRef(currentDirection).Segment!.Value;
            polygon.Segments.Add(currentSegment);
            SetSegmentColor(currentSegment, polygon.Color);
            currentNode = newNode;
         }
      }


      return polygon;
   }

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

   private PolygonParsing TraceIsland(Node node, Dictionary<int, List<PolygonParsing>> polygons,
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
         if (!exists) listRef = [];
         listRef!.Add(polygon);
      }
      return hole;
   }

   private PolygonParsing[] VisitNode(Node node, Dictionary<int, List<PolygonParsing>> polygons, ReadOnlySpan<Direction> arriveDirections)
   {
#if DEBUG
      foreach (var arrDir in arriveDirections)
         Debug.Assert(node.HasDirection(arrDir));
#endif

      var foundPolygons = new PolygonParsing[arriveDirections.Length];
      var index = 0;

      for (var i = 0; i < 4; i++)
      {
         var direction = (Direction)i;
         if (!node.HasDirection(direction) || node.IsVisited(direction))
            continue;

         var polygon = TraceFromNode(node, direction);

         var isArriveDir = false;
         foreach (var arrDir in arriveDirections)
         {
            if (arrDir == direction)
            {
               isArriveDir = true;
               break;
            }
         }

         if (isArriveDir)
         {
            foundPolygons[index++] = polygon;
            continue;
         }

         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygons, polygon.Color, out var exists);
         if (!exists) listRef = [];
         listRef!.Add(polygon);
      }

      return foundPolygons;
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
         if (!exists) listRef = [];
         listRef!.Add(polygon);
      }
   }

   #endregion

   #region Island Handling

   private void HandleIsland(Vector2I position, Dictionary<int, List<PolygonParsing>> polygons, Vector2I borderPos, int parentColor, ref PolygonParsing? parent)
   {
      // Instead of parsing from a node, we parse from a segment by finding the start node and tracing from there.
      // The edge case that no node exists will have to be taken into account.
      // First, find the nodes at the start and end of the segment.

      // We have a border on the left of the position
      
      Debug.Assert(NodeCache.Count == 0);
      Debug.Assert(_nodeQueue.Count == 0);

      var startPosition = new Vector2I(position.X, position.Y + 1);
      
      TraceEdge<UncheckedColorGetter>(startPosition, Direction.North, out var startNode, out var startArriveDir, true);

      if (!polygons.TryGetValue(parentColor, out var parentPolygonCandidates))
      {
         NodeCache.Clear();
         _nodeQueue.Clear();
         return;
      }

      // quick check the last polygon to see if it is still the parent
      if (parent == null || parent.Color != parentColor || !parent.IsOnBorder(borderPos))
      {
         var count = parentPolygonCandidates.Count;
         for (var i = 0; i < count; i++)
         {
            var candidate = parentPolygonCandidates[i];
            if (!candidate.IsOnBorder(borderPos)) continue;
            parent = candidate;
            break;
         }

         if (parent == null)
         {
            NodeCache.Clear();
            _nodeQueue.Clear();
            return;
         }
      }

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
         SetSegmentColor(invertedSeg, islandColor);
         SetSegmentColor(loopCache.Segment!.Value, parentColor);
         
         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygons, islandColor, out var exists);
         if (!exists) listRef = [];
         listRef!.Add(polygon);
         
         //TODO Color can be set to 0 again
         hole = new PolygonParsing(islandColor);
         hole.Segments.Add(loopCache.Segment!.Value);
         hole.Segments.Add(startNode);
         parent.Holes.Add(hole);

         _nodeQueue.Clear();
         NodeCache.Clear();
         return;
      }
      
      if (startNode.Position == startPosition)
      {
         // Complex island with existing loopNode
         NodeCache.Add(startPosition, startNode);
      }
      else
      {
         // Complex island with multiple nodes
         TraceEdge<UncheckedColorGetter>(position, Direction.South, out var endNode, out var endArriveDir, false);
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
      //Debug.Assert(position.X != 14974 && position.Y != 5476);
      //hole = TraceIsland(startNode, polygons, startArriveDir);
      
      // TODO maybe readd the TraceIsland method to prevent duplicates in the normal visitNodes
      //hole = TraceFromNode(startNode, startArriveDir);
      hole = TraceIsland(startNode, polygons, startArriveDir);
      //NodeCache.Remove(startNode.Position);

      while (_nodeQueue.TryDequeue(out var node))
         VisitNode(node, polygons);

      NodeCache.Clear();
      parent.Holes.Add(hole);
   }

   #endregion

   #region Main Entry Point

   public List<PolygonParsing> Trace()
   {
#if DEBUG
      var sw2 = Stopwatch.StartNew();
#endif
      TraceImageEdges();
#if DEBUG
      sw2.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"TraceImageEdges in {sw2.ElapsedMilliseconds} ms");
#endif

#if DEBUG
      var sw3 = Stopwatch.StartNew();
#endif
      TraceEdgeStubs();
#if DEBUG
      sw3.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"TraceEdgeStubs in {sw3.ElapsedMilliseconds} ms");
#endif

#if DEBUG
      var sw4 = Stopwatch.StartNew();
#endif
      List<PolygonParsing> polygons = [];
      while (_nodeQueue.TryDequeue(out var node))
         VisitNode(node, polygons);
#if DEBUG
      sw4.Stop();
      ArcLog.WriteLine("DBT", LogLevel.INF, $"VisitNodes in {sw4.ElapsedMilliseconds} ms");
#endif

      NodeCache.Clear();

#if DEBUG
      var sw = Stopwatch.StartNew();
#endif

      Dictionary<int, List<PolygonParsing>> polygonsDict = new();
      var polySpan = CollectionsMarshal.AsSpan(polygons);
      for (var i = 0; i < polySpan.Length; i++)
      {
         var poly = polySpan[i];
         ref var listRef = ref CollectionsMarshal.GetValueRefOrAddDefault(polygonsDict, poly.Color, out var exists);
         if (!exists) listRef = [];
         listRef!.Add(poly);
      }

      var counter = 0;
      var lastSwitchX = 0;
      var lastSwitchY = 0;
      PolygonParsing? lastPolygon = null;
      fixed (byte* bitMaskPtr = BitMasks)
      {
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
                     HandleIsland(new(x, y), polygonsDict, new(lastSwitchX, lastSwitchY), lastColor, ref lastPolygon);
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
      }

#if DEBUG
      sw.Stop();
      ArcLog.WriteLine("MPT", LogLevel.DBG, $"Found and traced {counter} islands in {sw.ElapsedMilliseconds} ms.");
#endif
   
      // TODO return dictionary since we need it anyway
      return polygonsDict.SelectMany(kvp => kvp.Value).ToList();
   }

   #endregion

   #region Disposable

   private bool _disposed;

   public void Dispose()
   {
      if (_disposed) return;
      _disposed = true;

      _bitmap.UnlockBits(_bitmapData);
      _visitedBitmap.UnlockBits(_visitedBitmapData);
      _visitedBitmap.Dispose();
   }

   #endregion
}