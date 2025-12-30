using System.Drawing.Imaging;
using System.IO;
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
         foreach (var v in vertices)
         {
            sb.Append($"{v.X},{v.Y};");
         }

         if (poly.Holes.Count > 0)
         {
            sb.Append("HOLES:[");
            var sortedHoles = SortPolygons(poly.Holes);
            foreach (var hole in sortedHoles)
            {
               var hVerts = ExtractVertices(hole);
               foreach (var hv in hVerts)
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

      int i = 0;
      foreach (var poly in sorted)
      {
         var vertices = ExtractVertices(poly);
         writer.WriteLine($"Polygon[{i++}] Color: {poly.Color:X} Vertices: {vertices.Count}");

         writer.WriteLine(string.Join(" -> ", vertices.Select(v => $"({v.X},{v.Y})")));

         if (poly.Holes.Count > 0)
         {
            writer.WriteLine($"  Holes: {poly.Holes.Count}");
            foreach (var hole in SortPolygons(poly.Holes))
            {
               var hVerts = ExtractVertices(hole);
               writer.WriteLine("    Hole: " + string.Join(" -> ", hVerts.Select(v => $"({v.X},{v.Y})")));
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
         foreach (var hole in poly.Holes)
         {
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
                var v = ExtractVertices(p);
                return v.Count > 0 ? v[0].X : 0;
             }) // Start Point X
            .ThenBy(p =>
             {
                var v = ExtractVertices(p);
                return v.Count > 0 ? v[0].Y : 0;
             }) // Start Point Y
            .ToList();
   }

   private static List<Vector2I> ExtractVertices(PolygonParsing poly)
   {
      var result = new List<Vector2I>();

      foreach (var segment in poly.Segments)
         if (segment is Node n)
            result.Add(n.Position);
         else if (segment is BorderSegmentDirectional bsd)
         {
            if (bsd.IsForward)
               result.AddRange(bsd.Segment.Points);
            else
               for (int i = bsd.Segment.Points.Count - 1; i >= 0; i--)
                  result.Add(bsd.Segment.Points[i]);
         }

      return result;
   }
}