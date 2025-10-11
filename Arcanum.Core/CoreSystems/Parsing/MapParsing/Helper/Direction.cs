using System.Runtime.CompilerServices;
namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

public enum Direction
{
    North,
    East,
    South,
    West,
}

public static class DirectionHelper
{
    // Slightly overengineered in terms of performance
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction RotateRight(this Direction d)
    {
        return (Direction)(((int)d + 1) & 3);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction RotateLeft(this Direction d)
    {
        return (Direction)(((int)d - 1) & 3);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Direction Invert(this Direction d)
    {
        return (Direction)(((int)d + 2) & 3);
    }
    
    
    // Can maybe be optimized further, but this is fine for now
    public static (bool, bool) GetDeltaMove(this Direction d)
    {
        return d switch
        {
            Direction.North => (false, false),
            Direction.East => (true, true),
            Direction.South => (false, true),
            Direction.West => (true, false),
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };
    }
    
    /// <summary>
    /// Given the current position in Grid Coordinates and a direction,
    /// calculates the pixel coordinates to the left and right of the current position.
    /// </summary>
    public static (int, int, int, int) GetStartPos(int xGrid, int yGrid, Direction d)
    {
        return d switch
        {
            Direction.North => (xGrid - 1, yGrid - 1, xGrid, yGrid - 1),
            Direction.East => (xGrid, yGrid - 1, xGrid, yGrid),
            Direction.South => (xGrid, yGrid, xGrid - 1, yGrid),
            Direction.West => (xGrid - 1, yGrid, xGrid - 1, yGrid - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };
    }
}