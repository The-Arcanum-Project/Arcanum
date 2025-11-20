using System.Globalization;
using System.Numerics;
using System.Text;
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

   public record struct RectangleDiff(RectangleF Rect1, bool Rect1Added, RectangleF Rect2, bool Rect2Added);

   /// <summary>
   /// Calculates the geometric difference between two rectangles that share exactly one corner.
   /// The difference is the "L" shape area of the larger rectangle not occupied by the smaller one.
   /// </summary>
   /// <param name="rectA">The first rectangle.</param>
   /// <param name="rectB">The second rectangle.</param>
   /// <returns>A tuple containing two rectangles that represent the "L" shape difference. 
   /// One or both can be RectangleF.Empty if there is no difference in that dimension.</returns>
   /// <exception cref="ArgumentException">Thrown if the rectangles do not share exactly one corner.</exception>
   public static RectangleDiff RectDiff(RectangleF rectA, RectangleF rectB)
   {
      if (rectA.Equals(rectB))
         return new(RectangleF.Empty, false, RectangleF.Empty, false);

      var unionRect = RectangleF.Union(rectA, rectB);
      var intersectRect = RectangleF.Intersect(rectA, rectB);

      // If one rectangle is fully contained within the other, but they don't share a corner,
      // Intersect will equal the smaller rect.
      if (intersectRect.IsEmpty)
         // If the rectangles differ, remove the old one and add the new one.
         return new(rectA, false, rectB, true);

#if DEBUG
      var sharesTopLeft = IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Top, rectB.Top);
      var sharesTopRight = IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Top, rectB.Top);
      var sharesBottomLeft = IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Bottom, rectB.Bottom);
      var sharesBottomRight = IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Bottom, rectB.Bottom);

      if (!sharesTopLeft && !sharesTopRight && !sharesBottomLeft && !sharesBottomRight)
         throw new ArgumentException("Rectangles must share at least one corner to calculate a difference.");
#endif

      var verticalIncrease = rectB.Height > rectA.Height;
      var horizontalIncrease = rectB.Width > rectA.Width;
      var isL = verticalIncrease == horizontalIncrease;

      RectangleF verticalRect;
      RectangleF horizontalRect;

      var verticalWidth = unionRect.Width - intersectRect.Width;
      var horizontalHeight = unionRect.Height - intersectRect.Height;

      if (IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Top, rectB.Top))
      {
         //Shares top left
         if (isL)
            verticalRect = unionRect with { Width = verticalWidth, X = intersectRect.Right };
         else
            verticalRect = intersectRect with { Width = verticalWidth, X = intersectRect.Right };

         horizontalRect = intersectRect with { Height = horizontalHeight, Y = intersectRect.Bottom };
      }
      else if (IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Top, rectB.Top))
      {
         //Shares top right
         if (isL)
            verticalRect = unionRect with { Width = verticalWidth, X = unionRect.Left };
         else
            verticalRect = intersectRect with { Width = verticalWidth, X = unionRect.Left };

         horizontalRect = intersectRect with { Height = horizontalHeight, Y = intersectRect.Bottom };
      }
      else if (IsClose(rectA.Left, rectB.Left) && IsClose(rectA.Bottom, rectB.Bottom))
      {
         //Shares bottom left
         if (isL)
            verticalRect = unionRect with { Width = verticalWidth, X = intersectRect.Right };
         else
            verticalRect = intersectRect with { Width = verticalWidth, X = intersectRect.Right };

         horizontalRect = intersectRect with { Height = horizontalHeight, Y = intersectRect.Top - horizontalHeight };
      }
      else if (IsClose(rectA.Right, rectB.Right) && IsClose(rectA.Bottom, rectB.Bottom))
      {
         //Shares bottom right
         if (isL)
            verticalRect = unionRect with { Width = verticalWidth, X = unionRect.Left };
         else
            verticalRect = intersectRect with { Width = verticalWidth, X = unionRect.Left };

         horizontalRect = intersectRect with { Height = horizontalHeight, Y = intersectRect.Top - horizontalHeight };
      }
      else
      {
         throw new ArgumentException("Rectangles must share at least one corner to calculate a difference.");
      }

      return new(verticalRect,
                 horizontalIncrease,
                 horizontalRect,
                 verticalIncrease);

      // var debugRects = new List<RectangleF>(4);
      // debugRects.Add(rectA);
      // debugRects.Add(rectB);
      // debugRects.Add(verticalRect);
      // debugRects.Add(horizontalRect);
      //
      // string svgOutput = GenerateSvgForDebugging(debugRects);
      //
      // // You can now print this to the console, a log file, or view it in the debugger
      // Debug.WriteLine(svgOutput);

      // Helper function to generate the SVG
#pragma warning disable CS8321 // Local function is declared but never used
      string GenerateSvgForDebugging(List<RectangleF> rectangles)
#pragma warning restore CS8321 // Local function is declared but never used
      {
         if (rectangles == null! || rectangles.Count == 0)
            return "<svg></svg>";

         // Calculate total bounds to define canvas
         var minX = rectangles.Min(r => r.Left);
         var minY = rectangles.Min(r => r.Top);
         var maxX = rectangles.Max(r => r.Right);
         var maxY = rectangles.Max(r => r.Bottom);

         var width = maxX - minX + 20; // padding
         var height = maxY - minY + 20;

         var sb = new StringBuilder();
         var fmt = CultureInfo.InvariantCulture; // always use '.' for decimals

         sb.AppendLine(string.Format(fmt,
                                     "<svg width='{0:F4}' height='{1:F4}' xmlns='http://www.w3.org/2000/svg' viewBox='{2:F4} {3:F4} {4:F4} {5:F4}'>",
                                     width,
                                     height,
                                     minX - 10,
                                     minY - 10,
                                     width,
                                     height));

         // Define styles
         sb.AppendLine("<defs><style>");
         sb.AppendLine(".rectA { fill: blue; fill-opacity: 0.3; stroke: blue; }");
         sb.AppendLine(".rectB { fill: green; fill-opacity: 0.3; stroke: green; }");
         sb.AppendLine(".diff  { fill: red; fill-opacity: 0.5; stroke: red; }");
         sb.AppendLine("</style></defs>");

         // Draw the first two rectangles (A, B)
         sb.AppendLine(string.Format(fmt,
                                     "<rect x='{0:F4}' y='{1:F4}' width='{2:F4}' height='{3:F4}' class='rectA' />",
                                     rectangles[0].X,
                                     rectangles[0].Y,
                                     rectangles[0].Width,
                                     rectangles[0].Height));

         if (rectangles.Count > 1)
            sb.AppendLine(string.Format(fmt,
                                        "<rect x='{0:F4}' y='{1:F4}' width='{2:F4}' height='{3:F4}' class='rectB' />",
                                        rectangles[1].X,
                                        rectangles[1].Y,
                                        rectangles[1].Width,
                                        rectangles[1].Height));

         // Draw diff rectangles if present
         for (int i = 2; i < rectangles.Count; i++)
            sb.AppendLine(string.Format(fmt,
                                        "<rect x='{0:F4}' y='{1:F4}' width='{2:F4}' height='{3:F4}' class='diff' />",
                                        rectangles[i].X,
                                        rectangles[i].Y,
                                        rectangles[i].Width,
                                        rectangles[i].Height));

         sb.AppendLine("</svg>");
         return sb.ToString();
      }
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

   // ReSharper disable once UnusedMember.Local
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