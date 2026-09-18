using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

/// <summary>
/// Lightweight struct holding segment cache data. Stored inline in Node.
/// </summary>
public struct SegmentCache
{
   public BorderSegmentDirectional? Segment;
   public Node? Node;
}

[InlineArray(4)]
public struct SegmentCacheBuffer
{
   private SegmentCache _element0;
}

public sealed class Node : ICoordinateAdder
{
#if DEBUG
   private static int _totalNodes;
   private readonly int _nodeId;

   public override string ToString() => $"Node {_nodeId} at ({XPos}, {YPos})";

#endif

   public readonly Vector2I Position;

   public int XPos
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Position.X;
   }

   public int YPos
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Position.Y;
   }

   private SegmentCacheBuffer _segments;

   /// <summary>
   /// Bitmask indicating which directions are valid exits. Bit i corresponds to Direction i.
   /// </summary>
   private byte _activeMask;

   /// <summary>
   /// Bitmask indicating which directions have been visited during polygon tracing.
   /// </summary>
   private byte _visitedMask;

   private Node(Vector2I position)
   {
#if DEBUG
      _nodeId = _totalNodes++;
#endif
      Position = position;
   }

   private Node(int x, int y) : this(new(x, y))
   {
   }

   /// <summary>
   /// Creates a border node with exits in left, forward, and right directions.
   /// The right direction is pre-marked as visited (coming from a previous border segment).
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Node CreateBorderNode(int x, int y, Direction forwardDir)
   {
      var node = new Node(x, y);
      var left = forwardDir.RotateLeft();
      var right = forwardDir.RotateRight();

      node._activeMask = (byte)((1 << (int)left) | (1 << (int)forwardDir) | (1 << (int)right));
      node._visitedMask = (byte)(1 << (int)right);
      // Visited flags are set during TraceFromNode, not at creation
      return node;
   }

   /// <summary>
   /// Creates a 3-way T-junction node with exits in left, forward, and right directions.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Node CreateThreeWayNode(int xPos, int yPos, Direction primaryDir)
   {
      var node = new Node(xPos, yPos)
      {
         _activeMask = (byte)((1 << (int)primaryDir.RotateLeft()) |
                             (1 << (int)primaryDir) |
                             (1 << (int)primaryDir.RotateRight())),
      };
      return node;
   }

   /// <summary>
   /// Creates a 4-way crossroads node with exits in all directions.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Node CreateFourWayNode(int xPos, int yPos) => new(xPos, yPos) { _activeMask = 0b1111, };

   /// <summary>
   /// Creates a 1-way node (single exit), used for closed loops.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Node CreateOneWayNode(int xPos, int yPos, Direction dir)
   {
      var node = new Node(xPos, yPos) { _activeMask = (byte)(1 << (int)dir), };
      return node;
   }

   /// <summary>
   /// Adds the node's position as a <see cref="Vector2I"/> to the provided list.
   /// </summary>
   /// <param name="points">The list to add the point to.</param>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void AddTo(List<Vector2I> points) => points.Add(Position);

   /// <summary>
   /// Marks the segment in the specified direction as visited.
   /// </summary>
   /// <param name="dir">The direction of the segment to mark as visited.</param>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetVisited(Direction dir) => _visitedMask |= (byte)(1 << (int)dir);

   /// <summary>
   /// Checks if the segment in the specified direction has been visited.
   /// </summary>
   /// <param name="dir">The direction to check.</param>
   /// <returns>True if the segment has been visited; otherwise, false.</returns>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool IsVisited(Direction dir) => (_visitedMask & (1 << (int)dir)) != 0;

   /// <summary>
   /// Checks if the node has a segment in the specified direction.
   /// </summary>
   /// <param name="dir">The direction to check.</param>
   /// <returns>True if the node has a segment in the direction</returns>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool HasDirection(Direction dir) => (_activeMask & (1 << (int)dir)) != 0;

   /// <summary>
   /// Gets a reference to the segment cache for a direction. O(1) indexed access.
   /// </summary>
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ref SegmentCache GetSegmentRef(Direction dir) => ref _segments[(int)dir];

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private Direction GetDirectionFrom(Vector2I point)
   {
      var dx = Position.X - point.X;
      var dy = Position.Y - point.Y;

      return (dx, dy) switch
      {
         (> 0, 0) => Direction.East,
         (< 0, 0) => Direction.West,
         (0, > 0) => Direction.South,
         (0, < 0) => Direction.North,
         _ => throw new InvalidOperationException($"Invalid movement from ({Position.X} {Position.Y}) to ({point.X}, {point.Y})"),
      };
   }

   /// <summary>
   /// Visits the node using the given input node and segment, updating the direction and returning the next segment and node.
   /// </summary>
   /// <param name="direction">The direction to update.</param>
   /// <param name="input">The input border segment used to determine the approach direction to the node.</param>
   /// <param name="segment">The found border segment, if any.</param>
   /// <param name="nextNode">The node the segment leads to, if any.</param>
   /// <returns>True if a segment and node were found; otherwise, false.</returns>
   public bool Visit(
      ref Direction direction,
      BorderSegmentDirectional input,
      out BorderSegmentDirectional segment,
      [MaybeNullWhen(false)] out Node nextNode)
   {
      // Step 1: Calculate the arrival direction from input
      if (input.Segment.Points.Count > 0)
      {
         var point = input.IsForward ? input.Segment.Points[^1] : input.Segment.Points[0];
         direction = GetDirectionFrom(point);
      }

      // Step 2: Determine the exit direction (right turn priority)
      var rightDir = direction.RotateRight();
      var outDir = HasDirection(rightDir) ? rightDir : direction;
      direction = outDir;

      // Step 3: Return the cached segment if exists
      ref var cache = ref _segments[(int)outDir];
      if (cache.Segment.HasValue)
      {
         segment = cache.Segment.Value;
         nextNode = cache.Node!;
         return true;
      }

      segment = default;
      nextNode = null;
      return false;
   }

   public int ActiveDirectionCount
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => System.Numerics.BitOperations.PopCount(_activeMask);
   }
}