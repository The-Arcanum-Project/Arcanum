using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.MapEditor.Engine.Core.Math;

namespace Arcanum.Core.MapEditor.Engine.Core.Spatial;

public partial class QuadTree<T> where T : ISpatialEntity
{
   private readonly Node _root;

   // Config stored only at the Tree level, not in every Node
   private readonly int _maxObjectsPerNode;
   private readonly int _maxDepth;

   public QuadTree(ref RectF bounds, int maxObjectsPerNode = 10, int maxDepth = 8)
   {
      _root = new(bounds);
      _maxObjectsPerNode = maxObjectsPerNode;
      _maxDepth = maxDepth;
   }

   public void Insert(T item) => _root.Insert(item, 0, _maxObjectsPerNode, _maxDepth);

   public bool Remove(T item) => _root.Remove(item);

   public void Move(T item, Vector2I newPosition)
   {
      if (!Remove(item))
         return;

      item.MoveTo(newPosition);
      Insert(item);
   }

   /// <summary>
   /// Non-allocating query. Pass a cached list to reuse memory.
   /// </summary>
   public void Query(RectF range, List<T> results) => _root.QueryRange(ref range, results);

   /// <summary>
   /// Allocating wrapper for convenience.
   /// </summary>
   public List<T> Query(RectF range)
   {
      var results = new List<T>();
      Query(range, results);
      return results;
   }

   public List<T> QueryPoint(Vector2I point, float radius = 1.0f) => Query(new(point.X - radius, point.Y - radius, radius * 2, radius * 2));

   public void Clear() => _root.Clear();
}