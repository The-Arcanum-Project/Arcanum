using System.Diagnostics;

// Make sure you have a using statement for your PointF and RectangleF,
// e.g., using System.Drawing;

namespace Arcanum.Core.CoreSystems.Map;

public sealed class QuadTree
{
   private const int MAX_POLYGONS = 10;
   private const int MAX_DEPTH = 15;

   public RectangleF Bounds { get; }
   public List<Polygon> Polygons { get; } = [];

   private QuadTree[]? _children;
   private readonly int _depth;

   public QuadTree(RectangleF bounds, int depth = 0)
   {
      Bounds = bounds;
      _depth = depth;
   }

   /// <summary>
   /// Inserts a polygon into the quadtree. This method remains unchanged.
   /// </summary>
   public void Insert(Polygon poly)
   {
      if (!Bounds.IntersectsWith(poly.Bounds))
         return;

      if (_children != null)
      {
         foreach (var child in _children)
            child.Insert(poly);
         return;
      }

      Polygons.Add(poly);

      if (Polygons.Count <= MAX_POLYGONS || _depth >= MAX_DEPTH)
         return;

      Subdivide();
      foreach (var p in Polygons)
      {
         Debug.Assert(_children != null, nameof(_children) + " != null");

         foreach (var child in _children)
            child.Insert(p);
      }

      Polygons.Clear();
   }

   /// <summary>
   /// Queries the quadtree for the single polygon that contains the specified point.
   /// Because polygons do not overlap, this method returns the first and only match found.
   /// </summary>
   /// <param name="point">The point to check.</param>
   /// <returns>The Polygon containing the point, or null if no polygon is found.</returns>
   public Polygon? Query(PointF point)
   {
      if (!Bounds.Contains(point))
         return null;

      if (_children != null)
         return (from child in _children
                 where child.Bounds.Contains(point)
                 select child.Query(point)).FirstOrDefault();

      return Polygons.FirstOrDefault(p => p.Contains(point));
   }

   private void Subdivide()
   {
      var halfWidth = Bounds.Width / 2;
      var halfHeight = Bounds.Height / 2;
      var x = Bounds.X;
      var y = Bounds.Y;

      _children =
      [
         new(new(x, y, halfWidth, halfHeight), _depth + 1),
         new(new(x + halfWidth, y, halfWidth, halfHeight), _depth + 1),
         new(new(x, y + halfHeight, halfWidth, halfHeight), _depth + 1),
         new(new(x + halfWidth, y + halfHeight, halfWidth, halfHeight), _depth + 1),
      ];
   }
}