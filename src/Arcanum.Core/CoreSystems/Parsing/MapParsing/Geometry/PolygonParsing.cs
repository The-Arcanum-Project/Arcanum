using System.Numerics;
using Arcanum.Core.CoreSystems.Map;
using LibTessDotNet;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;

public class PolygonParsing(int color)
{
   public readonly int Color = color;
   public List<ICoordinateAdder> Segments { get; } = [];
   public List<PolygonParsing> Holes { get; } = [];

   private List<Vector2I> GetAllPoints()
   {
      var points = new List<Vector2I>();
      foreach (var segment in Segments)
         segment.AddTo(points);

      return points;
   }

   public Polygon Tessellate()
   {
      var tess = new Tess();
      var points = GetAllPoints();
      var contour = new ContourVertex[points.Count];

      for (var i = 0; i < points.Count; i++)
         contour[i] = new(new(points[i].X, points[i].Y, 0));

      tess.AddContour(contour);
      if (Holes.Count > 0)
         foreach (var hole in Holes)
         {
            var holePoints = hole.GetAllPoints();
            var holeContour = new ContourVertex[holePoints.Count];

            for (var i = 0; i < holePoints.Count; i++)
               holeContour[i] = new(new(holePoints[i].X, holePoints[i].Y, 0));

            tess.AddContour(holeContour);
         }

      tess.Tessellate(normal: new(0, 0, 1));

      if (tess.VertexCount == 0 || tess.ElementCount == 0)
      {
         ArcLog.WriteLine("MPS", LogLevel.WRN, "Tessellation resulted in zero vertices or elements.");
         return null!;
      }

      var vertices = new Vector2[tess.VertexCount];

      for (var i = 0; i < tess.VertexCount; i++)
      {
         var pos = tess.Vertices[i].Position;
         vertices[i] = new(pos.X, pos.Y);
      }

      return new(vertices, tess.Elements);
   }

   public Rectangle GetBoundingBox()
   {
      var points = GetAllPoints();
      if (points.Count == 0)
         return new(0, 0, 0, 0);

      var minX = points[0].X;
      var maxX = points[0].X;
      var minY = points[0].Y;
      var maxY = points[0].Y;

      foreach (var point in points)
      {
         if (point.X < minX)
            minX = point.X;
         if (point.X > maxX)
            maxX = point.X;
         if (point.Y < minY)
            minY = point.Y;
         if (point.Y > maxY)
            maxY = point.Y;
      }

      return new(minX, minY, maxX - minX, maxY - minY);
   }

   private static bool IsOnBorder(List<Vector2I> points, Vector2I point)
   {
      if (points.Count == 0)
         return false;

      var p1 = points[^1];

      // ReSharper disable once ForCanBeConvertedToForeach
      for (var i = 0; i < points.Count; i++)
      {
         var p2 = points[i];

         // Vertical Segment Check
         if (point.X == p1.X && point.X == p2.X)
         {
            var minY = p1.Y < p2.Y ? p1.Y : p2.Y;
            var maxY = p1.Y > p2.Y ? p1.Y : p2.Y;

            if (point.Y >= minY && point.Y <= maxY)
               return true;
         }
         // Horizontal Segment Check
         else if (point.Y == p1.Y && point.Y == p2.Y)
         {
            var minX = p1.X < p2.X ? p1.X : p2.X;
            var maxX = p1.X > p2.X ? p1.X : p2.X;

            if (point.X >= minX && point.X <= maxX)
               return true;
         }

         p1 = p2;
      }

      return false;
   }

   public bool IsOnBorder(Vector2I point)
   {
      if (IsOnBorder(GetAllPoints(), point))
         return true;

      // ReSharper disable once ForCanBeConvertedToForeach
      // ReSharper disable once LoopCanBeConvertedToQuery
      for (var index = 0; index < Holes.Count; index++)
      {
         var hole = Holes[index];
         if (IsOnBorder(hole.GetAllPoints(), point))
            return true;
      }

      return false;
   }
}

public interface ICoordinateAdder
{
   public void AddTo(List<Vector2I> points);
}