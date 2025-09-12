using System.Diagnostics;

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
   /// Inserts a polygon into the quadtree.
   /// If the polygon's bounds do not intersect with the quadtree's bounds, it will not be inserted.
   /// If the quadtree has reached its maximum number of polygons and depth, it will subdivide and
   /// redistribute the polygons.
   /// </summary>
   /// <param name="poly"></param>
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

      if (Polygons.Count > MAX_POLYGONS && _depth < MAX_DEPTH)
      {
         Subdivide();
         foreach (var p in Polygons)
         {
            Debug.Assert(_children != null, nameof(_children) + " != null");

            foreach (var child in _children)
               child.Insert(p);
         }

         Polygons.Clear();
      }
   }

   /// <summary>
   /// Queries the quadtree for polygons that contain the specified point.
   /// If the point is outside the bounds of the quadtree, no polygons will be returned.
   /// If the quadtree has children, it will recursively query them.
   /// If the quadtree has no children, it will check each polygon's bounds to see if it contains the point,
   /// and add it to the results if it does.
   /// </summary>
   /// <param name="point"></param>
   /// <param name="results"></param>
   public void Query(PointF point, List<Polygon> results)
   {
      if (!Bounds.Contains(point))
         return;

      if (_children != null)
      {
         foreach (var child in _children)
            if (child.Bounds.Contains(point))
            {
               child.Query(point, results);
               break;
            }
      }
      else
      {
         foreach (var p in Polygons)
            if (p.Bounds.Contains(point))
               results.Add(p);
      }
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