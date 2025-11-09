using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.UserControls;

namespace Arcanum.UI.MapInteraction.Strategy;

public class MapNavigationStrategy(MapInteractionManager mapInteractionManager) : IMapInteractionStrategy
{
    private bool _isPanning;
    private bool _hasPanned;
    private Point _lastMousePosition;
    private const float ZOOM_STEP = 1.2f;
    private const float ZOOM_LOG_BASE = 2f;
    private static readonly float LnZoomLogBase = MathF.Log(ZOOM_LOG_BASE);
    
    public event Action? OnPanningStarted;
    public event Action? OnPanningEnded;
    
    public void Enter(MapControl map)
    {
    }

    public void Exit(MapControl map)
    {
        _isPanning = false;
        _hasPanned = false;
        Mouse.OverrideCursor = null;
    }

    public void OnMouseDown(MapControl map, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle && e.MiddleButton == MouseButtonState.Pressed)
        {
            _isPanning = true;
            OnPanningStarted?.Invoke();
            _lastMousePosition = e.GetPosition(map.HwndHostContainer);
        }
    }

    public void OnMouseMove(MapControl map, MouseEventArgs e)
    {
        if (!_isPanning)
            return;

        var currentPos = e.GetPosition(map.HwndHostContainer);
        var deltaX = currentPos.X - _lastMousePosition.X;
        var deltaY = currentPos.Y - _lastMousePosition.Y;

        if (!_hasPanned &&
            Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _hasPanned = true;
        Mouse.OverrideCursor = Cursors.ScrollAll;

        var aspectRatio = (float)(map.HwndHostContainer.ActualWidth / map.HwndHostContainer.ActualHeight);

        var panX = map.LocationRenderer.Pan.X - (float)(deltaX / (map.HwndHostContainer.ActualWidth * map.LocationRenderer.Zoom)) * map.Coords.ImageAspectRatio * aspectRatio;
        var panY = map.LocationRenderer.Pan.Y - (float)(deltaY / (map.HwndHostContainer.ActualHeight * map.LocationRenderer.Zoom));

        map.PanTo(panX, panY);
        _lastMousePosition = currentPos;
    }

    public void OnMouseUp(MapControl map, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle) return;
        Mouse.OverrideCursor = null;
        _isPanning = false;
        if (!_hasPanned) return;
        OnPanningEnded?.Invoke();
        _hasPanned = false;
    }

    public void OnMouseWheel(MapControl map, MouseWheelEventArgs e)
    {
        var mapPos = map.Coords.CurrentPosition.Norm;

        var zoomFactor = e.Delta > 0 ? ZOOM_STEP : 1 / ZOOM_STEP;

        var maxZoom = MathF.Exp(Config.Settings.MapSettings.MaxZoomLevel * LnZoomLogBase);
        var minZoom = MathF.Exp(Config.Settings.MapSettings.MinZoomLevel * LnZoomLogBase);

        var newZoom = map.LocationRenderer.Zoom * zoomFactor;
        if (newZoom < minZoom || newZoom > maxZoom)
            return;

        map.LocationRenderer.Zoom = newZoom;

        // Keep focus point under cursor stable
        var delta = new Vector2((float)mapPos.X - map.LocationRenderer.Pan.X, (float)mapPos.Y - map.LocationRenderer.Pan.Y);
        delta /= zoomFactor;

        var newPan = mapPos - delta;
        map.PanTo(newPan.X, newPan.Y);
    }
}