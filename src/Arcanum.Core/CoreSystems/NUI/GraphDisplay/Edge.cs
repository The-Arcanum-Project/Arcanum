namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public class Edge(GraphNode source, GraphNode target)
{
   public GraphNode Source { get; set; } = source;
   public GraphNode Target { get; set; } = target;
}