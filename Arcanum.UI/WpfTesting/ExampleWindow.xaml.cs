using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.Parsing.MapParsing;
using Point = System.Windows.Point;
using Polygon = Arcanum.Core.CoreSystems.Parsing.MapParsing.Polygon;

namespace Arcanum.UI.WpfTesting;

public class VisualHost : FrameworkElement
{
   private readonly VisualCollection _visuals;

   public VisualHost()
   {
      _visuals = new VisualCollection(this);
   }

   public void AddVisual(DrawingVisual visual)
   {
      _visuals.Add(visual);
   }

   public void ClearVisuals()
   {
      _visuals.Clear();
   }

   protected override int VisualChildrenCount => _visuals.Count;

   protected override Visual GetVisualChild(int index)
   {
      if (index < 0 || index >= _visuals.Count)
      {
         throw new ArgumentOutOfRangeException(nameof(index));
      }
      return _visuals[index];
   }
}

public partial class ExampleWindow : IDebugDrawer
{
   public UserSettings CurrentSettings { get; } = new();
   private double _currentScale = 1.0;
   private const double ZoomFactor = 1.1;
   private readonly VisualHost _polygonHost;
   
   public ExampleWindow()
   {
      InitializeComponent();
      _polygonHost = new VisualHost();
      drawingCanvas.Children.Add(_polygonHost);
      LoadMap();
      
   }
   private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
   {
      Point mousePosition = e.GetPosition(MainScrollViewer);
      double oldScale = _currentScale;

      if (e.Delta > 0)
         _currentScale *= ZoomFactor;
      else
         _currentScale /= ZoomFactor;

      _currentScale = Math.Max(0.1, Math.Min(10.0, _currentScale));

      CanvasScaleTransform.ScaleX = _currentScale;
      CanvasScaleTransform.ScaleY = _currentScale;

      double newHorizontalOffset = (MainScrollViewer.HorizontalOffset + mousePosition.X) * _currentScale / oldScale - mousePosition.X;
      double newVerticalOffset = (MainScrollViewer.VerticalOffset + mousePosition.Y) * _currentScale / oldScale - mousePosition.Y;

      MainScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
      MainScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
   }
   private void DrawLine_Click(object sender, RoutedEventArgs e)
   {
      Line line = new Line
      {
         X1 = 50,
         Y1 = 50,
         X2 = 200,
         Y2 = 150,
         Stroke = Brushes.Black,
         StrokeThickness = 2
      };

      drawingCanvas.Children.Add(line);
   }
   private Point? _lastDragPoint;
   private void Window_MouseDown(object sender, MouseButtonEventArgs e)
   {
      if (e.MiddleButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
      {
         _lastDragPoint = e.GetPosition(MainScrollViewer);
         Cursor = Cursors.SizeAll;
      }
   }

   private void Window_MouseUp(object sender, MouseButtonEventArgs e)
   {
      _lastDragPoint = null;
      Cursor = Cursors.Arrow;
      MainScrollViewer.ReleaseMouseCapture();
   }

   private void Window_MouseMove(object sender, MouseEventArgs e)
   {
      if (_lastDragPoint.HasValue)
      {
         Point currentPos = e.GetPosition(MainScrollViewer);
         double dX = currentPos.X - _lastDragPoint.Value.X;
         double dY = currentPos.Y - _lastDragPoint.Value.Y;

         MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - dX);
         MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - dY);

         _lastDragPoint = currentPos;
      }
   }
   
   private static Ellipse Circle => new()
   {
      Width = 2,
      Height = 2,
      Fill = Brushes.Red
   };

   private Brush _currentBush = GetRandomBrush();
   private const int MARGIN = 0;
   private const int SCALE = 4;
   private static Point ConvertCoordinates(int x, int y)
   {
      return new Point(x * (SCALE - 1) + 0.5 + MARGIN, y * (SCALE - 1) + 0.5 + MARGIN);
   }
   
   private void LoadMap()
   {
      BitmapImage bitmapImage = new BitmapImage(new Uri("D:\\SteamLibrary\\steamapps\\common\\Project Caesar Review\\game\\in_game\\map_data\\locations.png", UriKind.RelativeOrAbsolute));
      FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, null, 0);
      
      
      int width = formattedBitmap.PixelWidth;
      int height = formattedBitmap.PixelHeight;
      int stride = width * 4;
      byte[] pixels = new byte[height * stride];
      formattedBitmap.CopyPixels(pixels, stride, 0);

      _polygonHost.ClearVisuals();
      var childrenToRemove = drawingCanvas.Children.OfType<UIElement>().Where(c => c != _polygonHost).ToList();
      foreach (var child in childrenToRemove)
      {
         drawingCanvas.Children.Remove(child);
      }
      int pixelSize = 2;
      int margin = 1;
      int step = pixelSize + margin;
