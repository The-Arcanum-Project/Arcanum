using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.DirectX;
using CommunityToolkit.Mvvm.Input;
using Vortice.Mathematics;

namespace Arcanum.UI.Components.UserControls;

public partial class MapControl
{
   private D3D11HwndHost _d3dHost = null!;
   private LocationRenderer _locationRenderer = null!;

   private const float ZOOM_STEP = 1.2f;

   private Point _lastMousePosition;
   private bool _isPanning;
   private float _imageAspectRatio;
   private (int, int) _imageSize;

   public event Action<Vector2>? OnAbsolutePositionChanged;
   public static event Action? OnMapLoaded;

   public static readonly DependencyProperty CurrentPosProperty =
      DependencyProperty.Register(nameof(CurrentPos), typeof(Vector2), typeof(MapControl), new(default(Vector2)));

   public Vector2 CurrentPos
   {
      get => (Vector2)GetValue(CurrentPosProperty);
      set => SetValue(CurrentPosProperty, value);
   }

   public MapControl()
   {
      InitializeComponent();
   }

   private void SetupEvents()
   {
      HwndHostContainer.MouseWheel += OnMouseWheel;
      HwndHostContainer.MouseLeftButtonDown += OnMouseLeftButtonDown;
      HwndHostContainer.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
      HwndHostContainer.MouseMove += OnMouseMove;
      HwndHostContainer.MouseLeave += OnMouseLeave;

      OnAbsolutePositionChanged += pos =>
      {
         if (!Selection.MapManager.IsMapDataInitialized)
            return;

         var loc = Selection.MapManager.FindLocationAt(pos) ?? Location.Empty;
         Selection.Modify(SelectionTarget.Hover, [loc], true);
      };
   }

   private void OnMouseLeave(object sender, MouseEventArgs e)
   {
      // Stop panning if the mouse leaves the control
      if (_isPanning)
      {
         Mouse.OverrideCursor = null;
         _isPanning = false;
         _hasPanned = false;
      }

      CurrentPos = Vector2.Zero;
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

      _imageSize = imageSize;

      _imageAspectRatio = (float)imageSize.Item2 / imageSize.Item1;

      var vertices = await Task.Run(() => LocationRenderer.CreateVertices(polygons, imageSize));
      var startColor = CreateColors(polygons);
      _locationRenderer = new(vertices, startColor, _imageAspectRatio);
      _d3dHost = new(_locationRenderer, HwndHostContainer);
      HwndHostContainer.Child = _d3dHost;

      DataContext = _d3dHost;
      LoadingPanel.Visibility = Visibility.Collapsed;
      UpdateRenderer();
      SetupEvents();

      OnMapLoaded?.Invoke();
   }

   private void UpdateRenderer()
   {
      _locationRenderer.SetOrthographicProjection((float)HwndHostContainer.ActualWidth,
                                                  (float)HwndHostContainer.ActualHeight);
      _d3dHost.Invalidate();
   }

   public Vector2 ScreenToMap(Point screenPoint)
   {
      return ScreenToMap(new Vector2((float)screenPoint.X, (float)screenPoint.Y));
   }

   public Vector2 ScreenToMapAbsolute(Vector2 screenPoint)
   {
      var mapPoint = ScreenToMap(screenPoint);
      return new Vector2(mapPoint.X * _imageSize.Item1, mapPoint.Y * _imageSize.Item2);
   }

   public Vector2 ScreenToMapAbsolute(Point screenPoint)
   {
      return ScreenToMapAbsolute(new Vector2((float)screenPoint.X, (float)screenPoint.Y));
   }

   public ICommand ClickCommand => new RelayCommand<MouseButtonEventArgs>(args =>
   {
      if (args == null)
         return;

      var pos = ScreenToMap(args.GetPosition(this));
      Console.WriteLine(pos);
   });

