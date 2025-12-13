using System.Windows;

namespace Arcanum.UI.Util.WindowManagement;

public struct WindowInformation(int maxInstances, Type type)
{
   public int MaxInstances { get; init; } = maxInstances;
   public Type Type { get; init; } = type;
   public Point Location { get; set; } = new(-1, -1);
   public Size Size { get; set; } = new(0, 0);
   public int ZIndex { get; set; } = -1;

   public bool HasCustomLocation => Location is { X: >= 0, Y: >= 0 };
   public bool HasCustomSize => Size is { Width: >= 0, Height: >= 0 };
}