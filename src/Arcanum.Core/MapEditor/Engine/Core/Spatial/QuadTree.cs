using System.Numerics;
using Arcanum.Core.MapEditor.Engine.Core.Math;

namespace Arcanum.Core.MapEditor.Engine.Core.Spatial;

public partial class QuadTree<T> where T : I3DEntity
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

   public void Insert(T item) => _root.Insert(item, Flatten(item), 0, _maxObjectsPerNode, _maxDepth);

   public bool Remove(T item) => _root.Remove(item, Flatten(item));

   public void Move(T item, Vector3 newPosition)
   {
      var removed = _root.Remove(item, Flatten(item));

      if (!removed)
      {
         // Fallback: If strict removal failed, search the whole tree or log error
         // This happens if the user didn't track oldBounds correctly
      }

      item.Position3D = newPosition;
      Insert(item);
   }

   /// <summary>
   /// Non-allocating query. Pass a cached list to reuse memory.
   /// </summary>
   public void Query(ref RectF range, List<T> results) => _root.QueryRange(ref range, results);

   /// <summary>
   /// Allocating wrapper for convenience.
   /// </summary>
   public List<T> Query(RectF range)
   {
      var results = new List<T>();
      Query(ref range, results);
      return results;
   }

   public List<T> QueryPoint(Vector2 point, float radius = 1.0f) => Query(new(point.X - radius, point.Y - radius, radius * 2, radius * 2));

   public void Clear() => _root.Clear();

   internal static RectF Flatten(T item)
   {
      var b = item.Bounds3D;
      return new(b.Min.X, b.Min.Z, b.Width, b.Depth);
   }
}