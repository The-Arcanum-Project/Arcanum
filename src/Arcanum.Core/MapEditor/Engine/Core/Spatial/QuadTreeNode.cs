using System.Runtime.CompilerServices;
using Arcanum.Core.MapEditor.Engine.Core.Math;

namespace Arcanum.Core.MapEditor.Engine.Core.Spatial;

public partial class QuadTree<T>
{
   private class Node(RectF bounds)
   {
      private readonly RectF _bounds = bounds;
      private readonly float _midX = bounds.X + bounds.Width * 0.5f;
      private readonly float _midY = bounds.Y + bounds.Height * 0.5f;

      private List<T>? _items;
      private Node[]? _children;

      public void Insert(T item, RectF itemBounds, int currentDepth, int maxObjects, int maxDepth)
      {
         if (_children != null)
         {
            var index = GetChildIndex(in itemBounds);

            if (index != -1)
            {
               // Pass depth + 1 down the stack
               _children[index].Insert(item, itemBounds, currentDepth, maxObjects, maxDepth);
               return;
            }
         }

         _items ??= new(maxObjects);
         _items.Add(item);

         if (_items.Count > maxObjects && currentDepth < maxDepth && _children == null)
            Split(currentDepth, maxObjects, maxDepth);
      }

      private void Split(int currentDepth, int maxObjects, int maxDepth)
      {
         var subW = _bounds.Width * 0.5f;
         var subH = _bounds.Height * 0.5f;
         var x = _bounds.X;
         var y = _bounds.Y;

         // We do not pass depth to the constructor anymore
         _children = new Node[4];
         _children[0] = new(new(x + subW, y, subW, subH)); // Top Right
         _children[1] = new(new(x, y, subW, subH)); // Top Left
         _children[2] = new(new(x, y + subH, subW, subH)); // Bottom Left
         _children[3] = new(new(x + subW, y + subH, subW, subH)); // Bottom Right

         // Redistribute existing items
         var list = _items!;
         var nextDepth = currentDepth + 1;

         // Iterate backwards to allow swap-remove
         for (var i = list.Count - 1; i >= 0; i--)
         {
            var item = list[i];
            var b = Flatten(item);
            var index = GetChildIndex(in b);

            if (index != -1)
            {
               // Insert into child with incremented depth
               _children[index].Insert(item, b, nextDepth, maxObjects, maxDepth);

               // Swap-Remove from current list
               var lastIdx = list.Count - 1;
               if (i < lastIdx)
                  list[i] = list[lastIdx];

               list.RemoveAt(lastIdx);
            }
         }
      }

      public bool Remove(T item, RectF oldBounds)
      {
         if (_children != null)
         {
            var index = GetChildIndex(in oldBounds);
            if (index != -1 && _children[index].Remove(item, oldBounds))
               return true;
         }

         if (_items == null)
            return false;

         // Swap-Removal
         var count = _items.Count;
         for (var i = 0; i < count; i++)
            if (_items[i].Id == item.Id)
            {
               var lastIndex = count - 1;
               if (i < lastIndex)
                  _items[i] = _items[lastIndex];

               _items.RemoveAt(lastIndex);
               return true;
            }

         return false;
      }

      public void QueryRange(ref RectF range, List<T> results)
      {
         if (_items != null)
         {
            var count = _items.Count;
            for (var i = 0; i < count; i++)
            {
               var item = _items[i];
               if (range.Intersects(Flatten(item)))
                  results.Add(item);
            }
         }

         if (_children != null)
         {
            var c = _children;
            if (c[0]._bounds.Intersects(range))
               c[0].QueryRange(ref range, results);
            if (c[1]._bounds.Intersects(range))
               c[1].QueryRange(ref range, results);
            if (c[2]._bounds.Intersects(range))
               c[2].QueryRange(ref range, results);
            if (c[3]._bounds.Intersects(range))
               c[3].QueryRange(ref range, results);
         }
      }

      public void Clear()
      {
         _items?.Clear();
         _children = null;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private int GetChildIndex(in RectF itemBounds)
      {
         var topQuadrant = itemBounds.Y < _midY && itemBounds.Bottom < _midY;
         var bottomQuadrant = itemBounds.Y > _midY;

         if (itemBounds.X < _midX && itemBounds.Right < _midX)
         {
            if (topQuadrant)
               return 1;
            if (bottomQuadrant)
               return 2;
         }
         else if (itemBounds.X > _midX)
         {
            if (topQuadrant)
               return 0;
            if (bottomQuadrant)
               return 3;
         }

         return -1;
      }
   }
}