using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Arcanum.Core.Utils.Geometry;

namespace Arcanum.Core.CoreSystems.Map;

public sealed class Polygon
{
   public Vector2[] Vertices { get; }
   public int[] TriangleIndices { get; } // [0,1,2, 0,2,3,...]
   public RectangleF Bounds { get; }

   public int ColorIndex;

   public Polygon(Vector2[] vertices, int[] triangleIndices)
   {
      Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
      TriangleIndices = triangleIndices ?? throw new ArgumentNullException(nameof(triangleIndices));
      this.ColorIndex = ColorIndex;

#if DEBUG
      if (Vertices.Length < 3)
         throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertices));

      if (TriangleIndices.Length % 3 != 0)
         throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(triangleIndices));
#endif

      Bounds = CalculateBounds_SIMD();
   }

   private RectangleF CalculateBounds()
   {
      if (Vertices.Length == 0)
         return RectangleF.Empty;

      float minX = Vertices[0].X,
            maxX = Vertices[0].X;
      float minY = Vertices[0].Y,
            maxY = Vertices[0].Y;

      for (var i = 1; i < Vertices.Length; i++)
      {
         var vertex = Vertices[i];
         if (vertex.X < minX)
            minX = vertex.X;
         if (vertex.X > maxX)
            maxX = vertex.X;
         if (vertex.Y < minY)
            minY = vertex.Y;
         if (vertex.Y > maxY)
            maxY = vertex.Y;
      }

      return new(minX, minY, maxX - minX, maxY - minY);
   }

   private RectangleF CalculateBounds_SIMD()
   {
      if (Vertices.Length == 0)
         return RectangleF.Empty;

      // Get the number of Vector2s we can process at once.
      // Vector<float>.Count is the number of floats in a SIMD register (e.g., 4 or 8).
      // So we process (Vector<float>.Count / 2) Vector2s at a time.
      var vectorSize = Vector<float>.Count;
      var vector2Count = vectorSize / 2;

      var minValues = new Vector<float>(float.PositiveInfinity);
      var maxValues = new Vector<float>(float.NegativeInfinity);

      var i = 0;
      // Process the main part of the array in large, vectorized chunks
      for (; i <= Vertices.Length - vector2Count; i += vector2Count)
      {
         // Load a chunk of vertices into a SIMD vector.
         // The data layout needs to be [X1, Y1, X2, Y2, X3, Y3, X4, Y4] for this to work.
         // We can achieve this with a Span and MemoryMarshal.
         var span = new Span<Vector2>(Vertices, i, vector2Count);
         var vector = MemoryMarshal.Cast<Vector2, float>(span);

         minValues = Vector.Min(minValues, new(vector));
         maxValues = Vector.Max(maxValues, new(vector));
      }

      // Process the remaining elements that didn't fit into a full vector chunk
      float minX = float.PositiveInfinity,
            maxX = float.NegativeInfinity;
      float minY = float.PositiveInfinity,
            maxY = float.NegativeInfinity;

      for (; i < Vertices.Length; i++)
      {
         var vertex = Vertices[i];
         if (vertex.X < minX)
            minX = vertex.X;
         if (vertex.X > maxX)
            maxX = vertex.X;
         if (vertex.Y < minY)
            minY = vertex.Y;
         if (vertex.Y > maxY)
            maxY = vertex.Y;
      }

      // Reduce the SIMD vectors to single min/max values
      // minValues vector might look like [minX1, minY1, minX2, minY2]
      // We need to find the minimum of all X's and all Y's.
      for (var j = 0; j < vectorSize; j++)
         if (j % 2 == 0) // Even indices are X
         {
            if (minValues[j] < minX)
               minX = minValues[j];
            if (maxValues[j] > maxX)
               maxX = maxValues[j];
         }
         else // Odd indices are Y
         {
            if (minValues[j] < minY)
               minY = minValues[j];
            if (maxValues[j] > maxY)
               maxY = maxValues[j];
         }

      return new(minX, minY, maxX - minX, maxY - minY);
   }

   public bool Contains(Vector2 point)
   {
      if (!Bounds.ContainsVec2(point))
         return false;

      for (var i = 0; i < TriangleIndices.Length; i += 3)
      {
         var a = Vertices[TriangleIndices[i]];
         var b = Vertices[TriangleIndices[i + 1]];
         var c = Vertices[TriangleIndices[i + 2]];
         if (PointInTriangle(point, a, b, c))
            return true;
      }

      return false;
   }

   public bool Contains(RectangleF rect)
   {
      if (!Bounds.Contains(rect))
         return false;

      var corners = new[]
      {
         new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom),
         new Vector2(rect.Left, rect.Bottom),
      };

      foreach (var corner in corners)
         if (!Contains(corner))
            return false;

      return true;
   }

   public bool Intersects(Polygon b)
   {
      if (!Bounds.IntersectsWith(b.Bounds))
         return false;

      foreach (var vertex in Vertices)
         if (b.Contains(vertex))
            return true;

      foreach (var vertex in b.Vertices)
         if (Contains(vertex))
            return true;

      return false;
   }

   public bool Intersects(IEnumerable<Polygon> others)
   {
      foreach (var other in others)
         if (Intersects(other))
            return true;

      return false;
   }

   public bool Contains(Polygon other)
   {
      foreach (var vertex in other.Vertices)
         if (!Contains(vertex))
            return false;

      return true;
   }

   private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
   {
      var v0 = new Vector2(c.X - a.X, c.Y - a.Y);
      var v1 = new Vector2(b.X - a.X, b.Y - a.Y);
      var v2 = new Vector2(p.X - a.X, p.Y - a.Y);

      var dot00 = v0.X * v0.X + v0.Y * v0.Y;
      var dot01 = v0.X * v1.X + v0.Y * v1.Y;
      var dot02 = v0.X * v2.X + v0.Y * v2.Y;
      var dot11 = v1.X * v1.X + v1.Y * v1.Y;
      var dot12 = v1.X * v2.X + v1.Y * v2.Y;

      var invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
      var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      return u >= 0 && v >= 0 && u + v <= 1;
   }
}