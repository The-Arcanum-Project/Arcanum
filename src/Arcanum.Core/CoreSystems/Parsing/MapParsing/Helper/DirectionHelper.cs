using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

public static class DirectionHelper
{
   public struct PointSet(int xl, int yl, int xr, int yr, int xpos, int ypos)
   {
      public int Xl = xl;
      public int Yl = yl;
      public int Xr = xr;
      public int Yr = yr;
      public int Xpos = xpos;
      public int Ypos = ypos;

      public override string ToString()
      {
         return $"L:({Xl},{Yl}) R:({Xr},{Yr}) P:({Xpos},{Ypos})";
      }
   }

   public static Vector2I GetPosition(this PointSet ps)
   {
      return new(ps.Xpos, ps.Ypos);
   }

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

   public static Direction Invert(this Direction d)
   {
      return (Direction)(((int)d + 2) & 3);
   }

   public static void Move(this Direction d, ref PointSet ps, out int cachePos, out bool xaxis)
   {
      switch (d)
      {
         case Direction.North:
            cachePos = ps.Yl;
            ps.Yl--;
            ps.Yr--;
            ps.Ypos--;
            xaxis = false;
            break;
         case Direction.East:
            cachePos = ps.Xl;
            ps.Xl++;
            ps.Xr++;
            ps.Xpos++;
            xaxis = true;
            break;
         case Direction.South:
            cachePos = ps.Yl;
            ps.Yl++;
            ps.Yr++;
            ps.Ypos++;
            xaxis = false;
            break;
         case Direction.West:
            cachePos = ps.Xl;
            ps.Xl--;
            ps.Xr--;
            ps.Xpos--;
            xaxis = true;
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(d), d, null);
      }
   }

   /// <summary>
   /// Given the current position in Grid Coordinates and a direction,
   /// calculates the pixel coordinates to the left and right of the current position.
   /// </summary>
   public static PointSet GetStartPos(int xGrid, int yGrid, Direction d)
   {
      return d switch
      {
         Direction.North => new(xGrid - 1, yGrid - 1, xGrid, yGrid - 1, xGrid, yGrid),
         Direction.East => new(xGrid, yGrid - 1, xGrid, yGrid, xGrid, yGrid),
         Direction.South => new(xGrid, yGrid, xGrid - 1, yGrid, xGrid, yGrid),
         Direction.West => new(xGrid - 1, yGrid, xGrid - 1, yGrid - 1, xGrid, yGrid),
         _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
      };
   }

   public static (int, int) GetRightPixel(int startNodeXPos, int startNodeYPos, Direction currentDirection)
   {
      return currentDirection switch
      {
         Direction.North => (startNodeXPos, startNodeYPos - 1),
         Direction.East => (startNodeXPos, startNodeYPos),
         Direction.South => (startNodeXPos - 1, startNodeYPos),
         Direction.West => (startNodeXPos - 1, startNodeYPos - 1),
         _ => throw new ArgumentOutOfRangeException(nameof(currentDirection), currentDirection, null)
      };
   }
}