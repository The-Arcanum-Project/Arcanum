using System.Windows.Input;
using Arcanum.UI.Components.UserControls;

namespace Arcanum.UI.MapInteraction;

/// <summary>
/// Defines a strategy for handling map interactions such as panning, zooming, and selecting.
/// </summary>

public interface IMapInteractionStrategy
{
    void Enter(MapControl map);
    void Exit(MapControl map);
    void OnMouseDown(MapControl map, MouseButtonEventArgs e);
    void OnMouseMove(MapControl map, MouseEventArgs e);
    void OnMouseUp(MapControl map, MouseButtonEventArgs e);
    void OnMouseWheel(MapControl map, MouseWheelEventArgs e);
}