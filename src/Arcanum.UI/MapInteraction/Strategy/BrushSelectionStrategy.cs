using System.Numerics;
using System.Windows.Input;
using Arcanum.UI.Components.UserControls;

namespace Arcanum.UI.MapInteraction.Strategy;

public class BrushSelectionStrategy : IMapInteractionStrategy
{
    // Number of points used to approximate the circular brush
    private const int BRUSH_RESOLUTION = 32;
    
    private readonly Vector2[] _brushPoints = new Vector2[BRUSH_RESOLUTION];
    
    private float _brushRadius = 0.05f;
    
    private void GenerateBrushPoints(Vector2 pos,float radius)
    {
        for (int i = 0; i < BRUSH_RESOLUTION; i++)
        {
            float angle = (float)(i) / BRUSH_RESOLUTION * MathF.PI * 2;
            _brushPoints[i] = new Vector2(
                pos.X + radius * MathF.Cos(angle),
                pos.Y + radius * MathF.Sin(angle)
            );
        }
    }
    
    private void UpdateBrushOutline(MapControl map, Vector2 pos)
    {
        GenerateBrushPoints(pos, _brushRadius);
        map.LocationRenderer.UpdateSelectionOutline(_brushPoints, true);
        map.LocationRenderer.Render();
    }
    
    public void Enter(MapControl map)
    {
        var pos = map.Coords.CurrentPosition.Ndc;
        UpdateBrushOutline(map, pos);
    }

    public void Exit(MapControl map)
    {
        map.LocationRenderer.ClearSelectionOutline();
        map.LocationRenderer.Render();
    }

    public void OnMouseDown(MapControl map, MouseButtonEventArgs e)
    {
        
    }

    public void OnMouseMove(MapControl map, MouseEventArgs e)
    {
        var pos =  map.Coords.CurrentPosition.Ndc;
        UpdateBrushOutline(map, pos);
    }

    public void OnMouseUp(MapControl map, MouseButtonEventArgs e)
    {
        
    }

    public void OnMouseWheel(MapControl map, MouseWheelEventArgs e)
    {
        _brushRadius = e.Delta > 0 ? MathF.Min(_brushRadius * 1.3f, 0.5f) : MathF.Max(_brushRadius / 1.3f, 0.01f);
        UpdateBrushOutline(map,  map.Coords.CurrentPosition.Ndc);
    }
}