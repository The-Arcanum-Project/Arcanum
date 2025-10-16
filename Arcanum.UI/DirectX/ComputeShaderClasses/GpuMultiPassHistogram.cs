using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

public class GpuMultiPassHistogram
   : IDisposable
{
   private readonly GpuComputeRunner _runner;
   private const int HISTOGRAM_SIZE = 32768; // R5G5B5

   public GpuMultiPassHistogram(GpuComputeRunner runner)
   {
      _runner = runner;
   }

   public TopTwoColors[] Analyze(Image<Rgba32> image, GpuRect[] rects)
   {
      var device = _runner.Device;
      var context = _runner.Context;
      var rectCount = (uint)rects.Length;

      // --- 1. Compile all shaders ---
      using var clearShader =
         device.CreateComputeShader(ID3DRenderer
                                   .CompileBytecode("Compute.Histogram.ClearHistogram.hlsl", "main", "cs_5_0")
                                   .Span);
      using var voteShader =
         device.CreateComputeShader(ID3DRenderer
                                   .CompileBytecode("Compute.Histogram.VoteOnGlobalHistogram.hlsl", "main", "cs_5_0")
                                   .Span);
      using var reduceShader =
         device.CreateComputeShader(ID3DRenderer
                                   .CompileBytecode("Compute.Histogram.ReduceHistograms.hlsl", "main", "cs_5_0")
                                   .Span);

      // --- 2. Create all resources ---
      using var texture = _runner.CreateTexture2D(image);
      using var textureSrv = _runner.CreateShaderResourceView(texture);

      using var boundsBuffer = _runner.CreateStructuredBuffer<GpuRect>(rects);
      using var boundsSrv = _runner.CreateShaderResourceView(boundsBuffer);

      // This is our giant histogram buffer. It needs to be both UAV and SRV.
      using var histogramBuffer = _runner.CreateReadWriteStructuredBuffer<uint>((int)(rectCount * HISTOGRAM_SIZE));
      using var histogramSrv = _runner.CreateShaderResourceView(histogramBuffer);
      using var histogramUav = _runner.CreateUnorderedAccessView(histogramBuffer);

      using var outputBuffer = _runner.CreateReadWriteStructuredBuffer<TopTwoColors>((int)rectCount);
      using var outputUav = _runner.CreateUnorderedAccessView(outputBuffer);

      // --- 3. Execute the 3-pass pipeline ---

      // == PASS 1: Clear the global histogram ==
      context.CSSetShader(clearShader);
      context.CSSetUnorderedAccessView(0, histogramUav);
      var clearGroups = (uint)Math.Ceiling(rectCount * HISTOGRAM_SIZE / 256.0);
      context.Dispatch(clearGroups, 1, 1);
      context.CSSetUnorderedAccessView(0, null); // Unbind

      // == PASS 2: Vote on the global histogram ==
      context.CSSetShader(voteShader);
      context.CSSetShaderResources(0, [textureSrv, boundsSrv]);
      context.CSSetUnorderedAccessView(0, histogramUav);
      context.Dispatch(rectCount, 1, 1); // One group per rectangle
      context.CSSetUnorderedAccessView(0, null); // Unbind

      // == PASS 3: Reduce the histograms to find the top 2 ==
      context.CSSetShader(reduceShader);
      context.CSSetShaderResource(0, histogramSrv); // Histogram is now an INPUT (SRV)
      context.CSSetUnorderedAccessView(0, outputUav); // Final results are the OUTPUT (UAV)
      context.Dispatch(rectCount, 1, 1); // One group per rectangle
      context.CSSetUnorderedAccessView(0, null); // Unbind

      context.CSSetShader(null); // Cleanup
      context.CSSetShaderResource(0, null);

      // --- 4. Readback the final result ---
      return _runner.ReadBackBuffer<TopTwoColors>(outputBuffer);
   }

   public void Dispose()
   {
      GC.SuppressFinalize(this);
   }
}