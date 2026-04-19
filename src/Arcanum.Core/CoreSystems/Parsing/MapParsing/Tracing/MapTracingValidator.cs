using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

public static class MapTracingValidator
{
   /// <summary>
   /// Generates a unique SHA256 hash of the entire polygon set.
   /// Use this for a quick "Pass/Fail" check.
   /// </summary>
   public static string GenerateFingerprint(List<PolygonParsing> polygons)
   {
      // Sort by: Color -> Vertex Count -> Center Point X -> Center Point Y
      var sorted = SortPolygons(polygons);

      var sb = new StringBuilder();

      foreach (var poly in sorted)
      {
         sb.Append($"C:{poly.Color}|");

         // Flatten the polygon to a list of absolute points
         var vertices = ExtractVertices(poly);
         var normalized = NormalizeVertexOrder(vertices);

         foreach (var v in normalized)
            sb.Append($"{v.X},{v.Y};");

         if (poly.Holes.Count > 0)
         {
            sb.Append("HOLES:[");
            var sortedHoles = SortPolygons(poly.Holes);
            for (var index = 0; index < sortedHoles.Count; index++)
            {
               var hole = sortedHoles[index];
               var hVerts = ExtractVertices(hole);
               var hNormalized = NormalizeVertexOrder(hVerts);
               foreach (var hv in hNormalized)
                  sb.Append($"{hv.X},{hv.Y};");
               sb.Append("|");
            }

            sb.Append("]");
         }

         sb.AppendLine();
      }

      using var sha = SHA256.Create();
      var bytes = Encoding.UTF8.GetBytes(sb.ToString());
      var hash = sha.ComputeHash(bytes);
      return Convert.ToHexString(hash);
   }

   /// <summary>
   /// Dumps the full coordinate list to a text file.
   /// Use this if the Fingerprint fails, so you can use a Diff Tool (like BeyondCompare) 
   /// to see exactly which pixel shifted.
   /// </summary>
   public static void DumpToFile(List<PolygonParsing> polygons, string filepath)
   {
      var sorted = SortPolygons(polygons);
      IO.IO.EnsureFileDirectoryExists(filepath);
      using var writer = new StreamWriter(filepath);

      var i = 0;
      foreach (var poly in sorted)
      {
         var vertices = ExtractVertices(poly);
         var normalized = NormalizeVertexOrder(vertices);

         writer.WriteLine($"Polygon[{i++}] Color: {poly.Color:X} Vertices: {normalized.Count}");
         writer.WriteLine(string.Join(" -> ", normalized.Select(v => $"({v.X},{v.Y})")));

         if (poly.Holes.Count > 0)
         {
            writer.WriteLine($"  Holes: {poly.Holes.Count}");
            var list = SortPolygons(poly.Holes);
            for (var index = 0; index < list.Count; index++)
            {
               var hole = list[index];
               var hVerts = ExtractVertices(hole);
               var hNormalized = NormalizeVertexOrder(hVerts);
               writer.WriteLine("    Hole: " + string.Join(" -> ", hNormalized.Select(v => $"({v.X},{v.Y})")));
            }
         }

         writer.WriteLine("--------------------------------------------------");
      }
   }

   /// <summary>
   /// Visual Debug: Draws the polygons to a bitmap. 
   /// If the hash differs, comparing these two images will instantly show you 
   /// if the error is a "micro-shift" or a "catastrophic explosion".
   /// </summary>
   public static void RenderDebugImage(List<PolygonParsing> polygons, int width, int height, string filepath)
   {
      using var bmp = new Bitmap(width, height);
      using var g = Graphics.FromImage(bmp);
      g.Clear(Color.Black);

      var sorted = SortPolygons(polygons);

      foreach (var poly in sorted)
      {
         var vertices = ExtractVertices(poly);
         if (vertices.Count < 2)
            continue;

         var points = vertices.Select(v => new Point(v.X, v.Y)).ToArray();

         var c = Color.FromArgb(255, Color.FromArgb(poly.Color));

         using var pen = new Pen(c, 1);
         g.DrawPolygon(pen, points);

         using var holePen = new Pen(Color.Red, 1);
         for (var index = 0; index < poly.Holes.Count; index++)
         {
            var hole = poly.Holes[index];
            var hVerts = ExtractVertices(hole);
            if (hVerts.Count < 2)
               continue;

            g.DrawPolygon(holePen, hVerts.Select(v => new Point(v.X, v.Y)).ToArray());
         }
      }

      IO.IO.EnsureFileDirectoryExists(filepath);
      bmp.Save(filepath, ImageFormat.Png);
   }

   // --- Helpers ---

   private static List<PolygonParsing> SortPolygons(IEnumerable<PolygonParsing> input)
   {
      return input
            .OrderBy(p => p.Color)
            .ThenBy(p => ExtractVertices(p).Count)
            .ThenBy(p =>
             {
                var v = NormalizeVertexOrder(ExtractVertices(p));
                return v.Count > 0 ? v[0].X : 0;
             }) // Start Point X
            .ThenBy(p =>
             {
                var v = NormalizeVertexOrder(ExtractVertices(p));
                return v.Count > 0 ? v[0].Y : 0;
             }) // Start Point Y
            .ToList();
   }

   private static List<Vector2I> ExtractVertices(PolygonParsing poly)
   {
      var result = new List<Vector2I>();

      foreach (var segment in poly.Segments)
         switch (segment)
         {
            case Node n:
               result.Add(n.Position);
               break;
            case BorderSegmentDirectional bsd when bsd.IsForward:
               result.AddRange(bsd.Segment.Points);
               break;
            case BorderSegmentDirectional bsd:
            {
               for (var i = bsd.Segment.Points.Count - 1; i >= 0; i--)
                  result.Add(bsd.Segment.Points[i]);
               break;
            }
         }

      return result;
   }

   /// <summary>
   /// Rotates the vertex list so that the lexicographically smallest point is first.
   /// This makes comparison independent of starting point.
   /// </summary>
   private static List<Vector2I> NormalizeVertexOrder(List<Vector2I> vertices)
   {
      if (vertices.Count <= 1)
         return vertices;

      // Find the index of the lexicographically smallest point
      var minIndex = 0;
      for (var i = 1; i < vertices.Count; i++)
      {
         var current = vertices[i];
         var min = vertices[minIndex];

         if (current.X < min.X || (current.X == min.X && current.Y < min.Y))
            minIndex = i;
      }

      // Rotate the list so minIndex becomes index 0
      if (minIndex == 0)
         return vertices;

      var result = new List<Vector2I>(vertices.Count);
      for (var i = minIndex; i < vertices.Count; i++)
         result.Add(vertices[i]);
      for (var i = 0; i < minIndex; i++)
         result.Add(vertices[i]);

      return result;
   }
}