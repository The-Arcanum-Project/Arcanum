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
   private const int OUTSIDE_COLOR = 0x000000;
   private readonly int _width;
   private readonly int _height;
   private readonly int _stride;
   private readonly IntPtr _scan0;
   private readonly Bitmap _bitmap;
   private readonly BitmapData _bitmapData;
   private readonly Bitmap _visitedBitmap;
   private readonly BitmapData _visitedBitmapData;
   private readonly IntPtr _visitedBitmapDataPtr;
   private readonly int _visitedStride;

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
      _visitedBitmapDataPtr = _visitedBitmapData.Scan0;
      _visitedStride = _visitedBitmapData.Stride;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private int GetColor(int x, int y)
   {
      var row = (byte*)_scan0 + (y * _stride);
      var pixel = row + x * 3;

      return ALPHA |
             (pixel[2]) | // Red
             (pixel[1] << 8) | // Green
             (pixel[0] << 16); // Blue
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static int GetColorRowPtr(byte* pixelPtr) => ALPHA |
                                                        (pixelPtr[2]) |
                                                        (pixelPtr[1] << 8) |
                                                        (pixelPtr[0] << 16);

   private static ReadOnlySpan<byte> BitMasks => [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void ClearPixel(int x, int y)
   {
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
         return;

      var row = (byte*)_visitedBitmapDataPtr + y * _visitedStride;
      row[x >> 3] |= BitMasks[x & 7];
   }

   private bool IsPixelCleared(int i, int i1)
   {
      var row = (byte*)_visitedBitmapDataPtr + i1 * _visitedStride;
      return (row[i / 8] & (byte)(0x80 >> (i % 8))) != 0;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private int GetColorWithOutsideCheck(int x, int y)
   {
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
         return OUTSIDE_COLOR;

      return GetColor(x, y);
   }

   private int GetColorAndSetCleared(int x, int y)
   {
      if (x < 0 || x >= _width || y < 0 || y >= _height)
         return OUTSIDE_COLOR;

      ClearPixel(x, y);
      return GetColor(x, y);
   }

   private int GetColorWithOutsideCheck(Vector2I pos)
   {
      return GetColorWithOutsideCheck(pos.X, pos.Y);
   }

   private static void LinkNodes(Node a, Direction aDir, Node b, Direction bDir, BorderSegment segment)
   {
      ref var aCache = ref a.GetSegmentRef(aDir);
      aCache.Node = b;
      aCache.Segment = new BorderSegmentDirectional(segment, true);

      ref var bCache = ref b.GetSegmentRef(bDir);
      bCache.Node = a;
      bCache.Segment = new BorderSegmentDirectional(segment, false);
   }

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

      ClearPixel(0, 0);
      for (var y = 1; y <= _height - 1; y++)
      {
         var color = GetColor(0, y);
         if (color != lastColor)
            FinalizeSegment(0, y, color, Direction.East);

         ClearPixel(0, y);
      }

      currentSegment.Points.Add(new(0, _height));
      ClearPixel(0, _height - 1);
      for (var x = 1; x <= _width - 1; x++)
      {
         var color = GetColor(x, _height - 1);
         if (color != lastColor)
            FinalizeSegment(x, _height, color, Direction.North);

         ClearPixel(x, _height - 1);
      }

      currentSegment.Points.Add(new(_width, _height));
      ClearPixel(_width - 1, _height - 1);
      for (var y = _height - 1; y > 0; y--)
      {
         var color = GetColor(_width - 1, y - 1);
         if (color != lastColor)
            FinalizeSegment(_width, y, color, Direction.West);

         ClearPixel(_width - 1, y - 1);
      }

      currentSegment.Points.Add(new(_width, 0));
      ClearPixel(_width - 1, 0);
      for (var x = _width - 1; x > 0; x--)
      {
         var color = GetColor(x - 1, 0);
         if (color != lastColor)
            FinalizeSegment(x, 0, color, Direction.South);

         ClearPixel(x - 1, 0);
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

   private Node TraceEdgeStartNodeWithOutsideCheck(Node startNode, Direction startDirection)
   {
      TraceEdge(startNode.Position, startDirection, out var node, out var arriveDir);

      ref var endCache = ref node.GetSegmentRef(arriveDir);
      endCache.Node = startNode;

      ref var startCache = ref startNode.GetSegmentRef(startDirection);
      startCache.Node = node;
      startCache.Segment = endCache.Segment?.Invert();

      return node;
   }

   private void TraceEdge(Vector2I startPos, Direction startDirection, out Node resultNode, out Direction resultArriveDir, bool loopCheck = false)
   {
      var points = DirectionHelper.GetStartPos(startPos.X, startPos.Y, startDirection);

      var lColor = GetColorWithOutsideCheck(points.Xl, points.Yl);
      var rColor = GetColorWithOutsideCheck(points.Xr, points.Yr);

      var currentDirection = startDirection;
      var currentSegment = new BorderSegment();

      var startPointX = points.Xpos;
      var startPointY = points.Ypos;

      while (true)
      {
         ClearPixel(points.Xl, points.Yl);
         ClearPixel(points.Xr, points.Yr);

         currentDirection.Move(ref points, out var cachePos, out var xaxis);

         if (loopCheck && points.Xpos == startPointX && points.Ypos == startPointY)
         {
            currentSegment.Points.Add(points.GetPosition());
            var loopNode = Node.CreateOneWayNode(points.Xpos, points.Ypos, startDirection);

            ref var loopCache = ref loopNode.GetSegmentRef(startDirection);
            loopCache.Segment = new BorderSegmentDirectional(currentSegment, false);

            resultNode = loopNode;
            resultArriveDir = startDirection;
            return;
         }

         var lTest = GetColorWithOutsideCheck(points.Xl, points.Yl);
         var rTest = GetColorWithOutsideCheck(points.Xr, points.Yr);

         if (lTest == lColor && rTest == rColor)
            continue;

         if (lTest == lColor && rTest == lColor)
         {
            points.Xl = points.Xr;
            points.Yl = points.Yr;
            if (xaxis)
               points.Xr = cachePos;
            else
               points.Yr = cachePos;

            currentDirection = (Direction)(((int)currentDirection + 1) & 3);
            lColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         if (lTest == rColor && rTest == rColor)
         {
            points.Xr = points.Xl;
            points.Yr = points.Yl;
            if (xaxis)
               points.Xl = cachePos;
            else
               points.Yl = cachePos;

            currentDirection = (Direction)(((int)currentDirection - 1) & 3);
            rColor = lTest;
            currentSegment.Points.Add(points.GetPosition());
            continue;
         }

         var arriveDirection = currentDirection.Invert();

         if (!NodeCache.TryGetValue(points.GetPosition(), out var node))
         {
            var dir = arriveDirection;
            var isThreeWayNode = (rTest == lTest);

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

         ClearPixel(points.Xl, points.Yl);
         ClearPixel(points.Xr, points.Yr);

         resultNode = node;
         resultArriveDir = arriveDirection;
         return;
      }
   }

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

            TraceEdgeStartNodeWithOutsideCheck(node, dir);
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

      BorderSegmentDirectional currentSegment;

      polygon.Segments.Add(currentNode);
      if (!firstCache.Segment.HasValue)
      {
         var newNode = TraceEdgeStartNodeWithOutsideCheck(currentNode, currentDirection);
         currentSegment = currentNode.GetSegmentRef(currentDirection).Segment!.Value;
         currentNode = newNode;
      }
      else
      {
         currentNode = firstCache.Node!;
         currentSegment = firstCache.Segment.Value;
      }

      polygon.Segments.Add(currentSegment);
      while (true)
      {
         if (currentNode == startNode)
            break;

         if (currentNode.Visit(ref currentDirection, currentSegment, out var newSegment, out var nextNode))
         {
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
            var newNode = TraceEdgeStartNodeWithOutsideCheck(currentNode, currentDirection);
            currentSegment = currentNode.GetSegmentRef(currentDirection).Segment!.Value;
            currentNode = newNode;
            polygon.Segments.Add(currentSegment);
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

         var polygon = TraceFromNode(node, direction);
         polygons.Add(polygon);
      }
   }

   private PolygonParsing[] VisitNode(Node node, Dictionary<int, List<PolygonParsing>> polygons, Direction[] arriveDirections)
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
         for (var k = 0; k < arriveDirections.Length; k++)
            if (arriveDirections[k] == direction)
            {
               isArriveDir = true;
               break;
            }

         if (isArriveDir)
         {
            foundPolygons[index++] = polygon;
            continue;
         }

         if (!polygons.TryGetValue(polygon.Color, out var polygonsList))
         {
            polygonsList = [];
            polygons[polygon.Color] = polygonsList;
         }

         polygonsList.Add(polygon);
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

         if (!exists)
            listRef = [];

         listRef!.Add(polygon);
      }
   }

   private void HandleIsland(Vector2I position, Dictionary<int, List<PolygonParsing>> polygons, Vector2I borderPos, int parentColor)
   {
      Debug.Assert(NodeCache.Count == 0);
      Debug.Assert(_nodeQueue.Count == 0);

      TraceEdge(new(position.X, position.Y + 1), Direction.North, out var startNode, out var startArriveDir, true);

      if (!polygons.TryGetValue(parentColor, out var parentPolygonCandidates))
      {
         NodeCache.Clear();
         _nodeQueue.Clear();
         return;
      }

      PolygonParsing? parent = null;
      var count = parentPolygonCandidates.Count;
      for (var i = 0; i < count; i++)
      {
         var candidate = parentPolygonCandidates[i];
         if (candidate.IsOnBorder(borderPos))
         {
            parent = candidate;
            break;
         }
      }

      if (parent == null)
      {
         NodeCache.Clear();
         _nodeQueue.Clear();
         return;
      }

      if (startNode.ActiveDirectionCount == 1)
      {
         var polygon = new PolygonParsing(GetColorWithOutsideCheck(position.X, position.Y));
         ref var loopCache = ref startNode.GetSegmentRef(startArriveDir);
         polygon.Segments.Add(loopCache.Segment!.Value.Invert());

         if (!polygons.TryGetValue(polygon.Color, out var polygonsList))
            polygons[polygon.Color] = [polygon];
         else
            polygonsList.Add(polygon);

         var hole = new PolygonParsing(0);
         hole.Segments.Add(loopCache.Segment!.Value);
         hole.Segments.Add(startNode);
         parent.Holes.Add(hole);

         _nodeQueue.Clear();
         NodeCache.Clear();
         return;
      }

      TraceEdge(position, Direction.South, out var endNode, out var endArriveDir);

      var segment = new BorderSegment();
      ref var startCache = ref startNode.GetSegmentRef(startArriveDir);
      startCache.Segment!.Value.AddTo(segment.Points);

      ref var endCache = ref endNode.GetSegmentRef(endArriveDir);
      endCache.Segment!.Value.Invert().AddTo(segment.Points);

      startCache.Segment = new BorderSegmentDirectional(segment, true);
      endCache.Segment = new BorderSegmentDirectional(segment, false);
      startCache.Node = endNode;
      endCache.Node = startNode;

      var holes = VisitNode(startNode,
                            polygons,
                            startNode == endNode ? [startArriveDir, startArriveDir.Invert()] : [startArriveDir]);

      NodeCache.Remove(startNode.Position);

      while (_nodeQueue.TryDequeue(out var node))
         VisitNode(node, polygons);

      NodeCache.Clear();
      parent.Holes.AddRange(holes);
   }

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

      var sw = new Stopwatch();
      sw.Start();

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
      fixed (byte* bitMaskPtr = BitMasks)
      {
         for (var y = 0; y < _height; y++)
         {
            var row = (byte*)_scan0 + y * _stride;
            var visitedRow = (byte*)_visitedBitmapDataPtr + y * _visitedStride;

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
                     HandleIsland(new(x, y), polygonsDict, new(lastSwitchX, lastSwitchY), lastColor);
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

      sw.Stop();
      ArcLog.WriteLine("MPT", LogLevel.DBG, $"Found and traced {counter} islands in {sw.ElapsedMilliseconds} ms.");

      return polygonsDict.SelectMany(kvp => kvp.Value).ToList();
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