/*
      for (int y = 0; y < height; y++)
      {
         for (int x = 0; x < width; x++)
         {
            int index = y * stride + x * 4;
            byte b = pixels[index];
            byte g = pixels[index + 1];
            byte r = pixels[index + 2];
            byte a = pixels[index + 3];

            if (a == 0) continue; // Skip transparent pixels

            Rectangle rect = new Rectangle
            {
               Width = pixelSize,
               Height = pixelSize,
               Fill = new SolidColorBrush(Color.FromArgb(a, r, g, b))
            };

            Canvas.SetLeft(rect, 1 + x * step);
            Canvas.SetTop(rect, 1 + y * step);
            drawingCanvas.Children.Add(rect);
         }
      }
*/
      width = 1 + width * step;
      height = 1 + height * step;
      drawingCanvas.MaxWidth = width;
      drawingCanvas.MaxHeight = height;
      Debug.WriteLine($"Loaded image with dimensions: {width}x{step}");
      drawingCanvas.Width = width;
      drawingCanvas.Height = height;
   }
   private static readonly Random _rand = new Random();
   public static SolidColorBrush GetRandomBrush()
   {
      byte r = (byte)_rand.Next(0, 256);
      byte g = (byte)_rand.Next(0, 256);
      byte b = (byte)_rand.Next(0, 256);

      return new SolidColorBrush(Color.FromRgb(r, g, b));
   }
   public void DrawLine(int x1, int y1, int x2, int y2)
   {
      var p1 = ConvertCoordinates(x1, y1);
      var p2 = ConvertCoordinates(x2, y2);
      
      var line = new Line
      {
         X1 = p1.X,
         Y1 = p1.Y,
         X2 = p2.X,
         Y2 = p2.Y,
         Stroke = _currentBush,
         StrokeThickness = 1,
      };
      drawingCanvas.Children.Add(line);
      Panel.SetZIndex(line,1);
   }

   /*public void DrawPolygon(Polygon polygon)
   {
      var points = polygon.GetAllPoints().Select(p => ConvertCoordinates(p.X, p.Y)).ToList();
      if (points.Count < 2) return;

      // Create a lightweight DrawingVisual to hold the polygon's rendering.
      var drawingVisual = new DrawingVisual();

      // Obtain the DrawingContext to issue drawing commands.
      using (DrawingContext dc = drawingVisual.RenderOpen())
      {
         // Use StreamGeometry for the most efficient geometry representation.
         var streamGeometry = new StreamGeometry();
         using (StreamGeometryContext sgc = streamGeometry.Open())
         {
            sgc.BeginFigure(points[0], true, true); // isFilled, isClosed
            sgc.PolyLineTo(points.Skip(1).ToList(), true, false); // isStroked, isSmoothJoin
         }

         // Freeze resources for a significant performance boost.
         streamGeometry.Freeze();
         var fillBrush = GetRandomBrush();
         fillBrush.Freeze();
         var strokePen = new Pen(Brushes.Black, 1);
         strokePen.Freeze();

         // Draw the geometry onto the visual.
         dc.DrawGeometry(fillBrush, strokePen, streamGeometry);
      }

      // Add the finished visual to our host control.
      _polygonHost.AddVisual(drawingVisual);
   }*/

   private int count = 0;
   
   public void DrawPolygon(Polygon polygon)
   {
      throw new NotImplementedException();
      // 1. Tesselate the polygon to get vertices and triangle indices.
      var (vertices, indices) = polygon.Tesselate();

      if (vertices.Count == 0 || indices.Count == 0) return;
      
      // Convert all vertex coordinates at once.
      //var points = vertices.Select(p => ConvertCoordinates(p.X, p.Y)).ToList();
      var points = new List<Point>(vertices.Count);
      // Create a lightweight DrawingVisual to hold the polygon's rendering.
      var drawingVisual = new DrawingVisual();

      // Obtain the DrawingContext to issue drawing commands.
      using (DrawingContext dc = drawingVisual.RenderOpen())
      {
         // Use StreamGeometry for the most efficient geometry representation. [8]
         var streamGeometry = new StreamGeometry();
         using (StreamGeometryContext sgc = streamGeometry.Open())
         {
            // 2. Iterate through the indices to define each triangle.
            for (int i = 0; i < indices.Count; i += 3)
            {
               count++;
               sgc.BeginFigure(points[indices[i]], true, true); // Start the first vertex of the triangle
               sgc.LineTo(points[indices[i + 1]], true, false); // Draw a line to the second vertex
               sgc.LineTo(points[indices[i + 2]], true, false); // Draw a line to the third vertex
            }
         }

         // Freeze resources for a significant performance boost.
         streamGeometry.Freeze();
         var fillBrush = GetRandomBrush();
         fillBrush.Freeze();
         var strokePen = new Pen(Brushes.Black, 1);
         strokePen.Freeze();

         // Draw the geometry onto the visual.
         dc.DrawGeometry(fillBrush, strokePen, streamGeometry);
      }

      // Add the finished visual to our host control.
      _polygonHost.AddVisual(drawingVisual);
      Console.WriteLine($"Drew polygon with {vertices.Count} vertices and {indices.Count / 3} triangles. Total triangles drawn: {count}");
   }

   public void DrawNode(int x, int y, int color = unchecked((int)0xFFFF0000))
   {
      var circle = Circle;
      if (color != unchecked((int)0xFFFF0000))
      {
         var colorVar = Color.FromArgb(
            (byte)((color >> 24) & 0xFF), // Alpha
            (byte)((color >> 16) & 0xFF), // Red
            (byte)((color >> 8) & 0xFF),  // Green
            (byte)(color & 0xFF)          // Blue
         );
         circle.Fill = new SolidColorBrush(colorVar);
      }

      Canvas.SetLeft(circle, ConvertCoordinates(x, y).X - circle.Width / 2);
      Canvas.SetTop(circle, ConvertCoordinates(x, y).Y - circle.Height / 2);

      drawingCanvas.Children.Add(circle);
      Panel.SetZIndex(circle,2);
      _currentBush = GetRandomBrush();
   }
}

