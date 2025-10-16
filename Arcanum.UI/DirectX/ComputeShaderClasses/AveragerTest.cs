using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

public static class AveragerTest
{
   public static void RunTest()
   {
      const int rectCount = 4096 * 4;

      Console.WriteLine("Generating sample locations...");
      var random = new Random();
      using var image =
         SixLabors.ImageSharp.Image
                  .Load<
                      Rgba32>("C:\\Users\\david\\source\\repos\\Arcanum\\Arcanum.UI\\Assets\\Images\\ProvinceFileCreator1024x1024.png");
      var locations = new GpuRect[rectCount];
      var imageWidth = image.Width;
      var imageHeight = image.Height;

      var gridSize = (int)Math.Ceiling(Math.Sqrt(rectCount));

      var cellWidth = imageWidth / gridSize;
      var cellHeight = imageHeight / gridSize;
      var estimatedJitter = cellHeight * 0.2;
      var maxJitter = (int)Math.Max(estimatedJitter, 1);
      maxJitter = 0;

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

      using var runner = new GpuComputeRunner();

      // 1. Create and upload all GPU resources using the runner's helpers.
      //    Wrap them in 'using' blocks to ensure they are disposed.
      // using var gpuTextureSrv = runner.CreateShaderResourceView(gpuTexture);
      //
      // using var gpuBoundsBuffer = runner.CreateStructuredBuffer<GpuRect>(locations);
      // using var gpuBoundsSrv = runner.CreateShaderResourceView(gpuBoundsBuffer);

      // using var gpuOutputBuffer = runner.CreateReadWriteStructuredBuffer<Float4>(rectCount);
      // var threadGroupsX = (uint)Math.Ceiling(rectCount / 64.0);
      // Console.WriteLine("Mapping colors using GPU...");
      //
      // // 3. Execute the shader.
      // var results = runner.Execute<Float4>(shaderPath: "AverageColor.hlsl", // Or your custom resource path
      //                                      entryPoint: "main",
      //                                      threadGroupsX: threadGroupsX,
      //                                      threadGroupsY: 1,
      //                                      threadGroupsZ: 1,
      //                                      outputBuffer: gpuOutputBuffer,
      //                                      shaderResourceViews: [gpuTextureSrv, gpuBoundsSrv]);

      // --- SLOW HISTOGRAM ANALYSIS TEST ---

      // using var gpuTexture = runner.CreateTexture2D(image);
      // var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      // using var gpuOutputBuffer = runner.CreateReadWriteStructuredBuffer<TopTwoColors>(rectCount);
      //
      // Console.WriteLine("Finding top 2 colors using GPU...");
      //
      // // 3. Execute the new shader.
      // var histAnalyzer = new GpuHistogramAnalyzer(runner);
      // var results = histAnalyzer.AnalyzeRects(gpuTexture, locations);
      //
      // stopwatch.Stop();
      // Console.WriteLine($"Processing complete in {stopwatch.ElapsedMilliseconds} ms.");

      // --- FAST HISTOGRAM ANALYSIS TEST ---

      // using var gpuTexture = runner.CreateTexture2D(image);
      // using var gpuTextureSrv = runner.CreateShaderResourceView(gpuTexture);
      //
      // using var gpuBoundsBuffer = runner.CreateStructuredBuffer<GpuRect>(locations);
      // using var gpuBoundsSrv = runner.CreateShaderResourceView(gpuBoundsBuffer);
      //
      // using var gpuOutputBuffer = runner.CreateReadWriteStructuredBuffer<TopTwoColors>(rectCount);
      //
      // // --- THIS IS THE NEW DISPATCH LOGIC ---
      // // We dispatch ONE thread group PER RECTANGLE.
      //
      // Console.WriteLine("Finding top 2 colors using GPU (single dispatch)...");
      // var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      //
      // // Now we call the generic Execute method just ONCE.
      // var results = runner.Execute<TopTwoColors>("TopTwoColors.hlsl", // The new, correct shader
      //                                            "main",
      //                                            rectCount,
      //                                            1,
      //                                            1,
      //                                            gpuOutputBuffer,
      //                                            gpuTextureSrv,
      //                                            gpuBoundsSrv);
      //
      // stopwatch.Stop();
      // Console.WriteLine($"Processing complete in {stopwatch.ElapsedMilliseconds} ms.");

      using var analyzer = new GpuMultiPassHistogram(runner);

      Console.WriteLine("Finding top 2 colors using multi-pass GPU analysis...");
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      // The single call that does all the work.
      var results = analyzer.Analyze(image, locations);

      stopwatch.Stop();
      Console.WriteLine($"Processing complete in {stopwatch.ElapsedMilliseconds} ms.");

      // RenderRectanglesSingleColor(locations,
      //                             locations,
      //                             results,
      //                             image);

      RenderGradientRects(locations,
                          image,
                          results);
   }

