using System.Numerics;

namespace Arcanum.Core.CoreSystems.NUI.GraphDisplay;

public static class GraphLayout
{
   public static void ApplyGraphLayout(NodeGraph graph, float width, float height, int iterations = 500)
   {
      ApplyForceDirectedLayout(graph.Nodes, graph.Edges, width, height, iterations);
   }

   public static void ApplyForceDirectedLayout(List<GraphNode> nodes,
                                               List<Edge> edges,
                                               float width,
                                               float height,
                                               int iterations = 500)
   {
      if (nodes.Count == 0)
         return;

      if (nodes.Count <= 4)
      {
         HandleSmallGraphLayout(nodes, edges, width, height);
         PostLayoutLayout(nodes, width, height); // Still center it
         return;
      }

      // Parameters
      var k = (float)Math.Sqrt(width * height / nodes.Count);
      var temperature = width / 10; // Initial temperature

      var rand = new Random();

      // Random positions
      foreach (var node in nodes)
      {
         node.X = rand.NextSingle() * width;
         node.Y = rand.NextSingle() * height;
         node.Displacement = new(0, 0);
      }

      // Iterative Force Calculation
      for (var i = 0; i < iterations; i++)
      {
         // Reset displacements for this iteration
         foreach (var node in nodes)
            node.Displacement = new(0, 0);

         // a. Calculate Repulsive Forces
         for (var u = 0; u < nodes.Count; u++)
         {
            for (var v = u + 1; v < nodes.Count; v++) // Avoid recalculating pairs and self-repulsion
            {
               var nodeU = nodes[u];
               var nodeV = nodes[v];

               var delta = new Vector2(nodeV.X - nodeU.X, nodeV.Y - nodeU.Y);
               var distance = delta.Magnitude();
               if (distance < 0.01) // Prevent division by zero, give a small random push
               {
                  distance = 0.01f;
                  delta = new(rand.NextSingle() * 0.1f, rand.NextSingle() * 0.1f);
               }

               var force = k * k / distance; // F_rep = k^2 / d
               var forceVec = delta.Normalize() * force;

               nodeU.Displacement -= forceVec;
               nodeV.Displacement += forceVec;
            }
         }

         // b. Calculate Attractive Forces
         foreach (var edge in edges)
         {
            var nodeU = edge.Source;
            var nodeV = edge.Target;

            var delta = new Vector2(nodeV.X - nodeU.X, nodeV.Y - nodeU.Y);
            var distance = delta.Magnitude();
            if (distance < 0.01)
               distance = 0.01f; // Prevent division by zero

            var force = distance * distance / k; // F_att = d^2 / k
            var forceVec = delta.Normalize() * force;

            nodeU.Displacement += forceVec;
            nodeV.Displacement -= forceVec;
         }

         // c. Apply Displacements and Cool Down
         foreach (var node in nodes)
         {
            var dispMagnitude = node.Displacement.Magnitude();
            if (dispMagnitude > 0)
            {
               // Limit displacement to 'temperature' to ensure stability
               var limitedDisplacement = node.Displacement.Normalize() * Math.Min(dispMagnitude, temperature);

               node.X += limitedDisplacement.X;
               node.Y += limitedDisplacement.Y;

               // Clamp positions to canvas boundaries
               node.X = Math.Max(0, Math.Min(width, node.X));
               node.Y = Math.Max(0, Math.Min(height, node.Y));
            }
         }

         // Cool down the temperature
         temperature *= 1.0f - (float)i / iterations; // Linear cooling
         if (temperature < 0.1)
            temperature = 0.1f; // Prevent temperature from becoming too low too fast
      }

      // Post-layout adjustment: Center the graph within the canvas
      PostLayoutLayout(nodes, width, height);
   }