public enum SomeValues
{
   Value1,
   Value2,
   Value3,
   Value4,
   Value5
}

public enum SomeOtherValues
{
   OptionA,
   OptionB,
   OptionC,
   OptionD,
   OptionE
}

public class UserSettings
{
   public string FavoriteColor { get; set; } = "Blue";
   public string ProfilePicturePath { get; set; } = "/Assets/profile_picture.png";
   public string Bio { get; set; } = "This is a sample bio for the user.";
   public string Email { get; set; } = "this.example@web.de";
   public string PhoneNumber { get; set; } = "+1234567890";
   [Description("The user's account balance in USD.")]
   public double AccountBalance { get; set; } = 1000.50;
   //public DateTime DateOfBirth { get; set; } = new(1993, 5, 15); // We will have our own date implementation in the future so no need to do this one here.
   public List<string> Hobbies { get; set; } = ["Reading", "Gaming", "Traveling", "Cooking"];

   [Description("This is a list of Point objects.")]
   public List<Point> Coordinates { get; set; } = [new(1.0, 2.0), new(3.0, 4.0), new(5.0, 6.0)];
   [Description("This is a list of Point objects with a description.")]
   public List<List<Point>> NestedCoordinates { get; set; } =
   [
      new List<Point> { new(1.0, 2.0), new(3.0, 4.0) }, new List<Point> { new(5.0, 6.0), new(7.0, 8.0) }
   ];

   [Description("This is a complex object containing various properties.")]
   public ComplexObject ComplexData { get; set; } = new();
}

public class ComplexObject
{
   public string Line { get; set; } = "===============================";
   public Point[] ArrayCoordinates { get; set; } = [new(1.0, 2.0), new(3.0, 4.0), new(5.0, 6.0)];
   public Point[][] NestedArrayCoordinates { get; set; } =
   [
      new[] { new Point(1.0, 2.0), new Point(3.0, 4.0) }, new[] { new Point(5.0, 6.0), new Point(7.0, 8.0) }
   ];
   public string SLine { get; set; } = "===============================";
   [Description("This is a list of Point arrays.")]
   public List<Point[]> ListOfPointArrays { get; set; } =
   [
      new[] { new Point(1.0, 2.0), new Point(3.0, 4.0) }, new[] { new Point(5.0, 6.0), new Point(7.0, 8.0) }
   ];
   [Description("The user's height in meters.")]
   public float Height { get; set; } = 1.75f; // in meters
   [Description("The user's weight in kilograms.")]
   public SomeValues SelectedValue { get; set; } = SomeValues.Value2;
   public SomeOtherValues SelectedOption { get; set; } = SomeOtherValues.OptionB;
   [Description("The user's username.")]
   public string Username { get; set; } = "User123";
   public bool IsAdmin { get; set; } = true;
   [Description("The user's age in years.")]
   public int Age { get; set; } = 30;
   [Description("The user's account balance in USD.")]
   public double AccountBalance { get; set; } = 1000.50;
}