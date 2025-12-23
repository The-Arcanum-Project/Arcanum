using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

public class CacheNodeInfo(Node? node, BorderSegmentDirectional? segment, Direction dir, bool visited = false)
{
   public readonly Direction Dir = dir;
   public bool Visited = visited;
   public BorderSegmentDirectional? Segment = segment;
   public Node? Node = node;
}

public class Node : ICoordinateAdder
{
#if DEBUG
   private static int _totalNodes;
   private readonly int _nodeId;

   public override string ToString() => $"Node {_nodeId} at ({XPos}, {YPos})";

#endif

   public readonly Vector2I Position;
   public int XPos => Position.X;
   public int YPos => Position.Y;

   public readonly CacheNodeInfo[] Segments;

   /// <summary>
   /// Initializes a new instance of the <see cref="Node"/> class with the given segments and position.
   /// </summary>
   /// <param name="segments">Array of cached node information for each direction.</param>
   /// <param name="xPos">The X position of the node.</param>
   /// <param name="yPos">The Y position of the node.</param>
   public Node(CacheNodeInfo[] segments,
               int xPos,
               int yPos)
   {
#if DEBUG
      _nodeId = _totalNodes;
      _totalNodes++;
#endif

      Segments = segments;
      Position = new(xPos, yPos);
   }

   /// <summary>
   /// Creates a new node template with segments in all three directions (left, straight, right) based on the given direction.
   /// </summary>
   /// <param name="xPos"></param>
   /// <param name="yPos"></param>
   /// <param name="dir"></param>
   /// <param name="lastNodeVisited"></param>
   public Node(int xPos, int yPos, Direction dir, bool lastNodeVisited = false) : this([
                                                                                          new(null, null, dir.RotateLeft()), new(null, null, dir),
                                                                                          new(null, null, dir.RotateRight(), visited: lastNodeVisited)
                                                                                       ],
                                                                                       xPos,
                                                                                       yPos)
   {
   }

   public static Node GetThreeWayNode(int xPos, int yPos, Direction dir) => new([
                                                                                   new(null, null, dir.RotateRight()), new(null, null, dir),
                                                                                   new(null, null, dir.RotateLeft())
                                                                                ],
                                                                                xPos,
                                                                                yPos);

   public static Node GetOneWayNode(int xPos, int yPos, Direction dir) => new([new(null, null, dir),], xPos, yPos);

   public static Node GetFourWayNode(int xPos, int yPos, Direction dir) => new([
                                                                                  new(null, null, dir.Invert()), new(null, null, dir.RotateRight()),
                                                                                  new(null, null, dir.RotateLeft()), new(null, null, dir)
                                                                               ],
                                                                               xPos,
                                                                               yPos);

   /// <summary>
   /// Adds the node's position as a <see cref="Point"/> to the provided list.
   /// </summary>
   /// <param name="points">The list to add the point to.</param>
   public void AddTo(List<Vector2I> points)
   {
      points.Add(new(XPos, YPos));
   }

   /// <summary>
   /// Marks the segment in the specified direction as visited.
   /// </summary>
   /// <param name="dir">The direction of the segment to mark as visited.</param>
   public void SetDirection(Direction dir)
   {
      GetSegment(dir).Visited = true;
   }

   /// <summary>
   /// Checks if the segment in the specified direction has been visited.
   /// </summary>
   /// <param name="dir">The direction to check.</param>
   /// <returns>True if the segment has been visited; otherwise, false.</returns>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool TestDirection(Direction dir) => GetSegment(dir).Visited;

   /// <summary>
   /// Retrieves the cached segment information for the specified direction.
   /// </summary>
   /// <param name="dir">The direction to retrieve the segment for.</param>
   /// <returns>The <see cref="CacheNodeInfo"/> for the given direction.</returns>
   /// <exception cref="InvalidOperationException">Thrown if no segment exists in the specified direction.</exception>
   public CacheNodeInfo GetSegment(Direction dir)
   {
      foreach (var segment in Segments)
         if (segment.Dir == dir)
            return segment;

      throw new InvalidOperationException($"Node does not have a segment in direction {dir}");
   }