   /// <summary>
   /// Handles specialized layouts for very small graphs (2-4 nodes).
   /// </summary>
   private static void HandleSmallGraphLayout(List<GraphNode> nodes, List<Edge> edges, float width, float height)
   {
      // Define a base spacing unit. This ensures small graphs are not too cramped or too spread out.
      var spacing = Math.Min(width, height) / (nodes.Count + 1);

      switch (nodes.Count)
      {
         case 2:
            // Simple horizontal or vertical alignment
            // If there's an edge, draw it horizontally.
            if (edges.Count > 0)
            {
               // Source on left, Target on right
               nodes[0].X = width / 2 - spacing / 2;
               nodes[0].Y = height / 2;
               nodes[1].X = width / 2 + spacing / 2;
               nodes[1].Y = height / 2;
            }
            else // Two disconnected nodes
            {
               nodes[0].X = width / 2 - spacing / 2;
               nodes[0].Y = height / 2 - spacing / 4;
               nodes[1].X = width / 2 + spacing / 2;
               nodes[1].Y = height / 2 + spacing / 4;
            }

            break;

         case 3:
            // Arrange in a triangle for general case, or a line for a simple chain.
            // Let's go for a simple triangular pattern.
            nodes[0].X = width / 2;
            nodes[0].Y = height / 2 - spacing; // Top node

            nodes[1].X = width / 2 - spacing;
            nodes[1].Y = height / 2 + spacing; // Bottom-left

            nodes[2].X = width / 2 + spacing;
            nodes[2].Y = height / 2 + spacing; // Bottom-right
            break;

         case 4:
            // Arrange in a square/diamond pattern
            nodes[0].X = width / 2 - spacing;
            nodes[0].Y = height / 2 - spacing / 2;

            nodes[1].X = width / 2 + spacing;
            nodes[1].Y = height / 2 - spacing / 2;

            nodes[2].X = width / 2 - spacing;
            nodes[2].Y = height / 2 + spacing / 2;

            nodes[3].X = width / 2 + spacing;
            nodes[3].Y = height / 2 + spacing / 2;
            break;
      }
   }

   private static void PostLayoutLayout(List<GraphNode> nodes, float width, float height)
   {
      if (nodes.Count == 0)
         return;

      // Find min/max X and Y to determine the bounding box of the laid-out graph
      var minX = nodes.Min(n => n.X);
      var minY = nodes.Min(n => n.Y);
      var maxX = nodes.Max(n => n.X);
      var maxY = nodes.Max(n => n.Y);

      var graphWidth = maxX - minX;
      var graphHeight = maxY - minY;

      // Calculate current center of the graph
      var currentCenterX = minX + graphWidth / 2;
      var currentCenterY = minY + graphHeight / 2;

      // Calculate target center of the canvas
      var targetCenterX = width / 2;
      var targetCenterY = height / 2;

      // Calculate translation needed to move graph center to canvas center
      var translateX = targetCenterX - currentCenterX;
      var translateY = targetCenterY - currentCenterY;

      // Optional: Scale the graph to fit within the canvas if it's too large or too small
      var scaleX = width / (graphWidth + 20); // Add padding
      var scaleY = height / (graphHeight + 20); // Add padding
      var scale = Math.Min(scaleX, scaleY); // Use the smaller scale to fit both dimensions

      // Don't scale if the graph is already small enough, or if it makes it tiny
      if (scale > 1.0)
         scale = 1.0f; // Don't enlarge if it fits
      if (scale < 0.5 && nodes.Count > 1)
         scale = 0.5f; // Prevent it from becoming too small

      // If scale is applied, nodes need to be scaled relative to the graph's center
      // For now, let's just focus on translation first, as scaling can add complexity to rendering.
      // If you scale, you would do:
      // node.X = (node.X - currentCenterX) * scale + targetCenterX;
      // node.Y = (node.Y - currentCenterY) * scale + targetCenterY;

      // Apply translation to all nodes
      foreach (var node in nodes)
      {
         node.X += translateX;
         node.Y += translateY;

         // Ensure nodes are still within *overall* canvas bounds after centering,
         // if the graph was wider than the canvas (e.g., if there's no scaling)
         node.X = Math.Max(0, Math.Min(width, node.X));
         node.Y = Math.Max(0, Math.Min(height, node.Y));
      }
   }

   private static Vector2 Normalize(this Vector2 v)
   {
      var length = v.Length();
      return length > 0 ? v / length : new(0, 0);
   }

   private static float Magnitude(this Vector2 v)
   {
      return v.Length();
   }
}