using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
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

   private Dictionary<Vector2I, Node> NodeCache { get; } = new ();

   public MapTracing(Bitmap bmp)
   {
      _bitmap = bmp;
      _bitmapData = _bitmap.LockBits(new (0, 0, bmp.Width, bmp.Height),
                                     ImageLockMode.ReadOnly,
                                     PixelFormat.Format24bppRgb);
      _width = _bitmapData.Width;
      _height = _bitmapData.Height;
      _stride = _bitmapData.Stride;
      _scan0 = _bitmapData.Scan0;
      _visitedBitmap = new (_bitmap.Width, _bitmap.Height, PixelFormat.Format1bppIndexed);
      _visitedBitmapData = _visitedBitmap.LockBits(new (0, 0, bmp.Width, bmp.Height),
                                                   ImageLockMode.ReadWrite,
                                                   PixelFormat.Format1bppIndexed);
      _visitedBitmapDataPtr = _visitedBitmapData.Scan0;
      _visitedStride = _visitedBitmapData.Stride;
   }

   private int GetColor(int x, int y)
   {
      var row = (byte*)_scan0 + y * _stride;
      var xTimesThree = x * 3;
      return ALPHA |
             (row[xTimesThree + 2]) |
             (row[xTimesThree + 1] << 8) |
             row[xTimesThree] << 16;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void ClearPixel(int x, int y)
   {
      if (x < 0 || x >= _width || y < 0 || y >= _height)
         return;

      var row = (byte*)_visitedBitmapDataPtr + y * _visitedStride;
      // Set the bit to 1
      row[x / 8] |= (byte)(0x80 >> (x % 8));
   }

   // ReSharper disable once UnusedMember.Local
   private bool IsPixelCleared(int i, int i1)
   {
      var row = (byte*)_visitedBitmapDataPtr + i1 * _visitedStride;
      return (row[i / 8] & (byte)(0x80 >> (i % 8))) != 0;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private int GetColorWithOutsideCheck(int x, int y)
   {
      if (x < 0 || x >= _width || y < 0 || y >= _height)
         return OUTSIDE_COLOR;

      return GetColor(x, y);
   }

   // ReSharper disable once UnusedMember.Local
   private int GetColorAndSetCleared(int x, int y)
   {
      if (x < 0 || x >= _width || y < 0 || y >= _height)
         return OUTSIDE_COLOR;

      ClearPixel(x, y);
      return GetColor(x, y);
   }

   // ReSharper disable once UnusedMember.Local
   private int GetColorWithOutsideCheck(Vector2I pos)
   {
      return GetColorWithOutsideCheck(pos.X, pos.Y);
   }

   private static void LinkNodes(Node a, Node b, BorderSegment segment)
   {
      a.Segments[2].Node = b;
      a.Segments[2].Segment = new (segment, true);
      b.Segments[0].Node = a;
      b.Segments[0].Segment = new (segment, false);
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
      firstSegment.Points.Add(new (0, 0));

      Node? lastNode = null;

      var currentSegment = firstSegment;

      ClearPixel(0, 0);
      // Left Edge (top to bottom)
      for (var y = 1; y <= _height - 1; y++)
      {
         var color = GetColor(0, y);
         if (color != lastColor)
            FinalizeSegment(0, y, color, Direction.East);

         ClearPixel(0, y);
      }

      // Bottom Left Corner
      currentSegment.Points.Add(new (0, _height));
      ClearPixel(0, _height - 1);
      // Bottom Edge (left to right)
      for (var x = 1; x <= _width - 1; x++)
      {
         var color = GetColor(x, _height - 1);
         if (color != lastColor)
            FinalizeSegment(x, _height, color, Direction.North);

         ClearPixel(x, _height - 1);
      }

      // Bottom Right Corner
      currentSegment.Points.Add(new (_width, _height));
      ClearPixel(_width - 1, _height - 1);
      // Right Edge (bottom to top)
      for (var y = _height - 1; y > 0; y--)
      {
         var color = GetColor(_width - 1, y - 1);
         if (color != lastColor)
            FinalizeSegment(_width, y, color, Direction.West);

         ClearPixel(_width - 1, y - 1);
      }

      // Top Right Corner
      currentSegment.Points.Add(new (_width, 0));
      ClearPixel(_width - 1, 0);
      // Top Edge (right to left)
      for (var x = _width - 1; x > 0; x--)
      {
         var color = GetColor(x - 1, 0);
         if (color != lastColor)
            FinalizeSegment(x, 0, color, Direction.South);

         ClearPixel(x - 1, 0);
      }

      currentSegment.Points.AddRange(firstSegment.Points);

      //TODO: @MelCo: This currently does not handle the case where there are no nodes on the border.
      if (lastNode == null || firstNode == null)
         throw new
            InvalidOperationException("No nodes were on the border of the image. We currently do not support maps without nodes on the border.");

      LinkNodes(lastNode, firstNode, currentSegment);

      return;

      void FinalizeSegment(int xPos, int yPos, int color, Direction direction)
      {
         // Color changed, finalize the current segment and start a new one

         var newSegment = new BorderSegment();

         Node node = new (xPos, yPos, direction, true);

         // First node found

         if (lastNode is null)
            firstNode = node;
         else
            LinkNodes(lastNode, node, currentSegment);

         // Update caches and references

         NodeCache.Add(new (xPos, yPos), node);
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
      var (node, cache) = TraceEdge(startNode.Position, startDirection);

      cache.Node = startNode;

      var startCache = startNode.GetSegment(startDirection);
      startCache.Node = node;
      startCache.Segment = cache.Segment?.Invert();

      return node;
   }

   private (Node, CacheNodeInfo) TraceEdge(Vector2I startPos, Direction startDirection, bool loopCheck = false)
   {
      // Get pixel positions to the left and right of the start node in the given direction
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
            // We have looped back to the start position without finding a node.
            // This can happen in case of small enclosed areas.
            // To prevent infinite loops, we create a node here.
            currentSegment.Points.Add(points.GetPosition());
            var loopNode = Node.GetOneWayNode(points.Xpos, points.Ypos, startDirection);
            var segment = loopNode.Segments[0];
            segment.Segment = new (currentSegment, false);
            return (loopNode, segment);
         }

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

         CacheNodeInfo cache;

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

            cache = node.GetSegment(arriveDirection);
            //segment.Node = startNode;
            cache.Segment = new (currentSegment, false);
            NodeCache.Add(points.GetPosition(), node);
         }
         else
         {
            cache = node.GetSegment(arriveDirection);
            //cache.Node = startNode;
            cache.Segment = new (currentSegment, false);
         }

         ClearPixel(points.Xl, points.Yl);
         ClearPixel(points.Xr, points.Yr);
         return (node, cache);
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

   //TODO: @MelCo: Optimize this method
   private PolygonParsing[] VisitNode(Node node,
                                      Dictionary<int, List<PolygonParsing>> polygons,
                                      Direction[] arriveDirection)
   {
      var dirs = node.Segments.Select(s => s.Dir).ToList();
      Debug.Assert(arriveDirection.All(a => dirs.Contains(a)));
      var foundPolygon = new PolygonParsing[arriveDirection.Length];
      var index = 0;
      foreach (var direction in dirs)
      {
         if (node.TestDirection(direction))
            continue;

         var polygon = TraceFromNode(node, direction);

         if (arriveDirection.Contains(direction))
         {
            foundPolygon[index++] = polygon;
            continue;
         }

         if (!polygons.TryGetValue(polygon.Color, out var polygonsList))
            polygons[polygon.Color] = [polygon];
         else
            polygonsList.Add(polygon);
      }

      return foundPolygon;
   }

   private void VisitNode(Node node, Dictionary<int, List<PolygonParsing>> polygons)
   {
      var dirs = node.Segments.Select(s => s.Dir).ToList();

      foreach (var direction in dirs)
      {
         if (node.TestDirection(direction))
            continue;

         var polygon = TraceFromNode(node, direction);

         if (!polygons.TryGetValue(polygon.Color, out var polygonsList))
            polygons[polygon.Color] = [polygon];
         else
            polygonsList.Add(polygon);
      }
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="position">Position of the new color on the right of the previous color</param>
   /// <param name="polygons">List of polygons where new ones are added to</param>
   /// <param name="borderPos"></param>
   /// <param name="parentColor"></param>
   private void HandleIsland(Vector2I position,
                             Dictionary<int, List<PolygonParsing>> polygons,
                             Vector2I borderPos,
                             int parentColor)
   {
      // Instead of parsing from a node, we parse from a segment by finding the start node and tracing from there.
      // The edge case that no node exists will have to be taken into account.
      // First, find the nodes at the start and end of the segment.

      // We have a border on the left of the position

      Debug.Assert(NodeCache.Count == 0);

      var (startNode, startCache) = TraceEdge(new (position.X, position.Y + 1), Direction.North, true);

      if (!polygons.ContainsKey(parentColor))
      {
         NodeCache.Clear();
         return;
      }

      var parentPolygonCandidates = polygons[parentColor];
      //TODO: @MelCo: Cache this value while being in the same polygon
      var parent = parentPolygonCandidates.FirstOrDefault(polygonCandidate => polygonCandidate.IsOnBorder(borderPos));

      if (parent == null)
      {
         NodeCache.Clear();
         return;
      }

      if (startNode.Segments.Length == 1)
      {
         // Loop detected so directly creates a polygon
         var polygon = new PolygonParsing(GetColorWithOutsideCheck(position.X, position.Y));
         polygon.Segments.Add(startCache.Segment!.Value.Invert());
         if (!polygons.TryGetValue(polygon.Color, out var polygonsList))
            polygons[polygon.Color] = [polygon];
         else
            polygonsList.Add(polygon);

         var hole = new PolygonParsing(0);

         hole.Segments.Add(startCache.Segment!.Value);
         hole.Segments.Add(startNode);
         parent.Holes.Add(hole);
         return;
      }

      var (endNode, endCache) = TraceEdge(position, Direction.South);

      // Create a new segment from start to end node and link them
      var segment = new BorderSegment();
      var segmentDir = startCache.Segment!.Value;
      segmentDir.AddTo(segment.Points);
      endCache.Segment!.Value.Invert().AddTo(segment.Points);
      startCache.Segment = new (segment, true);
      endCache.Segment = new (segment, false);
      startCache.Node = endNode;
      endCache.Node = startNode;

      // Now find all other nodes along the segment

      var holes = VisitNode(startNode,
                            polygons,
                            startNode == endNode ? [startCache.Dir, startCache.Dir.Invert()] : [startCache.Dir]);

      NodeCache.Remove(startNode.Position);

      while (NodeCache.Count > 0)
      {
         var node = NodeCache.First();
         VisitNode(node.Value, polygons);
         NodeCache.Remove(node.Key);
      }

      parent.Holes.AddRange(holes);
   }

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

      // go through the entire visited bitmap and find borders which have not been visited yet
      var sw = new Stopwatch();
      sw.Start();

      //TODO: @MelCo: Fix this and remove later
      Dictionary<int, List<PolygonParsing>> polygonsDict = new ();

      foreach (var polygonParsing in polygons)
         if (!polygonsDict.TryGetValue(polygonParsing.Color, out var polygonsList))
            polygonsDict[polygonParsing.Color] = [polygonParsing];
         else
            polygonsList.Add(polygonParsing);

      var counter = 0;

      var lastSwitchX = 0;
      var lastSwitchY = 0;

      for (var y = 0; y < _height; y++)
      {
         var row = (byte*)_scan0 + y * _stride;
         var visitedRow = (byte*)_visitedBitmapDataPtr + y * _visitedStride;

         var lastColor = OUTSIDE_COLOR;

         for (var x = 0; x < _width; x++)
         {
            var idx = x * 3;
            var color = ALPHA |
                        (row[idx + 2]) |
                        (row[idx + 1] << 8) |
                        row[idx] << 16;

            var mask = (byte)(0x80 >> (x % 8));
            if (color != lastColor)
            {
               if ((visitedRow[x / 8] & mask) == 0)
               {
                  HandleIsland(new (x, y), polygonsDict, new (lastSwitchX, lastSwitchY), lastColor);
                  counter++;
               }

               lastSwitchX = x;
               lastSwitchY = y;

               lastColor = color;
            }
            else
               visitedRow[x / 8] |= mask;
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