   private static void RenderGradientRects(GpuRect[] locations,
                                           Image<Rgba32> image,
                                           TopTwoColors[] results)
   {
      using var renderer = new GpuRenderer();
      Console.WriteLine("Rendering gradient rectangles to output_gradient.png...");
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      // --- 1. Prepare resources for the GRADIENT render task ---
      var vsByteCode = ID3DRenderer.CompileBytecodeBlob("DrawGradientRects.hlsl", "VS", "vs_5_0");
      using var vertexShader = renderer.Device.CreateVertexShader(vsByteCode);
      var psByteCode = ID3DRenderer.CompileBytecode("DrawGradientRects.hlsl", "PS", "ps_5_0");
      using var pixelShader = renderer.Device.CreatePixelShader(psByteCode.Span);

      // Create Buffers
      var quadVertices = new Vertex[]
      {
         new() { Position = new(0, 0) }, // Top-left
         new() { Position = new(1, 0) }, // Top-right
         new() { Position = new(0, 1) }, // Bottom-left
         new() { Position = new(1, 1) }, // Bottom-right
      };
      var quadIndices = new ushort[] { 0, 1, 2, 2, 1, 3 };
      using var vertexBuffer = renderer.CreateBuffer<Vertex>(quadVertices, BindFlags.VertexBuffer);
      using var indexBuffer = renderer.CreateBuffer<ushort>(quadIndices, BindFlags.IndexBuffer);

      // --- THIS IS THE KEY CHANGE: Populate the new instance data struct ---
      var instanceData = new GradientInstanceData[locations.Length];
      for (int i = 0; i < locations.Length; i++)
      {
         instanceData[i] = new()
         {
            Offset = new(locations[i].Left, locations[i].Top),
            Size = new(locations[i].Right - locations[i].Left, locations[i].Bottom - locations[i].Top),
            Color1 = results[i].MostFrequent, // Use the first color
            Color2 = results[i].SecondMostFrequent // Use the second color
         };
      }

      using var instanceBuffer = renderer.CreateBuffer<GradientInstanceData>(instanceData, BindFlags.VertexBuffer);

      using var constantBuffer = renderer.CreateConstantBuffer((uint)Marshal.SizeOf<Constants>());
      renderer.UpdateBuffer(constantBuffer,
                            new Constants { RenderTargetSize = new(image.Width, image.Height) });

      // --- Define the new Input Layout that matches GradientInstanceData ---
      var inputElements = new[]
      {
         new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
         // Per-Instance Data from slot 1
         new InputElementDescription("INSTANCE_OFFSET",
                                     0,
                                     Format.R32G32_Float,
                                     0,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
         new InputElementDescription("INSTANCE_SIZE",
                                     0,
                                     Format.R32G32_Float,
                                     8,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
         new InputElementDescription("INSTANCE_COLOR",
                                     0,
                                     Format.R32G32B32A32_Float,
                                     16,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
         new InputElementDescription("INSTANCE_COLOR",
                                     1,
                                     Format.R32G32B32A32_Float,
                                     32,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1) // NEW: Second color at offset 32
      };

      // Create Render Target
      using var renderTargetTexture = renderer.CreateRenderTarget((uint)image.Width, (uint)image.Height);
      using var renderTargetView = renderer.Device.CreateRenderTargetView(renderTargetTexture);

      // --- 2. Call the generic Render method with the new layout ---
      renderer.Render(vsByteCode,
                      renderTargetView,
                      vertexShader,
                      pixelShader,
                      inputElements, // Pass the new layout
                      [vertexBuffer, instanceBuffer],
                      [(uint)Marshal.SizeOf<Vertex>(), (uint)Marshal.SizeOf<GradientInstanceData>()],
                      indexBuffer,
                      constantBuffer,
                      PrimitiveTopology.TriangleList,
                      (uint)quadIndices.Length,
                      (uint)locations.Length);

      // 3. Save the result
      renderer.ReadbackTextureAndSave(renderTargetTexture, "output_gradient.png");

      stopwatch.Stop();
      Console.WriteLine($"Rendering complete in {stopwatch.ElapsedMilliseconds} ms.");
   }

   private static void RenderRectanglesSingleColor(GpuRect[] locations,
                                                   GpuRect[] rects,
                                                   Float4[] colors,
                                                   Image<Rgba32> image)
   {
      using var renderer = new GpuRenderer();

      Console.WriteLine("Rendering rectangles to output.png...");
      var stopwatch = System.Diagnostics.Stopwatch.StartNew();

      // --- 1. Prepare all resources for this specific render task ---

      // Compile Shaders
      var vsByteCode = ID3DRenderer.CompileBytecode("DrawRects.hlsl", "VS", "vs_5_0");
      using var vertexShader = renderer.Device.CreateVertexShader(vsByteCode.Span); // Use the renderer's device
      var psByteCode = ID3DRenderer.CompileBytecode("DrawRects.hlsl", "PS", "ps_5_0");
      using var pixelShader = renderer.Device.CreatePixelShader(psByteCode.Span);

      // Create Buffers
      var quadVertices = new Vertex[]
      {
         new() { Position = new(0, 0) }, // Top-left
         new() { Position = new(1, 0) }, // Top-right
         new() { Position = new(0, 1) }, // Bottom-left
         new() { Position = new(1, 1) }, // Bottom-right
      };
      var quadIndices = new ushort[] { 0, 1, 2, 2, 1, 3 };
      using var vertexBuffer = renderer.CreateBuffer<Vertex>(quadVertices, BindFlags.VertexBuffer);
      using var indexBuffer = renderer.CreateBuffer<ushort>(quadIndices, BindFlags.IndexBuffer);

      var instanceData = new InstanceData[locations.Length];
      for (var i = 0; i < locations.Length; i++)
      {
         instanceData[i] = new()
         {
            Offset = new(rects[i].Left, rects[i].Top),
            Size = new(rects[i].Right - rects[i].Left, rects[i].Bottom - rects[i].Top),
            Color = colors[i],
         };
      }

      using var instanceBuffer = renderer.CreateBuffer<InstanceData>(instanceData, BindFlags.VertexBuffer);

      using var constantBuffer = renderer.CreateConstantBuffer((uint)Marshal.SizeOf<Constants>());
      renderer.UpdateBuffer(constantBuffer,
                            new Constants { RenderTargetSize = new(image.Width, image.Height) });

      // Create Input Layout
      var inputElements = new[]
      {
         // Per-Vertex Data (from vertexBuffer in slot 0)
         new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
         // Per-Instance Data (from instanceBuffer in slot 1)
         new InputElementDescription("INSTANCE_OFFSET",
                                     0,
                                     Format.R32G32_Float,
                                     0,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
         new InputElementDescription("INSTANCE_SIZE",
                                     0,
                                     Format.R32G32_Float,
                                     8,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
         new InputElementDescription("INSTANCE_COLOR",
                                     0,
                                     Format.R32G32B32A32_Float,
                                     16,
                                     1,
                                     InputClassification.PerInstanceData,
                                     1),
      };

      // Create Render Target
      using var renderTargetTexture = renderer.CreateRenderTarget((uint)image.Width, (uint)image.Height);
      using var renderTargetView = renderer.Device.CreateRenderTargetView(renderTargetTexture);

      // --- 2. Call the generic Render method ---

      var vertexBlob = new Blob(Marshal.AllocHGlobal(vsByteCode.Length));

      renderer.Render(vertexBlob,
                      renderTargetView,
                      vertexShader,
                      pixelShader,
                      inputElements,
                      [vertexBuffer, instanceBuffer],
                      [(uint)Marshal.SizeOf<Vertex>(), (uint)Marshal.SizeOf<InstanceData>()],
                      indexBuffer,
                      constantBuffer,
                      PrimitiveTopology.TriangleList,
                      (uint)quadIndices.Length,
                      (uint)locations.Length);

      // --- 3. Save the result ---

      renderer.ReadbackTextureAndSave(renderTargetTexture, "output.png");

      stopwatch.Stop();
      Console.WriteLine($"Rendering complete in {stopwatch.ElapsedMilliseconds} ms.");
      Console.WriteLine("Done. Check for output.png in your execution directory.");
   }
}