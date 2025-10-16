using System.Windows;
using SixLabors.ImageSharp.PixelFormats;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

public static class AveragerTest
{
   public static void RunTest()
   {
      const int rectCount = 4096 * 4;

      Console.WriteLine("Generating sample locations...");
      var random = new Random();
      using var image =
         SixLabors.ImageSharp.Image.Load<Rgba32>("C:\\Users\\david\\Bilder\\MV.png");
      var locations = new GpuRect[rectCount];
      var imageWidth = image.Width;
      var imageHeight = image.Height;

      var gridSize = (int)Math.Ceiling(Math.Sqrt(rectCount));

      var cellWidth = imageWidth / gridSize;
      var cellHeight = imageHeight / gridSize;
      var estimatedJitter = cellHeight * 0.2;
      var maxJitter = (int)Math.Max(estimatedJitter, 1);

      for (var i = 0; i < rectCount; i++)
      {
         // 3. Use the dynamic 'gridSize' instead of the hard-coded '10'.
         var left = i % gridSize * cellWidth;
         var top = i / gridSize * cellHeight;
         var right = left + cellWidth;
         var bottom = top + cellHeight;

         // Your jitter logic is perfectly fine and can remain as is.
         var jitterX = random.Next(-maxJitter, maxJitter);
         var jitterY = random.Next(-maxJitter, maxJitter);

         locations[i] = new()
         {
            Left = Math.Clamp(left + jitterX, 0, imageWidth - 1),
            Top = Math.Clamp(top + jitterY, 0, imageHeight - 1),
            Right = Math.Clamp(right + jitterX, 1, imageWidth),
            Bottom = Math.Clamp(bottom + jitterY, 1, imageHeight)
         };
      }

      using var mapper = new GpuLocationColorMapper();

      Console.WriteLine("Mapping colors using GPU...");
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      var results = mapper.MapColorsToLocations(image, locations);

      stopwatch.Stop();
      Console.WriteLine($"Processing complete in {stopwatch.ElapsedMilliseconds} ms.");

      using var renderer = new GpuRectangleRenderer();
      Console.WriteLine("Rendering rectangles to output.png...");
      stopwatch.Restart();
      renderer.RenderRectanglesToFile("output.png", image.Width, image.Height, locations, results);
      stopwatch.Stop();
      Console.WriteLine($"Rendering complete in {stopwatch.ElapsedMilliseconds} ms.");

      Console.WriteLine("Done. Check for output.png in your execution directory.");
   }
}