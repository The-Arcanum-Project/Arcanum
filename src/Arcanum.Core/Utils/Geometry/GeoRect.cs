using System.Numerics;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.Utils.Geometry;

public static class GeoRect
{
   private const float EPSILON = 1e-6f;

   /// <summary>
   /// Helper method for floating-point comparison.
   /// </summary>
   private static bool IsClose(float a, float b) => Math.Abs(a - b) < EPSILON;

   /// <summary>
   /// Calculates the geometric difference between two rectangles that share exactly one corner.
   /// The difference is the "L" shape area of the larger rectangle not occupied by the smaller one.
   /// </summary>
   /// <param name="rectA">The first rectangle.</param>
   /// <param name="rectB">The second rectangle.</param>
   /// <returns>A tuple containing two rectangles that represent the "L" shape difference. 
   /// One or both can be RectangleF.Empty if there is no difference in that dimension.</returns>
   /// <exception cref="ArgumentException">Thrown if the rectangles do not share exactly one corner.</exception>
   public static (RectangleF rect1, RectangleF rect2) RectDiff(RectangleF rectA, RectangleF rectB)
   {
      if (rectA.Equals(rectB))
         return (RectangleF.Empty, RectangleF.Empty);

      // Check if they share at least one corner. 
      var sharesTopLeft = IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Top, rectB.Top);
      var sharesTopRight = IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Top, rectB.Top);
      var sharesBottomLeft = IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Bottom, rectB.Bottom);
      var sharesBottomRight = IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Bottom, rectB.Bottom);

      if (!sharesTopLeft && !sharesTopRight && !sharesBottomLeft && !sharesBottomRight)
         throw new ArgumentException("Rectangles must share at least one corner to calculate a difference.");

      var unionRect = RectangleF.Union(rectA, rectB);
      var intersectRect = RectangleF.Intersect(rectA, rectB);

      // If one rectangle is fully contained within the other but they don't share a corner,
      // Intersect will equal the smaller rect.
      if (intersectRect.IsEmpty)
         // This case shouldn't be hit if the corner check passes.
         return (rectA, rectB);

      var rect1 = RectangleF.Empty;
      var rect2 = RectangleF.Empty;

      // The horizontal bar of the "L".
      // Its width is the full union width, and its height is the difference.
      var horizontalBarHeight = unionRect.Height - intersectRect.Height;
      if (horizontalBarHeight > EPSILON)
      {
         // If the intersection is at the top of the union, the bar is at the bottom.
         var y = IsClose(unionRect.Top, intersectRect.Top)
                    ? intersectRect.Bottom
                    : unionRect.Top;

         rect1 = unionRect with { Y = y, Height = horizontalBarHeight };
      }

      // The vertical bar of the "L".
      // Its height is the height of the *intersection*, and its width is the difference.
      var verticalBarWidth = unionRect.Width - intersectRect.Width;
      if (!(verticalBarWidth > EPSILON))
         return (rect1, rect2);

      // If the intersection is on the left of the union, the bar is on the right.
      var x = IsClose(unionRect.Left, intersectRect.Left)
                 ? intersectRect.Right
                 : unionRect.Left;

      rect2 = intersectRect with { X = x, Width = verticalBarWidth };

      return (rect1, rect2);
   }

   public static bool IsRectangleContained(RectangleF outer, RectangleF inner)
   {
      return outer.X <= inner.X &&
             outer.Y <= inner.Y &&
             outer.Right >= inner.Right &&
             outer.Bottom >= inner.Bottom;
   }

   public static List<Location> GetLocationsOnLine(Vector2 start, Vector2 end, List<Location> locations)
   {
      var result = new List<Location>();
      var lineRect = new RectangleF(Math.Min(start.X, end.X),
                                    Math.Min(start.Y, end.Y),
                                    Math.Abs(end.X - start.X),
                                    Math.Abs(end.Y - start.Y));

      foreach (var loc in locations)
         if (loc.Bounds.IntersectsWith(lineRect))
            if (loc.Polygons.Any(p => LineIntersectsPolygon(start, end, p)))
               result.Add(loc);

      return result;
   }

   private static bool LineIntersectsPolygon(Vector2 start, Vector2 end, Polygon polygon)
   {
      if (LineIntersectsRectangle(start, end, polygon.Bounds))
         return false;

      var vertices = polygon.Vertices;
      var count = vertices.Length;

      for (var i = 0; i < count - 1; i++)
         if (LinesIntersect(start, end, vertices[i], vertices[i + 1]))
            return true;

      return LinesIntersect(start, end, vertices[count - 1], vertices[0]);
   }

   public static bool LineIntersectsRectangle(Vector2 start, Vector2 end, RectangleF bounds)
   {
      return MathF.Max(start.X, end.X) < bounds.Left ||
             MathF.Min(start.X, end.X) > bounds.Right ||
             MathF.Max(start.Y, end.Y) < bounds.Top ||
             MathF.Min(start.Y, end.Y) > bounds.Bottom;
   }

   // manual implementation of Contains for Vector2
   public static bool ContainsVec2(this RectangleF polygon, Vector2 point)
   {
      return polygon.X <= point.X &&
             polygon.Y <= point.Y &&
             polygon.Right >= point.X &&
             polygon.Bottom >= point.Y;
   }

   public static List<Location> GetLocationsInPolygon(Polygon polygon, List<Location> locations)
   {
      // Get all polygons which bounds are contained or intersected by the given polygon
      // for intersected Locations we check if any of the sub polygons are contained if not do not add.
      var result = new List<Location>();

      foreach (var loc in locations)
      {
         if (polygon.Contains(loc.Bounds))
         {
            result.Add(loc);
            continue;
         }

         if (!loc.Polygons.Any(polygon.Intersects))
            continue;

         if (loc.Polygons.Any(polygon.Contains))
            result.Add(loc);
      }

      return result;
   }

   private static bool LinesIntersect_WithBoundingBoxCheck(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
   {
      if (MathF.Max(p1.X, p2.X) < MathF.Min(p3.X, p4.X) ||
          MathF.Max(p3.X, p4.X) < MathF.Min(p1.X, p2.X) ||
          MathF.Max(p1.Y, p2.Y) < MathF.Min(p3.Y, p4.Y) ||
          MathF.Max(p3.Y, p4.Y) < MathF.Min(p1.Y, p2.Y))
         return false;

      return LinesIntersect(p1, p2, p3, p4);
   }

   private static bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
   {
      var d = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
      if (MathF.Abs(d) < 1e-6f)
         return false;

      var uA = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / d;
      var uB = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / d;

      return uA is >= 0 and <= 1 && uB is >= 0 and <= 1;
   }
}