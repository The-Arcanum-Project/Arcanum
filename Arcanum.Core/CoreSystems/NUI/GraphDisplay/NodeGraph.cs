using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public class NodeGraph
{
   private static readonly Brush ForeColorBrush =
      Application.Current.TryFindResource("DefaultForeColorBrush") as Brush ?? Brushes.DarkGray;

   public List<GraphNode> Nodes { get; set; } = [];
   public List<Edge> Edges { get; set; } = [];

   public void AddNode(GraphNode node)
   {
      Nodes.Add(node);
   }

   public void AddEdge(GraphNode source, GraphNode target)
   {
      Edges.Add(new(source, target));
   }

   public void Clear()
   {
      Nodes.Clear();
      Edges.Clear();
   }

   public void ApplyLayout(float width, float height, int iterations = 100)
   {
      GraphLayout.ApplyForceDirectedLayout(Nodes, Edges, width, height, iterations);
   }

   public void DrawToCanvas(Canvas canvas)
   {
      canvas.Children.Clear();

      // Ensure nodes are measured first to get actual text block sizes
      // and therefore accurate node visual sizes for arrowhead calculation.
      // We'll create temporary TextBlocks to measure, then use that info.
      // This is a common WPF pattern for dynamic sizing.
      var nodeVisualData = new Dictionary<GraphNode, (double Width, double Height, TextBlock TextElement)>();

      foreach (var node in Nodes)
      {
         if (node.Label == null)
            node.Label = new()
            {
               Text = node.Name,
               FontSize = 12,
               Foreground = ForeColorBrush,
               TextWrapping = TextWrapping.Wrap,
            };
         else
            node.Label.Text = node.Name;

         node.Label.Measure(new(double.PositiveInfinity, double.PositiveInfinity));

         var textWidth = node.Label.DesiredSize.Width + 10;
         var textHeight = node.Label.DesiredSize.Height + 10;

         var nodeWidth = Math.Max(60, textWidth);
         var nodeHeight = Math.Max(30, textHeight);

         nodeVisualData[node] = (nodeWidth, nodeHeight, node.Label);
      }

      foreach (var edge in Edges)
      {
         var sourceData = nodeVisualData[edge.Source];
         var targetData = nodeVisualData[edge.Target];

         var linePoints = CalculateEdgeLinePoints(edge.Source,
                                                  sourceData.Width,
                                                  sourceData.Height,
                                                  edge.Target,
                                                  targetData.Width,
                                                  targetData.Height);

         // 1. Draw the main line
         var line = new Line
         {
            X1 = linePoints.Start.X,
            Y1 = linePoints.Start.Y,
            X2 = linePoints.End.X,
            Y2 = linePoints.End.Y,
            Stroke = ForeColorBrush,
            StrokeThickness = 1,
         };
         canvas.Children.Add(line);

         // Calculate and draw the arrowhead
         // Pass the *actual end point* of the line, which is at the target node's border
         DrawArrowhead(canvas, linePoints.Start, linePoints.End, ForeColorBrush, 1);
      }

      // Draw nodes and their labels
      foreach (var node in Nodes)
      {
         var (nodeWidth, nodeHeight, text) = nodeVisualData[node];

         // Node Visual (e.g., Rectangle)
         var rect = new Rectangle
         {
            Width = nodeWidth,
            Height = nodeHeight,
            Fill = Brushes.DimGray,
            Stroke = Brushes.Purple,
            StrokeThickness = 1.5,
            RadiusX = 5,
            RadiusY = 5,
         };

         // Center the rectangle around the node's X,Y
         Canvas.SetLeft(rect, node.X - rect.Width / 2);
         Canvas.SetTop(rect, node.Y - rect.Height / 2);
         canvas.Children.Add(rect);

         // Text Label
         text.TextAlignment = TextAlignment.Center;
         text.VerticalAlignment = VerticalAlignment.Center;
         text.Width = nodeWidth;
         text.Height = nodeHeight;

         // Calculate vertical offset for text to truly center within the node's allocated height
         // DesiredSize.Height gives the *content* height, nodeHeight is the *allocated* height
         var verticalOffset = (nodeHeight - text.DesiredSize.Height) / 2;

         // Position text block to align with the node rectangle
         Canvas.SetLeft(text, node.X - nodeWidth / 2);
         Canvas.SetTop(text, node.Y - nodeHeight / 2 + verticalOffset);
         canvas.Children.Add(text);
      }
   }

   /// <summary>
   /// Calculates the intersection point of a line segment with a rectangle,
   /// given the line segment starts at P1 and goes towards the center of the rectangle R.
   /// </summary>
   private static Point GetRectangleIntersectionPoint(Point p1, Point rectCenter, double rectWidth, double rectHeight)
   {
      // Define rectangle boundaries
      var left = rectCenter.X - rectWidth / 2;
      var right = rectCenter.X + rectWidth / 2;
      var top = rectCenter.Y - rectHeight / 2;
      var bottom = rectCenter.Y + rectHeight / 2;

      // Vector from P1 to rectangle center
      var direction = new Vector2((float)(rectCenter.X - p1.X), (float)(rectCenter.Y - p1.Y));
      if (direction.Length() == 0)
         return rectCenter; // Should not happen with valid nodes

      // Check intersection with each of the four lines of the rectangle
      // A line segment is defined by p1 and p2 = rectCenter
      // We need to find the point on the rectangle border that lies on the line passing through p1 and rectCenter.

      // Candidate intersection points
      Point? intersection = null;
      var minDistance = double.MaxValue;

      // Top line
      if (LineIntersectsLine(p1, rectCenter, new(left, top), new(right, top), out var hitTop))
      {
         var dist = Distance(p1, hitTop);
         if (dist < minDistance)
         {
            intersection = hitTop;
            minDistance = dist;
         }
      }

      // Bottom line
      if (LineIntersectsLine(p1, rectCenter, new(left, bottom), new(right, bottom), out var hitBottom))
      {
         var dist = Distance(p1, hitBottom);
         if (dist < minDistance)
         {
            intersection = hitBottom;
            minDistance = dist;
         }
      }

      // Left line
      if (LineIntersectsLine(p1, rectCenter, new(left, top), new(left, bottom), out var hitLeft))
      {
         var dist = Distance(p1, hitLeft);
         if (dist < minDistance)
         {
            intersection = hitLeft;
            minDistance = dist;
         }
      }

      // Right line
      if (!LineIntersectsLine(p1, rectCenter, new(right, top), new(right, bottom), out var hitRight))
         // Return intersection or center if no intersection found (e.g., P1 is inside rect)
         return intersection ?? rectCenter;

      {
         var dist = Distance(p1, hitRight);
         if (dist < minDistance)
            intersection = hitRight;
      }

      // Return intersection or center if no intersection found (e.g., P1 is inside rect)
      return intersection ?? rectCenter;
   }

   private static bool LineIntersectsLine(Point p1, Point p2, Point p3, Point p4, out Point intersection)
   {
      intersection = new(0, 0);

      // Denominators for the intersection formula
      var den = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
      if (den == 0)
         return false; // Lines are parallel or collinear

      var t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / den;
      var u = -((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / den;

      // Check if intersection lies within both line segments (important for rectangle edges)
      if (t is < 0 or > 1 || u is < 0 or > 1)
         return false;

      intersection.X = p1.X + t * (p2.X - p1.X);
      intersection.Y = p1.Y + t * (p2.Y - p1.Y);
      return true;
   }

   // Simple distance helper
   private static double Distance(Point p1, Point p2)
   {
      var dx = p1.X - p2.X;
      var dy = p1.Y - p2.Y;
      return Math.Sqrt(dx * dx + dy * dy);
   }

   /// <summary>
   /// Calculates the start and end points for an edge line, ensuring they connect to the borders of the nodes.
   /// </summary>
   private (Point Start, Point End) CalculateEdgeLinePoints(GraphNode sourceNode,
                                                            double sourceWidth,
                                                            double sourceHeight,
                                                            GraphNode targetNode,
                                                            double targetWidth,
                                                            double targetHeight)
   {
      var sourceCenter = new Point(sourceNode.X, sourceNode.Y);
      var targetCenter = new Point(targetNode.X, targetNode.Y);

      // Find the intersection point on the target node's border
      var endPoint = GetRectangleIntersectionPoint(sourceCenter, targetCenter, targetWidth, targetHeight);

      // Find the intersection point on the source node's border
      // We trace the line from the *target* towards the *source* to find the correct start point
      var startPoint = GetRectangleIntersectionPoint(targetCenter, sourceCenter, sourceWidth, sourceHeight);

      return (startPoint, endPoint);
   }

   /// <summary>
   /// Draws an arrowhead at the target end of a line.
   /// </summary>
   /// <param name="canvas">The canvas to draw on.</param>
   /// <param name="lineStart">The actual start point of the line at the source node's border.</param>
   /// <param name="lineEnd">The actual end point of the line at the target node's border (where arrow tip should be).</param>
   /// <param name="strokeBrush">The brush for the arrowhead.</param>
   /// <param name="strokeThickness">The thickness of the arrowhead outline.</param>
   private static void DrawArrowhead(Canvas canvas,
                                     Point lineStart,
                                     Point lineEnd,
                                     Brush strokeBrush,
                                     double strokeThickness)
   {
      const double arrowLength = 10;
      const double arrowWidth = 5;

      // Vector from start to end of the line
      var delta = new Vector2((float)(lineEnd.X - lineStart.X), (float)(lineEnd.Y - lineStart.Y));
      var distance = delta.Length();

      if (distance < arrowLength)
         return; // Don't draw arrowhead if line is too short

      var direction = Vector2.Normalize(delta);

      // Perpendicular vector for arrowhead wings
      var perp = new Vector2(-direction.Y, direction.X); // Rotate 90 degrees

      // The arrow tip is precisely at lineEnd
      var arrowTip = lineEnd;

      var arrowBase = new Point(arrowTip.X - direction.X * arrowLength,
                                arrowTip.Y - direction.Y * arrowLength);

      var wing1 = new Point(arrowBase.X + perp.X * arrowWidth,
                            arrowBase.Y + perp.Y * arrowWidth);

      var wing2 = new Point(arrowBase.X - perp.X * arrowWidth,
                            arrowBase.Y - perp.Y * arrowWidth);

      var arrowhead = new Polygon
      {
         Points = [arrowTip, wing1, wing2], // Arrow tip is the first point
         Fill = strokeBrush,
         Stroke = strokeBrush,
         StrokeThickness = strokeThickness,
         StrokeLineJoin = PenLineJoin.Round,
      };

      canvas.Children.Add(arrowhead);
   }
}