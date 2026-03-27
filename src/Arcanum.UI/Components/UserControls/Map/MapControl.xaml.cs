using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.Imagery;
using Arcanum.UI.Components.Behaviors;
using Arcanum.UI.DirectX;
using Arcanum.UI.MapInteraction;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using Vortice.Mathematics;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
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

   public static readonly DependencyProperty CurrentPosProperty =
      DependencyProperty.Register(nameof(CurrentPos), typeof(Vector2), typeof(MapControl), new(default(Vector2)));

   public readonly MapInteractionManager MapInteractionManager;

   private Vector2? _contextMenuClickLocation;

   private D3D11HwndHost _d3dHost = null!;

   private float rot = 0;

   private bool _isMapReady;
   private int _mapHeight = -1;

   private int _mapWidth = -1;
   private Color4[] _selectionColor = null!;

   private bool _isRenderingLoopActive = false;

   public MapControl()
   {
      MapInteractionManager = new(this);
      InitializeComponent();
   }

   public LocationRenderer LocationRenderer { get; private set; } = null!;

   public MapCoordinateConverter Coords { get; private set; } = null!;

   // Get to pass this into the color generation
   public Color4[] CurrentBackgroundColors { get; private set; } = null!;

   public Vector2 CurrentPos
   {
      get => (Vector2)GetValue(CurrentPosProperty);
      set => SetValue(CurrentPosProperty, value);
   }

   // Command to load the project in the Arcanum view
   public ICommand DoubleClickCommand => new RelayCommand<MouseButtonEventArgs>(_ => { });

   public bool HasLocationOwner => Selection.GetLocation(_contextMenuClickLocation) is { } loc && PoliticalMapMode.GetLocationOwner(loc) != Country.Empty;
   public event Action<Vector2>? OnAbsolutePositionChanged;

   public event Action<Location, Vector2>? OnAbsoluteLocationChangedLocation;

   public event Action<MapClickEventArgs>? OnMapClick;

   public static event Action? OnMapLoaded;

   private void SetupEvents()
   {
      HwndHostContainer.MouseWheel += OnMouseWheel;
      HwndHostContainer.MouseDown += OnMouseMiddleButtonDown;
      HwndHostContainer.PreviewMouseUp += OnPreviewMouseUp;
      HwndHostContainer.MouseMove += OnMouseMove;
      HwndHostContainer.MouseEnter += (_, _) => Window.GetWindow(this)!.KeyDown += MapInteractionManager.HandleKeyDown;
      HwndHostContainer.MouseLeave += (_, _) => Window.GetWindow(this)!.KeyDown -= MapInteractionManager.HandleKeyDown;

      OnAbsoluteLocationChangedLocation += (loc, _) =>
      {
         if (!Selection.MapManager.IsMapDataInitialized)
            return;

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
      if (!MapModeManager.IsMapReady)
         return;

      // for (var index = 0; index < CurrentBackgroundColors.Length; index++)
      // {
      //    var color = CurrentBackgroundColors[index];
      //    CurrentBackgroundColors[index] =  new Color4(color.R, color.G, color.B, rot);
      // }
      // rot = Random.Shared.NextSingle() * 2 * float.Pi;
      // rot += float.Pi / 180f;
      // rot %= float.Pi * 2;

      if (_selectionColor.Length != CurrentBackgroundColors.Length)
         _selectionColor = new Color4[CurrentBackgroundColors.Length];
      Array.Copy(CurrentBackgroundColors, _selectionColor, CurrentBackgroundColors.Length);

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
         // Make alpha always 0
         var preColor = location.Color.AsInt();
         preColor &= 0x00FFFFFF;
         var color = new Color4(preColor);

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
      CurrentBackgroundColors = startColor;
      _selectionColor = (Color4[])CurrentBackgroundColors.Clone();
      _d3dHost = new(LocationRenderer, HwndHostContainer, OnRendererLoaded);
      HwndHostContainer.Child = _d3dHost;

      SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
      SelectionManager.PreviewChanged += RefreshAndRenderSelectionColors;

      DataContext = _d3dHost;
      LoadingPanel.Visibility = Visibility.Collapsed;

      _mapWidth = imageSize.Item1;
      _mapHeight = imageSize.Item2;

      SetMapEffect(2);
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

      _isMapReady = true;
   }

   private void LocationSelectedAddHandler(List<Location> locations)
   {
      RefreshSelectionColors();

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   public void RefreshAndRenderSelectionColors()
   {
      Array.Copy(CurrentBackgroundColors, _selectionColor, CurrentBackgroundColors.Length);
      RefreshSelectionColors();
      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void RefreshSelectionColors()
   {
      var frozenFactor = 1f - Config.Settings.MapSettings.FrozenSelectionColorOpacity;
      var selectionFactor = 1f - Config.Settings.MapSettings.SelectionColorOpacity;
      var previewFactor = 1f - Config.Settings.MapSettings.PrviewOpacityFactor;

      if (SelectionManager.ObjectSelectionMode == ObjectSelectionMode.Frozen)
         foreach (var loc in SelectionManager.GetActiveSelectionLocations())
         {
            if (loc == Location.Empty)
               continue;

            _selectionColor[loc.ColorIndex] = CurrentBackgroundColors[loc.ColorIndex] * frozenFactor + FreezeSelectionColor;
         }

      foreach (var loc in Selection.GetSelectedLocations)
         _selectionColor[loc.ColorIndex] = CurrentBackgroundColors[loc.ColorIndex] * selectionFactor + SelectionColor;

      foreach (var loc in SelectionManager.PreviewedLocations)
         _selectionColor[loc.ColorIndex] = CurrentBackgroundColors[loc.ColorIndex] * previewFactor + PreviewColor;
   }

   private void LocationDeselectedAddHandler(List<Location> locations)
   {
      foreach (var loc in locations)
         _selectionColor[loc.ColorIndex] = CurrentBackgroundColors[loc.ColorIndex];

      RefreshSelectionColors();

      LocationRenderer.UpdateColors(_selectionColor);
      _d3dHost.Invalidate();
   }

   private void UpdateRenderer()
   {
      LocationRenderer.SetMousePosition(CurrentPos.X / _mapWidth, Coords.ImageAspectRatio * (1.0f - CurrentPos.Y / _mapHeight));
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

   public void PanToCoords(float x, float y)
   {
      if (_mapWidth <= 0 || _mapHeight <= 0)
         return;

      var coordX = x / _mapWidth;
      var coordY = y / _mapHeight;
      PanTo(coordX, coordY);
   }

   public void PanTo(List<Location> locations)
   {
      if (locations.Count == 0)
         return;

      var boundingBox = locations[0].Bounds;
      for (var i = 1; i < locations.Count; i++)
         boundingBox = RectangleF.Union(boundingBox, locations[i].Bounds);

      var centerX = boundingBox.X + boundingBox.Width / 2f;
      var centerY = boundingBox.Y + boundingBox.Height / 2f;

      PanToCoords(centerX, centerY);
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

   private void Border_ContextMenuOpening(object sender, ContextMenuEventArgs e)
   {
      if (Keyboard.Modifiers != ModifierKeys.None)
         e.Handled = true;
   }

   private void ExportBackColorsToBitmap()
   {
      if (_mapWidth <= 0 || _mapHeight <= 0)
         return;

      using var bmp = new Bitmap(_mapWidth, _mapHeight, PixelFormat.Format24bppRgb);
      var bmpData = bmp.LockBits(new(0, 0, _mapWidth, _mapHeight),
                                 ImageLockMode.WriteOnly,
                                 bmp.PixelFormat);

      var colors = CurrentBackgroundColors;
      var width = _mapWidth;
      var height = _mapHeight;
      var stride = bmpData.Stride;

      var sourcePolygons = ((LocationMapTracing)DescriptorDefinitions.MapTracingDescriptor.LoadingService[0]).Polygons!;

      const int sliceHeight = 32;

      var workItems = new List<RenderTask>(sourcePolygons.Length * 2);

      for (var i = 0; i < sourcePolygons.Length; i++)
      {
         var poly = sourcePolygons[i];

         var complexity = poly.TriangleIndices.Length;
         var polyTop = (int)poly.Bounds.Top;
         var polyBottom = (int)poly.Bounds.Bottom;

         if (polyBottom < 0 || polyTop >= height)
            continue;

         polyTop = Math.Max(0, polyTop);
         polyBottom = Math.Min(height - 1, polyBottom);

         var polyHeight = polyBottom - polyTop;

         if (polyHeight <= sliceHeight)
         {
            // Cost = Complexity * Height
            var cost = (long)complexity * polyHeight;
            workItems.Add(new(poly, polyTop, polyBottom, cost));
         }
         else
            for (var y = polyTop; y <= polyBottom; y += sliceHeight)
            {
               var sliceEnd = Math.Min(y + sliceHeight - 1, polyBottom);
               var sliceH = sliceEnd - y;
               var cost = (long)complexity * sliceH;
               workItems.Add(new(poly, y, sliceEnd, cost));
            }
      }

      workItems.Sort((a, b) => b.EstimatedCost.CompareTo(a.EstimatedCost));

      unsafe
      {
         var scan0 = (byte*)bmpData.Scan0;
         var partitioner = Partitioner.Create(workItems, EnumerablePartitionerOptions.NoBuffering);

         Parallel.ForEach(partitioner,
                          task =>
                          {
                             var polygon = task.Polygon;
                             var color = colors[polygon.ColorIndex];

                             var r = (byte)(color.R * 255);
                             var g = (byte)(color.G * 255);
                             var b = (byte)(color.B * 255);

                             var drawer = new BitmapPixelDrawer(scan0, stride, width, height, r, g, b);
                             polygon.Rasterize(ref drawer, task.YStart, task.YEnd);
                          });
      }

      bmp.UnlockBits(bmpData);
      ImageTagger.ExportTaggedTexture(IO.GetNextAvailableFilePath($"{MapModeManager.GetCurrent().Name}.png", IO.GetMapExportPath), bmp, ImageFormat.Png);
   }

   private void MenuItem_OnClick(object sender, RoutedEventArgs e)
   {
      ExportBackColorsToBitmap();
   }

   private void SelectProvince_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      Selection.Modify(SelectionTarget.Selection, SelectionMethod.Expand, location.Province.Locations, true, false);
   }

   private void SelectArea_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      var locs = location.Province.Area.GetRelevantLocations([location.Province.Area]);
      Selection.Modify(SelectionTarget.Selection, SelectionMethod.Expand, locs, true, false);
   }

   private void SelectRegion_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      Selection.Modify(SelectionTarget.Selection,
                       SelectionMethod.Expand,
                       location.Province.Area.Region.GetRelevantLocations([location.Province.Area.Region]),
                       true,
                       false);
   }

   private void SelectSuperRegion_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      Selection.Modify(SelectionTarget.Selection,
                       SelectionMethod.Expand,
                       location.Province.Area.Region.SubContinent.GetRelevantLocations([location.Province.Area.Region.SubContinent]),
                       true,
                       false);
   }

   private void SelectContinent_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      Selection.Modify(SelectionTarget.Selection,
                       SelectionMethod.Expand,
                       location.Province.Area.Region.SubContinent.Continent.GetRelevantLocations([location.Province.Area.Region.SubContinent.Continent]),
                       true,
                       false);
   }

   private void CopySelectedLocationIds_Click(object sender, RoutedEventArgs e)
   {
      var selectedLocations = SelectionManager.GetActiveSelectionLocations();
      if (selectedLocations.Count == 0)
         return;

      var idList = string.Join(" ", selectedLocations.Select(loc => loc.UniqueId));
      Clipboard.SetText(idList);
   }

   private void SelectOwner_Click(object sender, RoutedEventArgs e)
   {
      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      var owner = PoliticalMapMode.GetLocationOwner(location);
      if (owner == Country.Empty)
         return;

      SelectionManager.Eu5ObjectSelectedInSearch(owner);
   }

   private void MapContextMenu_Opened(object sender, RoutedEventArgs e)
   {
      const string dynamicTag = "dynamic";
      if (!_isMapReady || sender is not ContextMenu contextMenu)
      {
         e.Handled = true;
         return;
      }

      for (int i = contextMenu.Items.Count - 1; i >= 0; i--)
      {
         if (contextMenu.Items[i] is FrameworkElement { Tag: dynamicTag })
         {
            contextMenu.Items.RemoveAt(i);
         }
      }

      _contextMenuClickLocation = CurrentPos;

      // Check if we have options from the currenty map mode we want to add
      var mapModeOptions = MapModeManager.GetCurrent().GetContextMenuOptions();
      if (mapModeOptions != null)
      {
         contextMenu.Items.Add(new Separator() { Tag = dynamicTag, });

         foreach (var option in mapModeOptions)
         {
            var menuItem = new MenuItem
            {
               Header = option.OptionName,
               IsEnabled = option.IsEnabled,
               ToolTip = option.Tooltip(_contextMenuClickLocation ?? Vector2.Zero),
               Tag = dynamicTag,
            };

            menuItem.Click += OnMenuItemOnClick;
            contextMenu.Unloaded += OnUnloaded;
            contextMenu.Items.Add(menuItem);
            continue;

            void OnMenuItemOnClick(object o, RoutedEventArgs routedEventArgs)
            {
               var clickPos = _contextMenuClickLocation ?? Vector2.Zero;
               option.OptionAction(clickPos);
            }

            void OnUnloaded(object o, RoutedEventArgs routedEventArgs)
            {
               menuItem.Click -= OnMenuItemOnClick;
               contextMenu.Unloaded -= OnUnloaded;
            }
         }
      }
   }

   private void CopyMapCoordinates_OnClick(object sender, RoutedEventArgs e)
   {
      if (!_contextMenuClickLocation.HasValue)
         return;

      var subPixelPresicision = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
      var format = subPixelPresicision ? "0.##" : "###";

      if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
      {
         var values = new Dictionary<string, double>
         {
            ["x"] = _contextMenuClickLocation.Value.X, ["y"] = _contextMenuClickLocation.Value.Y,
         };
         Clipboard.SetText(CustomNumberParser.Format(Config.Settings.MiscSettings.CustomCoordinatesFormat, values));
      }
      else if (Keyboard.Modifiers == ModifierKeys.Control)
         Clipboard.SetText($"X:{_contextMenuClickLocation.Value.X.ToString(format)}, Y:{_contextMenuClickLocation.Value.Y.ToString(format)}");
      else
         Clipboard.SetText($"{_contextMenuClickLocation.Value.X.ToString(format)} {_contextMenuClickLocation.Value.Y.ToString(format)}");
   }

   private void MapModeDataCopy_OnClick(object sender, RoutedEventArgs e)
   {
      if (!_contextMenuClickLocation.HasValue)
         return;

      var location = Selection.GetLocation(_contextMenuClickLocation);
      if (location == Location.Empty)
         return;

      var mapMode = MapModeManager.GetCurrent();
      var relatedData = mapMode.GetLocationRelatedData(location);
      if (relatedData == null!)
         return;

      if (relatedData is IEu5Object eu5Object)
         Clipboard.SetText(eu5Object.UniqueId);
      else
      {
         var str = relatedData.ToString();
         if (string.IsNullOrEmpty(str))
            return;

         Clipboard.SetText(str);
      }
   }

   private readonly record struct RenderTask(Polygon Polygon, int YStart, int YEnd, long EstimatedCost);

   private readonly unsafe struct BitmapPixelDrawer(byte* scan0, int stride, int width, int height, byte r, byte g, byte b)
      : IPixelAction
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Invoke(int x, int y)
      {
         if (x < 0 || x >= width || y < 0 || y >= height)
            return;

         var pixelPtr = scan0 + y * stride + x * 3;

         pixelPtr[0] = b;
         pixelPtr[1] = g;
         pixelPtr[2] = r;
      }
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
      var location = Selection.GetLocation(CurrentPos);
      OnAbsoluteLocationChangedLocation?.Invoke(location, CurrentPos);
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

   public void SetMapEffect(int mode)
   {
      LocationRenderer.CurrentEffectMode = mode;

      // Effect mode 0 is "Standard/Off". If mode > 0, we need the loop.
      if (mode > 0)
      {
         if (!_isRenderingLoopActive)
         {
            _isRenderingLoopActive = true;
            CompositionTarget.Rendering += OnRenderingTick;
         }
      }
      else
      {
         // If mode is 0, stop the loop to save CPU/GPU resources
         if (_isRenderingLoopActive)
         {
            _isRenderingLoopActive = false;
            CompositionTarget.Rendering -= OnRenderingTick;
         }

         // Force one last update to reset to standard view
         UpdateRenderer();
      }
   }

   private void OnRenderingTick(object? sender, EventArgs e)
   {
      // This ensures that even if the mouse is still, 
      // the shader gets updated Time and Mouse coordinates every frame.
      UpdateRenderer();
   }
}