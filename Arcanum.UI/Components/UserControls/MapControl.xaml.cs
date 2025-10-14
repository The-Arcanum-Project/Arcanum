using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.DirectX;
using Vortice.Mathematics;

namespace Arcanum.UI.Components.UserControls;

public partial class MapControl
{
   private D3D11HwndHost _d3dHost = null!;
   private LocationRenderer _locationRenderer = null!;
   
   private Point _lastMousePosition;
   private bool _isPanning;
   private float _imageAspectRatio;
   public MapControl()
   {
      InitializeComponent();
   }

   private void SetupEvents()
   {
      HwndHostContainer.MouseWheel += OnMouseWheel;
      HwndHostContainer.MouseLeftButtonDown += OnMouseLeftButtonDown;
      HwndHostContainer.MouseLeftButtonUp += OnMouseLeftButtonUp;
      HwndHostContainer.MouseMove += OnMouseMove;
   }

   private static Color4[] CreateColors(Polygon[] polygons)
   {
      var colors = new Color4[polygons.Length];
      
      for (var i = 0; i < polygons.Length; i++)
         colors[i] = new(polygons[i].Color);

      return colors;
   }

   public async Task SetupRenderer(Polygon[] polygons, (int, int) imageSize)
   {
      if (!IsLoaded)
         throw new InvalidOperationException("MapControl must be loaded before calling SetupRendering");

      _imageAspectRatio = (float)imageSize.Item2 / imageSize.Item1;
      
      var vertices = await Task.Run(() => LocationRenderer.CreateVertices(polygons, imageSize));
      var startColor = CreateColors(polygons);
      _locationRenderer = new (vertices, startColor, _imageAspectRatio);
      _d3dHost = new(_locationRenderer, HwndHostContainer);
      HwndHostContainer.Child = _d3dHost;

      DataContext = _d3dHost;
      LoadingPanel.Visibility = Visibility.Collapsed;
      UpdateRenderer();
      SetupEvents();
   }

   private void UpdateRenderer()
   {
      _locationRenderer.SetOrthographicProjection((float)HwndHostContainer.ActualWidth, (float)HwndHostContainer.ActualHeight);
      _d3dHost.Invalidate();
   }

   public Vector2 ScreenToMap(Point screenPoint)
   {
      return ScreenToMap(new Vector2((float)screenPoint.X, (float)screenPoint.Y));
   }
   
   public Vector2 ScreenToMap(Vector2 screenPoint)
   {
      var width = (float)HwndHostContainer.ActualWidth;
      var height = (float)HwndHostContainer.ActualHeight;
      var aspectRatio = width / height;

      var ndcX = screenPoint.X / width * 2.0f - 1.0f;
      var ndcY = 1.0f - screenPoint.Y / height * 2.0f;

      var worldX = ndcX * (aspectRatio / _locationRenderer.Zoom);
      var worldY = ndcY * (1.0f / _locationRenderer.Zoom);

      worldX += _locationRenderer.Pan.X;
      worldY += (1f - _locationRenderer.Pan.Y) * _imageAspectRatio;

      var mapX = worldX;
      var mapY = 1.0f - worldY / _imageAspectRatio;

      return new(mapX, mapY);
   }
   
   private void PanTo(float x, float y)
   {
      _locationRenderer.Pan.X = Math.Clamp(x, -0.1f, 1.1f);
      _locationRenderer.Pan.Y = Math.Clamp(y, -0.1f, 1.1f);
      UpdateRenderer();
   }
   
   private void OnMouseWheel(object sender, MouseWheelEventArgs e)
   {
      var pos = ScreenToMap(e.GetPosition(this));

      var zoomFactor = e.Delta > 0 ? 1.2f : 1 / 1.2f;

      var newZoom = _locationRenderer.Zoom * zoomFactor;

      if (newZoom < Config.Settings.MapSettings.MinZoomLevel || newZoom > Config.Settings.MapSettings.MaxZoomLevel)
         if (_locationRenderer.Zoom < Config.Settings.MapSettings.MinZoomLevel || _locationRenderer.Zoom > Config.Settings.MapSettings.MaxZoomLevel)
            newZoom = Math.Clamp(_locationRenderer.Zoom,
               Config.Settings.MapSettings.MinZoomLevel,
               Config.Settings.MapSettings.MaxZoomLevel);
         else
            return;

      _locationRenderer.Zoom = newZoom;

      var delta = new Vector2(pos.X - _locationRenderer.Pan.X, pos.Y - _locationRenderer.Pan.Y);

      delta /= zoomFactor;

      var newPan = pos - delta;
      PanTo(newPan.X, newPan.Y);
   }
   
   private void OnMouseMove(object sender, MouseEventArgs e)
   {
      if (!_isPanning)
         return;
      
      Mouse.OverrideCursor = Cursors.ScrollAll;
      
      var currentMousePosition = e.GetPosition(HwndHostContainer);
      var delta = currentMousePosition - _lastMousePosition;

      var aspectRatio = (float)(HwndHostContainer.ActualWidth / HwndHostContainer.ActualHeight);

      var x = _locationRenderer.Pan.X - (float)(delta.X * 2 / (HwndHostContainer.ActualWidth * _locationRenderer.Zoom)) * aspectRatio;
      var y = _locationRenderer.Pan.Y - (float)(delta.Y * 2 / (HwndHostContainer.ActualHeight * _locationRenderer.Zoom)) / _imageAspectRatio;

      PanTo(x, y);

      _lastMousePosition = currentMousePosition;
   }
   
   private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is not IInputElement surface)
         return;
      
      _isPanning = true;
      _lastMousePosition = e.GetPosition(surface);
      surface.CaptureMouse();
   }

   private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
   {
      if (sender is not IInputElement surface)
         return;
      if (_isPanning)
      {
         Mouse.OverrideCursor = null;
         _isPanning = false;
      }

      surface.ReleaseMouseCapture();
   }
}