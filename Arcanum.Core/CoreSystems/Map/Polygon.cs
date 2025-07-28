namespace Arcanum.Core.CoreSystems.Map;

public sealed class Polygon
{
   public List<PointF> Vertices { get; private set; }
   public List<int> TriangleIndices { get; private set; } // [0,1,2, 0,2,3,...]
   public RectangleF Bounds { get; private set; }
   public bool IsTriangulated => TriangleIndices.Count > 0;

   public Polygon(List<PointF> vertices, List<int> triangleIndices)
   {
      Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
      TriangleIndices = triangleIndices ?? throw new ArgumentNullException(nameof(triangleIndices));

      if (Vertices.Count < 3)
         throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertices));

      if (TriangleIndices.Count % 3 != 0)
         throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(triangleIndices));

      Bounds = CalculateBounds();
   }

   public Polygon(List<PointF> vertices)
      : this(vertices, Triangulate(vertices))
   {
      if (vertices.Count < 3)
         throw new ArgumentException("A polygon must have at least 3 vertices.", nameof(vertices));
   }

   public void AddVertex(PointF vertex)
   {
      Vertices.Add(vertex);
      Bounds = CalculateBounds();
      TriangleIndices = Triangulate(Vertices);
   }

   public void AddRangeOfVertices(IEnumerable<PointF> vertices)
   {
      if (vertices == null)
         throw new ArgumentNullException(nameof(vertices));

      Vertices.AddRange(vertices);
      Bounds = CalculateBounds();
      TriangleIndices = Triangulate(Vertices);
   }

   public void Clear()
   {
      Vertices.Clear();
      TriangleIndices.Clear();
      Bounds = RectangleF.Empty;
   }

   public void RemoveVertex(PointF vertex)
   {
      if (!Vertices.Remove(vertex))
         return;

      Bounds = CalculateBounds();
      TriangleIndices = Triangulate(Vertices);
   }

   public void RemoveRangeOfVertices(IEnumerable<PointF> vertices)
   {
      if (vertices == null)
         throw new ArgumentNullException(nameof(vertices));

      foreach (var vertex in vertices)
      {
         Vertices.Remove(vertex);
      }

      Bounds = CalculateBounds();
      TriangleIndices = Triangulate(Vertices);
   }

   public static List<int> Triangulate(List<PointF> vertices)
   {
      throw new NotImplementedException();
   }

   public RectangleF CalculateBounds()
   {
      if (Vertices.Count == 0)
         return RectangleF.Empty;

      float minX = Vertices[0].X,
            maxX = Vertices[0].X;
      float minY = Vertices[0].Y,
            maxY = Vertices[0].Y;

      foreach (var vertex in Vertices)
      {
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

   public bool Contains(PointF point)
   {
      if (!Bounds.Contains(point))
         return false;

      for (int i = 0; i < TriangleIndices.Count; i += 3)
      {
         var a = Vertices[TriangleIndices[i]];
         var b = Vertices[TriangleIndices[i + 1]];
         var c = Vertices[TriangleIndices[i + 2]];
         if (PointInTriangle(point, a, b, c))
            return true;
      }

      return false;
   }

   private static bool PointInTriangle(PointF p, PointF a, PointF b, PointF c)
   {
      var v0 = new PointF(c.X - a.X, c.Y - a.Y);
      var v1 = new PointF(b.X - a.X, b.Y - a.Y);
      var v2 = new PointF(p.X - a.X, p.Y - a.Y);

      var dot00 = v0.X * v0.X + v0.Y * v0.Y;
      var dot01 = v0.X * v1.X + v0.Y * v1.Y;
      var dot02 = v0.X * v2.X + v0.Y * v2.Y;
      var dot11 = v1.X * v1.X + v1.Y * v1.Y;
      var dot12 = v1.X * v2.X + v1.Y * v2.Y;

      var invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
      var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      return u >= 0 && v >= 0 && (u + v) <= 1;
   }
}