using System.Diagnostics;
using System.Numerics;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Geometry;

// Assumes the existence of the PolygonReference struct defined above.

namespace Arcanum.Core.CoreSystems.Map;

public sealed class QuadTree
{
   private const int MAX_OBJECTS = 10;
   private const int MAX_DEPTH = 15;

   public RectangleF Bounds { get; }

   private readonly Location[] _allLocations;
   private List<PolygonReference>? _references;
   private QuadTree[]? _children;
   private readonly int _depth;

   public QuadTree(RectangleF bounds, Location[] allLocations, int depth = 0)
   {
      Bounds = bounds;
      _allLocations = allLocations;
      _depth = depth;
      _children = null;
      _references = null;
   }

   /// <summary>
   /// Inserts all polygons from a given Location into the quadtree.
   /// </summary>
   public void Insert(Location location)
   {
      var locationId = location.ColorIndex;

      if (locationId < 0 || locationId >= _allLocations.Length)
         throw new ArgumentException("Location has an invalid ID for insertion.", nameof(location));

      for (var i = 0; i < location.Polygons.Length; i++)
      {
         var polyBounds = location.Polygons[i].Bounds;
         var polyRef = new PolygonReference(locationId, i);
         InsertInternal(polyRef, polyBounds);
      }
   }

   private void InsertInternal(PolygonReference polyRef, RectangleF polyBounds)
   {
      if (!Bounds.IntersectsWith(polyBounds))
         return;

      if (_children != null)
      {
         foreach (var child in _children)
            child.InsertInternal(polyRef, polyBounds);
         return;
      }

      _references ??= [];
      _references.Add(polyRef);

      if (_references.Count <= MAX_OBJECTS || _depth >= MAX_DEPTH)
         return;

      Subdivide();

      Debug.Assert(_children != null, "Subdivision failed to create child nodes.");

      foreach (var pRef in _references)
         foreach (var child in _children)
            child.InsertInternal(pRef, _allLocations[pRef.LocationId].Polygons[pRef.PolygonIndex].Bounds);

      _references = null;
   }

   /// <summary>
   /// Queries the quadtree for the single Location that contains the specified point.
   /// </summary>
   /// <param name="point">The point to check.</param>
   /// <returns>The Location containing the point, or null if no location is found.</returns>
   public Location? Query(Vector2 point)
   {
      if (!Bounds.ContainsVec2(point))
         return null;

      // If this is a branch node, find the correct child and query it.
      if (_children != null)
      {
         var index = GetChildIndexForPoint(point);
         return _children[index].Query(point);
      }

      // If this is a leaf node, check the references it contains.
      if (_references == null)
         return null; // No match found in this leaf.

      foreach (var polyRef in _references)
      {
         // Resolve the reference to the actual polygon for a precise check.
         var location = _allLocations[polyRef.LocationId];
         var polygon = location.Polygons[polyRef.PolygonIndex];
         if (polygon.Contains(point))
            return location;
      }

      return null;
   }

   private void Subdivide()
   {
      var halfWidth = Bounds.Width / 2;
      var halfHeight = Bounds.Height / 2;
      var x = Bounds.X;
      var y = Bounds.Y;

      _children = new QuadTree[4];
      _children[0] = new(new(x, y, halfWidth, halfHeight), _allLocations, _depth + 1);
      _children[1] = new(new(x + halfWidth, y, halfWidth, halfHeight), _allLocations, _depth + 1);
      _children[2] = new(new(x, y + halfHeight, halfWidth, halfHeight), _allLocations, _depth + 1);
      _children[3] = new(new(x + halfWidth, y + halfHeight, halfWidth, halfHeight), _allLocations, _depth + 1);
   }

   private int GetChildIndexForPoint(Vector2 point)
   {
      var index = 0;
      var midX = Bounds.X + (Bounds.Width / 2);
      var midY = Bounds.Y + (Bounds.Height / 2);

      if (point.X > midX)
         index |= 1;
      if (point.Y > midY)
         index |= 2;

      // 0: Top-Left, 1: Top-Right, 2: Bottom-Left, 3: Bottom-Right
      return index;
   }

   #region Query Methods

   /// <summary>
   /// Retrieves all unique Locations whose polygons intersect with the given range.
   /// </summary>
   private List<Location> QueryRange(RectangleF range)
   {
      var foundLocations = new HashSet<Location>();
      QueryRangeRecursive(range, foundLocations);
      return [..foundLocations];
   }

   private void QueryRangeRecursive(RectangleF range, HashSet<Location> foundLocations)
   {
      if (!Bounds.IntersectsWith(range))
         return;

      if (_children != null)
         foreach (var child in _children)
            child.QueryRangeRecursive(range, foundLocations);

      else if (_references != null)
         foreach (var polyRef in _references)
         {
            var location = _allLocations[polyRef.LocationId];
            if (location.Polygons[polyRef.PolygonIndex].Bounds.IntersectsWith(range))
               foundLocations.Add(location);
         }
   }

   /// <summary>
   /// Finds all unique locations whose bounds either intersect with or are contained by a given rectangle.
   /// </summary>
   /// <param name="area">The rectangular search area.</param>
   public List<Location> FindLocations(RectangleF area)
   {
      return QueryRange(area);
   }

   /// <summary>
   /// Finds all unique locations that intersect with a given polygon.
   /// </summary>
   /// <param name="containerPolygon">The polygon to check against.</param>
   public List<Location> FindLocations(Polygon containerPolygon)
   {
      var results = new List<Location>();
      var containerBounds = containerPolygon.Bounds;

      var candidateLocations = QueryRange(containerBounds);
      foreach (var candidate in candidateLocations)
      {
         // Fast reject using bounding boxes.
         if (!containerBounds.IntersectsWith(candidate.Bounds))
            continue;

         foreach (var polygon in candidate.Polygons)
         {
            if (!containerPolygon.Intersects(polygon))
               continue;

            results.Add(candidate);
            break;
         }
      }

      return results;
   }

   #endregion
}