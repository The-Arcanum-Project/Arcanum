using System.Numerics;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.GameObjects.LocationCollections;

namespace Arcanum.Core.Utils.Geometry;

public static class GeoRect
{
   public static (RectangleF rect1, RectangleF rect2) RectDiff(RectangleF rectA, RectangleF rectB)
   {
      // Ensure they are aligned.
      const float epsilon = 1e-6f;
      if (Math.Abs(rectA.X - rectB.X) > epsilon || Math.Abs(rectA.Y - rectB.Y) > epsilon)
         throw new ArgumentException("Rectangles must share the same top-left corner.");

      if (Math.Abs(rectA.Width - rectB.Width) < epsilon && Math.Abs(rectA.Height - rectB.Height) < epsilon)
         return (RectangleF.Empty, RectangleF.Empty);

      // Find the dimensions of the intersection (the smaller of the two)
      var minWidth = Math.Min(rectA.Width, rectB.Width);
      var minHeight = Math.Min(rectA.Height, rectB.Height);

      // Find the dimensions of the union (the larger of the two)
      var maxWidth = Math.Max(rectA.Width, rectB.Width);
      var maxHeight = Math.Max(rectA.Height, rectB.Height);

      // The non-intersecting area is the "L" shape formed by (Union - Intersection).

      var rect1 = RectangleF.Empty;
      var rect2 = RectangleF.Empty;

      // The horizontal bar of the "L" shape.
      // This is the area at the bottom of the union rectangle.
      var horizontalBarHeight = maxHeight - minHeight;
      if (horizontalBarHeight > epsilon)
         rect1 = new(rectA.X,
                     rectA.Y + minHeight,
                     maxWidth,
                     horizontalBarHeight);

      // The vertical bar of the "L" shape.
      // This is the area at the right side of the union, but only as tall as the intersection.
      var verticalBarWidth = maxWidth - minWidth;
      if (verticalBarWidth > epsilon)
         rect2 = new(rectA.X + minWidth,
                     rectA.Y,
                     verticalBarWidth,
                     minHeight);

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