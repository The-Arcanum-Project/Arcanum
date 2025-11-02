using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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

   public void BuildFrom(HashSet<string> terms)
   {
      var termArray = terms.ToArray();
      if (termArray.Length == 0)
      {
         _root = null;
         return;
      }

      _root = BuildRecursive(termArray.AsSpan());
   }

   [SuppressMessage("ReSharper", "UseIndexFromEndExpression")]
   private static Node BuildRecursive(Span<string> terms)
   {
      if (terms.IsEmpty)
         return null!;

      var pivotIndex = Random.Shared.Next(terms.Length);
      var pivotTerm = terms[pivotIndex];
      var node = new Node(pivotTerm);

      (terms[pivotIndex], terms[terms.Length - 1]) = (terms[terms.Length - 1], terms[pivotIndex]); // Swap
      var remainingTerms = terms[..^1];

      // Early exit if there are no children to process.
      if (remainingTerms.IsEmpty)
         return node;

      var groupsByDistance = new Dictionary<int, List<string>>();
      foreach (var term in remainingTerms)
      {
         var distance = Queastor.LevinsteinDistance(pivotTerm, term);
         if (distance <= 0) // Guard against duplicates/pivot
            continue;

         if (!groupsByDistance.TryGetValue(distance, out var group))
         {
            group = [];
            groupsByDistance[distance] = group;
         }

         group.Add(term);
      }

      foreach (var (distance, group) in groupsByDistance)
         node.Children[distance] = BuildRecursive(CollectionsMarshal.AsSpan(group));

      return node;
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

   public void Clear() => _root = null;
}