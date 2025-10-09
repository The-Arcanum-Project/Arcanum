using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public static class GraphHelper
{
   public static NodeGraph CreateGraph(this IEu5Object target, Type? propType = null)
   {
      propType ??= target.GetType();

      var graph = new NodeGraph();
      var nodes = new Dictionary<IEu5Object, GraphNode>();
      var edges = new List<Edge>();
      var visitedInTraversal = new HashSet<IEu5Object>();

      Traverse(target, propType);

      graph.Nodes = nodes.Values.ToList();
      graph.Edges = edges;

      return graph;

      void AddNode(IEu5Object obj)
      {
         if (nodes.ContainsKey(obj))
            return;

         var node = new GraphNode
         {
            Name = obj.UniqueId,
            X = Random.Shared.NextSingle() * 100,
            Y = Random.Shared.NextSingle() * 100,
            Displacement = new(0, 0),
            LinkedObject = obj,
         };
         nodes[obj] = node;
      }

      void Traverse(IEu5Object obj, Type type)
      {
         if (!visitedInTraversal.Add(obj))
            return;

         AddNode(obj);

         foreach (var propValue in Nx.GetPropertiesOfType(obj, type))
         {
            var related = Nx.ForceGetAs<IEu5Object>(target, propValue);
            if (Equals(EmptyRegistry.Empties[type], related))
               continue;

            AddNode(related);

            var edgeExists = edges.Any(e => (e.Source == nodes[obj] && e.Target == nodes[related]));
            if (!edgeExists)
               edges.Add(new(nodes[obj], nodes[related]));

            Traverse(related, type);
         }
         // After traversing all children, remove the current object from 'visitedInTraversal'
         // if you want to allow it to be revisited via a different path later on for *new* edges
         // (e.g., if A->B and C->B, B would be added to visited via A, then when traversing C,
         // if B is in visitedInTraversal, we skip it. If you remove it here, C could then
         // re-process B. For graph building this usually means leaving it in the HashSet)
         // For now, leaving it in visitedInTraversal is usually the safest for preventing SOF.
      }
   }
}