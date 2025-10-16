using System.Windows;
using SixLabors.ImageSharp.PixelFormats;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

public static class AveragerTest
{
   public static void RunTest()
   {
      const int rectCount = 100;

      Console.WriteLine("Generating sample locations...");
      var random = new Random();
      using var image =
         SixLabors.ImageSharp.Image
                  .Load<
                      Rgba32>("C:\\Users\\david\\source\\repos\\Arcanum\\Arcanum.UI\\Assets\\Images\\ProvinceFileCreator1024x1024.png");
      var locations = new GpuRect[rectCount];
      var imageWidth = image.Width;
      var imageHeight = image.Height;

      // split the image into rectCount triangles covering the whole image
      for (var i = 0; i < rectCount; i++)
      {
         var left = (i % 10) * (imageWidth / 10);
         var top = (i / 10) * (imageHeight / 10);
         var right = left + (imageWidth / 10);
         var bottom = top + (imageHeight / 10);

         // Add some random jitter to the rectangle position
         var jitterX = random.Next(-5, 5);
         var jitterY = random.Next(-5, 5);

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

      var sb = new System.Text.StringBuilder();
      // Print some results
      for (var i = 0; i < rectCount; i++)
      {
         var str =
            $"Location {i} ({locations[i]}) Avg Color: R={results[i].R:F2}, G={results[i].G:F2}, B={results[i].B:F2}";
         Console.WriteLine(str);
         sb.AppendLine(str);
      }

      Clipboard.SetText(sb.ToString());
   }
}