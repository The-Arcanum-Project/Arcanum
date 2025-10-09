using System.Numerics;

namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public class GraphNode
{
   public string Name { get; set; }
   public float X { get; set; }
   public float Y { get; set; }
   public Vector2 Displacement { get; set; }
}