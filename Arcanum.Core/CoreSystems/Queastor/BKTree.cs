namespace Arcanum.Core.CoreSystems.Queastor;

public class BkTree
{
   private Node? _root;

   private class Node(string term)
   {
      public readonly string Term = term;
      public readonly Dictionary<int, Node> Children = [];
   }
   
   public void Add(string term)
   {
      term = term.ToLowerInvariant();
      if (_root == null)
      {
         _root = new(term);
         return;
      }

      var current = _root;
      while (true)
      {
         var distance = Queastor.LevinsteinDistance(term, current.Term);
         if (distance == 0)
            return;

         if (!current.Children.TryGetValue(distance, out var child))
         {
            current.Children[distance] = new(term);
            return;
         }

         current = child;
      }
   }

   public List<string> Search(string query, int maxDistance)
   {
      var results = new List<string>();
      if (_root == null)
         return results;

      var stack = new Stack<Node?>();
      stack.Push(_root);

      while (stack.Count > 0)
      {
         var node = stack.Pop();
         if (node == null)
            continue;
         var distance = Queastor.LevinsteinDistance(query, node.Term);
         if (distance <= maxDistance)
            results.Add(node.Term);

         for (var i = distance - maxDistance; i <= distance + maxDistance; i++)
         {
            if (i < 0)
               continue;

            if (node.Children.TryGetValue(i, out var child))
               stack.Push(child);
         }
      }

      return results;
   }
}