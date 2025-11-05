using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Behaviors;
using Arcanum.UI.DirectX;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using Vortice.Mathematics;
using Point = System.Windows.Point;

namespace Arcanum.UI.Components.UserControls;

public partial class MapControl
{
   private D3D11HwndHost _d3dHost = null!;
   public LocationRenderer LocationRenderer { get; private set; } = null!;

   private const float ZOOM_STEP = 1.2f;

   private Point _lastMousePosition;
   private bool _isPanning;
   private float _imageAspectRatio;
   private (int, int) _imageSize;

   private Color4[] _currentBackgroundColor;
   private Color4[] _selectionColor;

   private bool _hasPanned;

   public event Action<Vector2>? OnAbsolutePositionChanged;
   public static event Action? OnMapLoaded;

   public static readonly DependencyProperty CurrentPosProperty =
      DependencyProperty.Register(nameof(CurrentPos), typeof(Vector2), typeof(MapControl), new(default(Vector2)));

   public Vector2 CurrentPos
   {
      get => (Vector2)GetValue(CurrentPosProperty);
      set => SetValue(CurrentPosProperty, value);
   }

   // Command to load the project in the Arcanum view
   public ICommand DoubleClickCommand => new RelayCommand<MouseButtonEventArgs>(args => { });
   private const float ZOOM_LOG_BASE = 2f;
   private static readonly float LnZoomLogBase = MathF.Log(ZOOM_LOG_BASE);

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
         Selection.CurrentLocationBelowMouse = loc;
         Selection.Clear(SelectionTarget.Hover);
         Selection.Modify(SelectionTarget.Hover, SelectionMethod.Simple, [loc], true);
      };

      var behaviors = Interaction.GetBehaviors(HwndHostContainer);
      behaviors.Add(new ClickAndDoubleClickBehavior
      {
         FireSingleClickOnDoubleClick = true, RespectMouseMove = true,
      });
   }

   public void SetColors(Color4[] colors)
   {
      Debug.Assert(colors.Length == _currentBackgroundColor.Length,
                   "Color array length does not match the number of locations.");
      
      _currentBackgroundColor = colors;
      _selectionColor = (Color4[])_currentBackgroundColor.Clone();
      LocationRenderer.UpdateColors(_currentBackgroundColor);
      _d3dHost.Invalidate();
   }

   private static Color4[] CreateColors(Polygon[] polygons)
   {
      var locations = Globals.Locations.Values;
      var colors = new Color4[locations.Count];

      foreach (var location in locations)
      {
         var color = new Color4(location.Color.AsInt());
         colors[location.ColorIndex] = color;
      }

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
      LocationRenderer = new(vertices, startColor, _imageAspectRatio);
      _currentBackgroundColor = startColor;
      _selectionColor = (Color4[])_currentBackgroundColor.Clone();
      _d3dHost = new(LocationRenderer, HwndHostContainer, OnRendererLoaded);
      HwndHostContainer.Child = _d3dHost;

      DataContext = _d3dHost;
      LoadingPanel.Visibility = Visibility.Collapsed;
   }
   
   private void OnRendererLoaded(object? sender, ID3DRenderer e)
   {
      UpdateRenderer();
      SetupEvents();

      OnMapLoaded?.Invoke();
      Selection.LocationSelected += LocationSelectedAddHandler;
      Selection.LocationDeselected += LocationDeselectedAddHandler;
   }
   
   private static readonly Color4 SelectionColor = new (0.5f, 0, 0, 0);
   
   private void LocationSelectedAddHandler(List<Location> locations)
   {
      foreach (var loc in locations)
      {
         if (loc == Location.Empty)
            continue;
         
         _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex] * 0.5f + SelectionColor;
      }

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void LocationDeselectedAddHandler(List<Location> locations)
   {
      foreach (var loc in locations)
         _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex];

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void UpdateRenderer()
   {
      LocationRenderer.SetOrthographicProjection((float)HwndHostContainer.ActualWidth,
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
      return new(mapPoint.X * _imageSize.Item1, mapPoint.Y * _imageSize.Item2);
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
      // TODO: MelCo draw selections:
      Console.WriteLine(pos);
   });

   public Vector2 ScreenToMap(Vector2 screenPoint)
   {
      var width = (float)HwndHostContainer.ActualWidth;
      var height = (float)HwndHostContainer.ActualHeight;
      var aspectRatio = width / height;

      // Get Coordinates from -1 to 1
      var ndcX = screenPoint.X / width * 2.0f - 1.0f;
      var ndcY = 1.0f - screenPoint.Y / height * 2.0f;
      var zoomRatio = _imageAspectRatio / (LocationRenderer.Zoom * 2);
      var worldX = ndcX * (aspectRatio * zoomRatio);
      var worldY = ndcY * (zoomRatio);

      worldX += LocationRenderer.Pan.X;
      worldY += (1f - LocationRenderer.Pan.Y) * _imageAspectRatio;

      var mapX = worldX;
      var mapY = 1.0f - worldY / _imageAspectRatio;

      return new(mapX, mapY);
   }

   public Vector2 AbsoluteMapToNDC(Vector2 mapPoint)
   {
      var worldX = mapPoint.X / _imageSize.Item1;
      var worldY = (1.0f - mapPoint.Y / _imageSize.Item2) * _imageAspectRatio;

      // Apply inverse of pan
      worldX -= LocationRenderer.Pan.X;
      worldY -= (1f - LocationRenderer.Pan.Y) * _imageAspectRatio;

      // Convert world coordinates to NDC
      var width = (float)HwndHostContainer.ActualWidth;
      var height = (float)HwndHostContainer.ActualHeight;
      var aspectRatio = width / height;

      var zoomRatio = _imageAspectRatio / (LocationRenderer.Zoom * 2);

      var ndcX = worldX / (aspectRatio * zoomRatio);
      var ndcY = worldY / zoomRatio;
      return new(ndcX, ndcY);
   }

   public void PanTo(float x, float y)
   {
      LocationRenderer.Pan.X = Math.Clamp(x, -0.1f, 1.1f);
      LocationRenderer.Pan.Y = Math.Clamp(y, -0.1f, 1.1f);
      UpdateRenderer();
   }

   public void PanTo(Vector2 position)
   {
      PanTo(position.X, position.Y);
   }

   public void EnsureVisible(RectangleF area)
   {
      var areaAspectRatio = area.Width / area.Height;
      var controlAspectRatio = (float)HwndHostContainer.ActualWidth / (float)HwndHostContainer.ActualHeight;

      float requiredZoom;

      if (areaAspectRatio > controlAspectRatio)
         requiredZoom = controlAspectRatio / areaAspectRatio;
      else
         requiredZoom = area.Height / area.Width * controlAspectRatio;

      requiredZoom /= _imageAspectRatio;
      requiredZoom *= 2f; // Because orthographic projection is from -1 to 1

      LocationRenderer.Zoom = requiredZoom;

      var panX = area.X + area.Width / 2f;
      var panY = 1f - (area.Y + area.Height / 2f);

      PanTo(panX, panY);
   }

   public void EnsureVisibleWithMargin(RectangleF area, float marginFraction)
   {
      var marginX = area.Width * marginFraction;
      var marginY = area.Height * marginFraction;

      var expandedArea = new RectangleF(area.X - marginX,
                                        area.Y - marginY,
                                        area.Width + 2 * marginX,
                                        area.Height + 2 * marginY);

      EnsureVisible(expandedArea);
   }

   #region Events

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

   private void OnMouseWheel(object sender, MouseWheelEventArgs e)
   {
      var pos = ScreenToMap(e.GetPosition(this));

      if (UpdateZoomLevel(e, pos, out var delta))
         return;

      var newPan = pos - delta;
      PanTo(newPan.X, newPan.Y);
   }

   private void OnMouseMove(object sender, MouseEventArgs e)
   {
      if (_isPanning)
      {
         HandleMousePanning(e);
         return;
      }

      UpdateCursorLocation(e.GetPosition(HwndHostContainer));
      HandleMouseSelection(e);
   }

   private void UpdateCursorLocation(Point position)
   {
      CurrentPos = ScreenToMapAbsolute(position);
      OnAbsolutePositionChanged?.Invoke(CurrentPos);
   }

   private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
   {
      SelectionModeStarting(e, sender);
   }

   private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
   {
      // If we were panning, do not process selection
      if (!_hasPanned)
         SelectionModeTermination(e);

      ResetPanningState(e);
   }

   #endregion

   #region Selection Logic

   private void SelectionModeStarting(MouseButtonEventArgs e, object sender)
   {
      switch (e.ChangedButton)
      {
         case MouseButton.Left:
            switch (Keyboard.Modifiers)
            {
               // Unused
               case ModifierKeys.None:
                  if (sender is not IInputElement surface)
                     return;

                  InitializePanning(surface, e);
                  break;
               case ModifierKeys.Control:
                  break;
               // Enter Rectangle Selection Mode
               case ModifierKeys.Shift:
                  Selection.StartRectangleSelection(CurrentPos);
                  break;
               // Enter Lasso Selection Mode
               case ModifierKeys.Alt:
                  Selection.StartLassoSelection(CurrentPos);
                  break;
            }

            break;
         case MouseButton.Right:
            switch (Keyboard.Modifiers)
            {
               // WILL_BE: Default smart context menu
               case ModifierKeys.None:
                  break;
               // WILL_BE: Smart Selection Menu
               case ModifierKeys.Control:
                  break;
               // WILL_BE: Invert collection for scope x (e.g. Continent)
               case ModifierKeys.Shift:
                  break;
               // Unused
               case ModifierKeys.Alt:
                  break;
            }

            break;
      }
   }


   private void SetSelectionRectangle()
   {
      // In map coordinates
      var upperLeft = Selection.DragPath.First();
      var lowerRight = Selection.DragPath.Last();
      
      //TODO: @Melco Optimize this to cache the data and do not instantiate new arrays every frame
      var topLeftNdc = AbsoluteMapToNDC(new Vector2(upperLeft.X, upperLeft.Y));
      var bottomRightNdc = AbsoluteMapToNDC(new Vector2(lowerRight.X, lowerRight.Y));

      Vector2[] rectangleNdc = [
         new(topLeftNdc.X, topLeftNdc.Y),
         new(bottomRightNdc.X, topLeftNdc.Y),
         new(bottomRightNdc.X, bottomRightNdc.Y),
         new(topLeftNdc.X, bottomRightNdc.Y),
         new(topLeftNdc.X, topLeftNdc.Y)
      ];
      
      LocationRenderer.UpdateSelectionOutline(rectangleNdc, false);
      _d3dHost.Invalidate();
      // Convert to NDC coordinates
   }

   private void HandleMouseSelection(MouseEventArgs e)
   {
      if (e.LeftButton == MouseButtonState.Pressed)
         switch (Keyboard.Modifiers)
         {
            case ModifierKeys.Shift:
               Selection.UpdateDragSelection(CurrentPos, true, false);
               SetSelectionRectangle();
               break;
            case ModifierKeys.Alt:
               Selection.UpdateDragSelection(CurrentPos, true, true);
               break;
            case ModifierKeys.None:

               if (LocationRenderer.ClearSelectionOutline())
               {
                  LocationRenderer.Render();
               }
               
               break;
         }
   }

   private void SelectionModeTermination(MouseButtonEventArgs e)
   {
      switch (e.ChangedButton)
      {
         case MouseButton.Left:
            switch (Keyboard.Modifiers)
            {
               // Simple LMB Click selection
               case ModifierKeys.None:
                  //TODO: @Melco Basically everything here needs to be reworked to support proper selection rendering
                  Selection.DragArea = RectangleF.Empty;
                  Selection.DragPath.Clear();
                  LocationRenderer.ClearSelectionOutline();
                  if (!Selection.GetLocation(CurrentPos, out var location1))
                     return;

                  Selection.Modify(SelectionTarget.Selection,
                                   SelectionMethod.Simple,
                                   [location1],
                                   true,
                                   false,
                                   true);
                  break;
               // Simple LMB Click selection with inversion
               case ModifierKeys.Control:
                  if (!Selection.GetLocation(CurrentPos, out var location2))
                     return;

                  Selection.Modify(SelectionTarget.Selection,
                                   SelectionMethod.Simple,
                                   [location2],
                                   true);
                  break;
               case ModifierKeys.Shift:
                  LocationRenderer.ClearSelectionOutline();
                  Selection.EndRectangleSelection(CurrentPos);
                  
                  break;
               case ModifierKeys.Alt:
                  Selection.EndLassoSelection(CurrentPos);
                  break;
            }

            break;
         // Unused
         case MouseButton.Right:
            switch (Keyboard.Modifiers)
            {
               // Unused
               case ModifierKeys.None:
                  break;
               // Unused
               case ModifierKeys.Control:
                  break;
               // Unused
               case ModifierKeys.Shift:
                  break;
               // Unused
               case ModifierKeys.Alt:
                  break;
            }

            break;
      }
   }

   #endregion

   #region Internal Panning

   public event Action? OnPanningStarted;
   public event Action? OnPanningEnded;

   private void InitializePanning(IInputElement surface, MouseButtonEventArgs e)
   {
      _isPanning = true;
      _lastMousePosition = e.GetPosition(surface);
   }

   private void HandleMousePanning(MouseEventArgs e)
   {
      if (!_hasPanned &&
          (e.LeftButton != MouseButtonState.Pressed ||
           (!(Math.Abs(e.GetPosition(HwndHostContainer).X - _lastMousePosition.X) >
              SystemParameters.MinimumHorizontalDragDistance) &&
            !(Math.Abs(e.GetPosition(HwndHostContainer).Y - _lastMousePosition.Y) >
              SystemParameters.MinimumVerticalDragDistance))))
         return;

      _hasPanned = true;

      OnPanningStarted?.Invoke();

      Mouse.OverrideCursor = Cursors.ScrollAll;

      var currentMousePosition = e.GetPosition(HwndHostContainer);
      var delta = currentMousePosition - _lastMousePosition;

      var aspectRatio = (float)(HwndHostContainer.ActualWidth / HwndHostContainer.ActualHeight);

      var x = LocationRenderer.Pan.X -
              (float)(delta.X / (HwndHostContainer.ActualWidth * LocationRenderer.Zoom)) *
              _imageAspectRatio *
              aspectRatio;
      var y = LocationRenderer.Pan.Y -
              (float)(delta.Y / (HwndHostContainer.ActualHeight * LocationRenderer.Zoom));

      PanTo(x, y);

      _lastMousePosition = currentMousePosition;
   }

   private void ResetPanningState(MouseButtonEventArgs e)
   {
      if (!_isPanning)
         return;

      Mouse.OverrideCursor = null;
      _isPanning = false;
      if (!_hasPanned)
         return;

      e.Handled = true;
      _hasPanned = false;

      OnPanningEnded?.Invoke();
   }

   #endregion

   private bool UpdateZoomLevel(MouseWheelEventArgs e, Vector2 pos, out Vector2 delta)
   {
      var zoomFactor = e.Delta > 0 ? ZOOM_STEP : 1 / ZOOM_STEP;

      var maxZoom = MathF.Exp(Config.Settings.MapSettings.MaxZoomLevel * LnZoomLogBase);
      var minZoom = MathF.Exp(Config.Settings.MapSettings.MinZoomLevel * LnZoomLogBase);

      var newZoom = LocationRenderer.Zoom * zoomFactor;

      if (newZoom < minZoom || maxZoom < newZoom)
      {
         delta = Vector2.Zero;
         return true;
      }

      LocationRenderer.Zoom = newZoom;

      delta = new(pos.X - LocationRenderer.Pan.X, pos.Y - LocationRenderer.Pan.Y);

      delta /= zoomFactor;
      return false;
   }
}