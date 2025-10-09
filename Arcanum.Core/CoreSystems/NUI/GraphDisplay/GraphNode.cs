using System.Numerics;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public class GraphNode
{
   public string Name { get; set; }
   public float X { get; set; }
   public float Y { get; set; }
   public Vector2 Displacement { get; set; }
   public TextBlock? Label { get; set; }
   public IEu5Object LinkedObject { get; set; } = null!;
}