   // Command to load the project in the Arcanum view
   public ICommand DoubleClickCommand => new RelayCommand<MouseButtonEventArgs>(args => { });

   public Vector2 ScreenToMap(Vector2 screenPoint)
   {
      var width = (float)HwndHostContainer.ActualWidth;
      var height = (float)HwndHostContainer.ActualHeight;
      var aspectRatio = width / height;

      // Get Coordinates from -1 to 1
      var ndcX = screenPoint.X / width * 2.0f - 1.0f;
      var ndcY = 1.0f - screenPoint.Y / height * 2.0f;
      var zoomRatio = _imageAspectRatio / (_locationRenderer.Zoom * 2);
      var worldX = ndcX * (aspectRatio * zoomRatio);
      var worldY = ndcY * (zoomRatio);

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

   private const float ZOOM_LOG_BASE = 2f;
   private static readonly float LnZoomLogBase = MathF.Log(ZOOM_LOG_BASE);

   private void OnMouseWheel(object sender, MouseWheelEventArgs e)
   {
      var pos = ScreenToMap(e.GetPosition(this));

      var zoomFactor = e.Delta > 0 ? ZOOM_STEP : 1 / ZOOM_STEP;

      var maxZoom = MathF.Exp(Config.Settings.MapSettings.MaxZoomLevel * LnZoomLogBase);
      var minZoom = MathF.Exp(Config.Settings.MapSettings.MinZoomLevel * LnZoomLogBase);

      var newZoom = _locationRenderer.Zoom * zoomFactor;

      if (newZoom < minZoom || maxZoom < newZoom)
      {
         return;
      }

      _locationRenderer.Zoom = newZoom;

      var delta = new Vector2(pos.X - _locationRenderer.Pan.X, pos.Y - _locationRenderer.Pan.Y);

      delta /= zoomFactor;

      var newPan = pos - delta;
      PanTo(newPan.X, newPan.Y);
   }

   private bool _hasPanned;

   private void OnMouseMove(object sender, MouseEventArgs e)
   {
      if (!_isPanning)
         UpdateCursorLocation(e.GetPosition(HwndHostContainer));

      if (!_hasPanned &&
          (e.LeftButton != MouseButtonState.Pressed ||
           (!(Math.Abs(e.GetPosition(HwndHostContainer).X - _lastMousePosition.X) >
              SystemParameters.MinimumHorizontalDragDistance) &&
            !(Math.Abs(e.GetPosition(HwndHostContainer).Y - _lastMousePosition.Y) >
              SystemParameters.MinimumVerticalDragDistance))))
         return;

      _hasPanned = true;

      Mouse.OverrideCursor = Cursors.ScrollAll;

      var currentMousePosition = e.GetPosition(HwndHostContainer);
      var delta = currentMousePosition - _lastMousePosition;

      var aspectRatio = (float)(HwndHostContainer.ActualWidth / HwndHostContainer.ActualHeight);

      var x = _locationRenderer.Pan.X -
              (float)(delta.X / (HwndHostContainer.ActualWidth * _locationRenderer.Zoom)) *
              _imageAspectRatio *
              aspectRatio;
      var y = _locationRenderer.Pan.Y -
              (float)(delta.Y / (HwndHostContainer.ActualHeight * _locationRenderer.Zoom));

      PanTo(x, y);

      _lastMousePosition = currentMousePosition;
   }

   private void UpdateCursorLocation(Point position)
   {
      CurrentPos = ScreenToMapAbsolute(position);
      OnAbsolutePositionChanged?.Invoke(CurrentPos);
   }

   private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is not IInputElement surface)
         return;

      _isPanning = true;
      _lastMousePosition = e.GetPosition(surface);
   }

   private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
   {
      if (!_isPanning)
         return;

      Mouse.OverrideCursor = null;
      _isPanning = false;
      if (!_hasPanned)
         return;

      e.Handled = true;
      _hasPanned = false;
   }
}