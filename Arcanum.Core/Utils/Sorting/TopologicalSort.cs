namespace Arcanum.Core.Utils.Sorting;

public static class TopologicalSort
{
   public static List<TNode> Sort<TId, TNode>(IEnumerable<TNode> nodes)
      where TNode : IDependencyNode<TId> where TId : notnull
   {
      var nodeDict = nodes.ToDictionary(n => n.Id);
      var sorted = new List<TNode>();
      var visited = new HashSet<TId>();
      var visiting = new HashSet<TId>();

      foreach (var node in nodeDict.Values)
         if (!Visit(node))
            throw new InvalidOperationException($"Circular dependency detected for node '{node.Id}'");

      return sorted;

      bool Visit(TNode n)
      {
         if (visited.Contains(n.Id))
            return true;
         if (!visiting.Add(n.Id))
            return false;

         foreach (var depId in n.Dependencies)
            if (nodeDict.TryGetValue(depId, out var depNode))
            {
               if (!Visit(depNode))
                  return false;
            }
            else
            {
               throw new InvalidOperationException($"Missing dependency '{depId}' for node '{n.Id}'");
            }

         visiting.Remove(n.Id);
         visited.Add(n.Id);
         sorted.Add(n);
         return true;
      }
   }

   public static List<TNode> GetAllDependencies<TId, TNode>(TNode root, IEnumerable<TNode> allNodes)
      where TNode : IDependencyNode<TId> where TId : notnull
   {
      var nodeMap = allNodes.ToDictionary(n => n.Id, n => n);
      var result = new List<TNode>();
      var visited = new HashSet<TId>();

      Dfs(root, visited, result, nodeMap);
      return result;
   }

   private static void Dfs<TNode, TId>(TNode current,
                                       HashSet<TId>? visited,
                                       List<TNode>? result,
                                       Dictionary<TId, TNode>? nodeMap)
      where TNode : IDependencyNode<TId> where TId : notnull
   {
      if (visited != null && !visited.Add(current.Id))
         return;

      result?.Add(current);

      foreach (var depId in current.Dependencies)
         if (nodeMap != null && nodeMap.TryGetValue(depId, out var depNode))
            Dfs(depNode, visited, result, nodeMap);
   }

   public static List<TNode> GetAllDependents<TId, TNode>(TNode root, IEnumerable<TNode> allNodes)
      where TNode : IDependencyNode<TId> where TId : notnull
   {
      var dependentsMap = new Dictionary<TId, List<TNode>>();

      // Build reverse edges
      foreach (var node in allNodes)
      {
         foreach (var depId in node.Dependencies)
         {
            if (!dependentsMap.TryGetValue(depId, out var list))
            {
               list = [];
               dependentsMap[depId] = list;
            }

            list.Add(node);
         }
      }

      var result = new List<TNode>();
      var visited = new HashSet<TId>();
      var stack = new Stack<TNode>();

      stack.Push(root);

      while (stack.Count > 0)
      {
         var current = stack.Pop();
         if (!visited.Add(current.Id))
            continue;

         result.Add(current);

         if (dependentsMap.TryGetValue(current.Id, out var dependents))
         {
            foreach (var depNode in dependents)
               stack.Push(depNode);
         }
      }

      return result;
   }
}