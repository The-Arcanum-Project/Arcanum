using System.Numerics;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.Map;

public class MapClickEventArgs(Vector2 position, MouseButton button) : EventArgs
{
   public Vector2 ClickPosition { get; } = position;
   public MouseButton MouseButton { get; } = button;
}