   /// <summary>
   /// Attempts to retrieve the cached segment information for the specified direction.
   /// </summary>
   /// <param name="dir">The direction to retrieve the segment for.</param>
   /// <param name="segment">The resulting <see cref="CacheNodeInfo"/> if found; otherwise, null.</param>
   /// <returns>True if the segment was found; otherwise, false.</returns>
   private bool TryGetSegment(Direction dir, [MaybeNullWhen(false)] out CacheNodeInfo segment)
   {
      foreach (var cache in Segments)
      {
         if (cache.Dir != dir)
            continue;

         segment = cache;
         return true;
      }

      segment = null;
      return false;
   }

   /// <summary>
   /// Visits the node using the given input node and segment, updating the direction and returning the next segment and node.
   /// </summary>
   /// <param name="direction">The direction to update.</param>
   /// <param name="input">The input border segment used to determine the approach direction to the node.</param>
   /// <param name="segment">The found border segment, if any.</param>
   /// <param name="node">The node the segment leads to, if any.</param>
   /// <returns>True if a segment and node were found; otherwise, false.</returns>
   public bool Visit(ref Direction direction,
                     BorderSegmentDirectional input,
                     [MaybeNullWhen(false)] out CacheNodeInfo segment,
                     [MaybeNullWhen(false)] out Node node)
   {
      if (input.Segment.Points.Count == 0)
         return Visit(ref direction, out segment, out node);

      // Get last point based on the direction
      var point = input.IsForward ? input.Segment.Points[^1] : input.Segment.Points[0];

      return Visit(ref direction, point.X, point.Y, out segment, out node);
   }

   /// <summary>
   /// Visits the node using the given node position and the last segment position, updating the direction and returning the next segment and node.
   /// </summary>
   /// <param name="direction">The direction to update.</param>
   /// <param name="x">The X coordinate of the last segment point.</param>
   /// <param name="y">The Y coordinate of the last segment point.</param>
   /// <param name="segment">The found border segment, if any.</param>
   /// <param name="node">The node the segment leads to, if any.</param>
   /// <returns>True if a segment and node were found; otherwise, false.</returns>
   /// <exception cref="InvalidOperationException">Thrown if the movement between points is invalid, so not in a straight line.</exception>
   private bool Visit(ref Direction direction,
                      int x,
                      int y,
                      [MaybeNullWhen(false)] out CacheNodeInfo segment,
                      [MaybeNullWhen(false)] out Node node)
   {
      // Get direction based of the difference between the two points
      var dx = XPos - x;
      var dy = YPos - y;

      direction = dx switch
      {
         > 0 when dy == 0 => Direction.East,
         < 0 when dy == 0 => Direction.West,
         0 when dy > 0 => Direction.South,
         0 when dy < 0 => Direction.North,
         _ => throw new InvalidOperationException($"Invalid movement from ({XPos} {YPos}) to ({x}, {y})")
      };
      return Visit(ref direction, out segment, out node);
   }

   /// <summary>
   /// Checks if a segment is cached in the given direction and returns it with the node it leads to.
   /// </summary>
   /// <param name="direction">The direction to check and update.</param>
   /// <param name="segment">The found border segment, if any.</param>
   /// <param name="node">The node the segment leads to, if any.</param>
   /// <returns>True if a segment and node were found; otherwise, false.</returns>
   private bool Visit(ref Direction direction,
                      [MaybeNullWhen(false)] out CacheNodeInfo segment,
                      [MaybeNullWhen(false)] out Node node)
   {
      // Not only check the right direction, since it is a T-shaped intersection a possible path is straight ahead.
      var newDirection = direction.RotateRight();
      if (TryGetSegment(newDirection, out var cache))
         direction = newDirection;
      else
         cache = GetSegment(direction);

      if (cache.Segment.HasValue)
      {
         segment = cache;
         node = cache.Node!;
         return true;
      }

      segment = null;
      node = null;
      return false;
   }
}