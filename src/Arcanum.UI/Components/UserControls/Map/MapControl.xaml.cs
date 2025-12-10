using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Behaviors;
using Arcanum.UI.DirectX;
using Arcanum.UI.MapInteraction;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using Vortice.Mathematics;
using Point = System.Windows.Point;

namespace Arcanum.UI.Components.UserControls.Map;

// What should the map support?
// - Pan and Zoom with Mouse Wheel
// - Select single with click
// - select rectangle with click and drag
// - ctrl rectangle -> remove (only check at end)
// - select lasso with click and drag while holding Alt
// - ctrl lasso -> remove (only check at end)
// - select with circle brush while holding C (Diameter with mouse wheel)
// - ctrl circle brush -> remove
// - circle brush can either be click to apply or click and drag to apply continuously
// - click with alt for upper scope and alt shift for lower scope
public partial class MapControl
{
   private static readonly Color4 SelectionColor = new(0.5f, 0, 0, 0);
   private static readonly Color4 FreezeSelectionColor = new(0, 0, 0.5f, 0);
   private static readonly Color4 PreviewColor = new(0.5f, 0.5f, 0, 0);

   private D3D11HwndHost _d3dHost = null!;
   public LocationRenderer LocationRenderer { get; private set; } = null!;

   public readonly MapInteractionManager MapInteractionManager;

   public MapCoordinateConverter Coords { get; private set; } = null!;

   private Color4[] _currentBackgroundColor = null!;
   private Color4[] _selectionColor = null!;
   public event Action<Vector2>? OnAbsolutePositionChanged;

   public event Action<MapClickEventArgs>? OnMapClick;

   public static event Action? OnMapLoaded;

   // Get to pass this into the color generation
   public Color4[] CurrentBackgroundColors => _currentBackgroundColor;

   public static readonly DependencyProperty CurrentPosProperty =
      DependencyProperty.Register(nameof(CurrentPos), typeof(Vector2), typeof(MapControl), new(default(Vector2)));

   public Vector2 CurrentPos
   {
      get => (Vector2)GetValue(CurrentPosProperty);
      set => SetValue(CurrentPosProperty, value);
   }

   // Command to load the project in the Arcanum view
   public ICommand DoubleClickCommand => new RelayCommand<MouseButtonEventArgs>(_ => { });

   public MapControl()
   {
      MapInteractionManager = new(this);
      InitializeComponent();
   }

   private void SetupEvents()
   {
      HwndHostContainer.MouseWheel += OnMouseWheel;
      HwndHostContainer.MouseDown += OnMouseMiddleButtonDown;
      HwndHostContainer.PreviewMouseUp += OnPreviewMouseUp;
      HwndHostContainer.MouseMove += OnMouseMove;
      HwndHostContainer.MouseEnter += (_, _) => Window.GetWindow(this)!.KeyDown += MapInteractionManager.HandleKeyDown;
      HwndHostContainer.MouseLeave += (_, _) => Window.GetWindow(this)!.KeyDown -= MapInteractionManager.HandleKeyDown;

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

   public void UpdateColors()
   {
      _selectionColor = (_currentBackgroundColor.Clone() as Color4[])!;
      RefreshSelectionColors();
      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private static Color4[] CreateColors()
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

      Coords = new(this, imageSize);

      var vertices = await Task.Run(() => LocationRenderer.CreateVertices(polygons, imageSize));
      var startColor = CreateColors();
      LocationRenderer = new(vertices, startColor, Coords.ImageAspectRatio);
      _currentBackgroundColor = startColor;
      _selectionColor = (Color4[])_currentBackgroundColor.Clone();
      _d3dHost = new(LocationRenderer, HwndHostContainer, OnRendererLoaded);
      HwndHostContainer.Child = _d3dHost;

      SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
      SelectionManager.PreviewChanged += RefreshAndRenderSelectionColors;

      DataContext = _d3dHost;
      LoadingPanel.Visibility = Visibility.Collapsed;
   }

   private void SelectionManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if (e.PropertyName != nameof(SelectionManager.ObjectSelectionMode))
         return;

      RefreshAndRenderSelectionColors();
   }

   private void OnRendererLoaded(object? sender, ID3DRenderer e)
   {
      UpdateRenderer();
      SetupEvents();

      OnMapLoaded?.Invoke();
      Selection.LocationSelected += LocationSelectedAddHandler;
      Selection.LocationDeselected += LocationDeselectedAddHandler;
   }

   private void LocationSelectedAddHandler(List<Location> locations)
   {
      RefreshSelectionColors();

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   public void RefreshAndRenderSelectionColors()
   {
      Array.Copy(_currentBackgroundColor, _selectionColor, _currentBackgroundColor.Length);
      RefreshSelectionColors();
      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void RefreshSelectionColors()
   {
      if (SelectionManager.ObjectSelectionMode == ObjectSelectionMode.Frozen)
         foreach (var loc in SelectionManager.GetActiveSelectionLocations().Where(loc => loc != Location.Empty))
         {
            _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex] * 0.5f + FreezeSelectionColor;
         }

      foreach (var loc in Selection.GetSelectedLocations)
      {
         _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex] * 0.5f + SelectionColor;
      }

      foreach (var loc in SelectionManager.PreviewedLocations)
      {
         _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex] * 0.5f + PreviewColor;
      }
   }

   private void LocationDeselectedAddHandler(List<Location> locations)
   {
      foreach (var loc in locations)
         _selectionColor[loc.ColorIndex] = _currentBackgroundColor[loc.ColorIndex];

      RefreshSelectionColors();

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void UpdateRenderer()
   {
      LocationRenderer.SetOrthographicProjection((float)HwndHostContainer.ActualWidth,
                                                 (float)HwndHostContainer.ActualHeight);
      _d3dHost.Invalidate();
   }

   public void PanTo(float x, float y)
   {
      LocationRenderer.Pan.X = Math.Clamp(x, -0.1f, 1.1f);
      LocationRenderer.Pan.Y = Math.Clamp(y, -0.1f, 1.1f);
      UpdateRenderer();
   }

   public void PanToWithView(float x, float y)
   {
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

      requiredZoom /= Coords.ImageAspectRatio;
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

   private void OnMouseWheel(object sender, MouseWheelEventArgs e)
   {
      MapInteractionManager.HandleMouseWheel(e);
   }

   private void OnMouseMove(object sender, MouseEventArgs e)
   {
      UpdateCursorLocation(e.GetPosition(HwndHostContainer));
      MapInteractionManager.HandleMouseMove(e);
   }

   private void UpdateCursorLocation(Point position)
   {
      Coords.InvalidateCache(position);
      OnAbsolutePositionChanged?.Invoke(CurrentPos);
   }

   private void OnMouseMiddleButtonDown(object sender, MouseButtonEventArgs e)
   {
      MapInteractionManager.HandleMouseDown(e);
   }

   private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
   {
      MapInteractionManager.HandleMouseUp(e);
      OnMapClick?.Invoke(new(CurrentPos, e.ChangedButton));
   }

   #endregion

   private void Border_ContextMenuOpening(object sender, ContextMenuEventArgs e)
   {
      if (Keyboard.Modifiers != ModifierKeys.None)
         e.Handled = true;
   }
}