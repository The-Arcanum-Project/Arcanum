using Arcanum.API;

namespace Arcanum.Core.PluginServices;

public static class DependencyManager
{
   public static List<IPluginMetadata> TopologicalSort(Dictionary<Guid, PluginNode> nodes)
   {
      // Build edges (dependencies)
      foreach (var node in nodes.Values)
      {
         foreach (var dep in node.Metadata.Dependencies)
            if (nodes.TryGetValue(dep.PluginGuid, out var depNode))
               node.Dependencies.Add(depNode);
            else
               throw new($"Missing dependency {dep.PluginGuid} for plugin {node.Metadata.Guid}");
      }

      var sorted = new List<IPluginMetadata>();
      var visited = new HashSet<PluginNode>();
      var visiting = new HashSet<PluginNode>();

      foreach (var node in nodes.Values)
         if (!Visit(node))
            throw
               new($"Circular plugin dependency detected for node '{node.Metadata.Name}' ({node.Metadata.Guid})");

      return sorted;

      bool Visit(PluginNode n)
      {
         if (visited.Contains(n))
            return true;
         if (!visiting.Add(n))
            return false; // cycle detected

         foreach (var dep in n.Dependencies)
            if (!Visit(dep))
               return false;

         visiting.Remove(n);

         visited.Add(n);
         sorted.Add(n.Metadata);
         return true;
      }
   }

   public static List<IPluginMetadata> GetAllDependentFor(IPluginMetadata plugin,
                                                          IEnumerable<IPluginMetadata> allPlugins)
   {
      var nodes = allPlugins.ToDictionary(p => p.Guid, p => new PluginNode(p));
      var result = new List<IPluginMetadata>();

      // Build edges (dependencies)
      foreach (var node in nodes.Values)
      {
         foreach (var dep in node.Metadata.Dependencies)
            if (nodes.TryGetValue(dep.PluginGuid, out var depNode))
               node.Dependencies.Add(depNode);
      }

      if (nodes.TryGetValue(plugin.Guid, out var startNode))
         FindDependencies(startNode);

      return result;

      // Find all dependencies of the given plugin
      void FindDependencies(PluginNode n)
      {
         if (result.Contains(n.Metadata))
            return;

         result.Add(n.Metadata);
         foreach (var dep in n.Dependencies)
            FindDependencies(dep);
      }
   }

   public static List<IPluginMetadata> GetAllDependentOn(IPluginMetadata plugin,
                                                         IEnumerable<IPluginMetadata> allPlugins)
   {
      var nodes = allPlugins.ToDictionary(p => p.Guid, p => new PluginNode(p));

      // Build reverse edges: from dependency to dependents
      foreach (var node in nodes.Values)
      {
         foreach (var dep in node.Metadata.Dependencies)
            if (nodes.TryGetValue(dep.PluginGuid, out var depNode))
               depNode.Dependents.Add(node);
      }

      var result = new List<IPluginMetadata>();
      var visited = new HashSet<IPluginMetadata>();

      if (nodes.TryGetValue(plugin.Guid, out var startNode))
         FindDependents(startNode);

      return result;

      void FindDependents(PluginNode n)
      {
         if (!visited.Add(n.Metadata))
            return;

         result.Add(n.Metadata);

         foreach (var dependent in n.Dependents)
            FindDependents(dependent);
      }
   }

   public class PluginNode
   {
      public IPluginMetadata Metadata { get; }
      public List<PluginNode> Dependencies { get; } = [];
      public List<PluginNode> Dependents { get; } = [];

      public PluginNode(IPluginMetadata metadata)
      {
         Metadata = metadata;
      }
   }
}