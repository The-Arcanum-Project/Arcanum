using Vortice.Direct3D11;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

public class GpuHistogramAnalyzer : IDisposable
{
   private readonly GpuComputeRunner _runner;
   private const int HISTOGRAM_SIZE = 32768;

   public GpuHistogramAnalyzer(GpuComputeRunner runner)
   {
      _runner = runner;
   }

   public TopTwoColors[] AnalyzeRects(ID3D11Texture2D texture, GpuRect[] rects)
   {
      // 1. Create a single, global histogram buffer on the GPU.
      using var histogramBuffer = _runner.CreateReadWriteStructuredBuffer<uint>(HISTOGRAM_SIZE);

      // --- Shader Compilation ---
      var clearByteCode = ID3DRenderer.CompileBytecode("Compute.ClearHistogram.hlsl", "main", "cs_5_0");
      using var clearShader = _runner.Device.CreateComputeShader(clearByteCode.Span);

      var buildByteCode = ID3DRenderer.CompileBytecode("Compute.BuildHistogram.hlsl", "main", "cs_5_0");
      using var buildShader = _runner.Device.CreateComputeShader(buildByteCode.Span);

      // --- Other resources ---
      using var textureSrv = _runner.Device.CreateShaderResourceView(texture);
      using var histogramUav = _runner.Device.CreateUnorderedAccessView(histogramBuffer);
      using var constantBuffer = _runner.CreateConstantBuffer(16); // For one Rect

      var results = new TopTwoColors[rects.Length];

      // --- Process each rectangle one by one ---
      for (var i = 0; i < rects.Length; i++)
      {
         var rect = rects[i];

         // A. Clear the histogram buffer.
         _runner.Context.CSSetShader(clearShader);
         _runner.Context.CSSetUnorderedAccessView(0, histogramUav);
         var clearGroups = (uint)Math.Ceiling(HISTOGRAM_SIZE / 256.0);
         _runner.Context.Dispatch(clearGroups, 1, 1);
         _runner.Context.CSSetUnorderedAccessView(0, null); // Unbind

         // B. Build the histogram for the current rectangle.
         var rectWidth = rect.Right - rect.Left;
         var rectHeight = rect.Bottom - rect.Top;
         if (rectWidth <= 0 || rectHeight <= 0)
            continue;

         // Update constant buffer with the current rect
         _runner.UpdateBuffer(constantBuffer, rect);

         _runner.Context.CSSetShader(buildShader);
         _runner.Context.CSSetShaderResource(0, textureSrv);
         _runner.Context.CSSetUnorderedAccessView(0, histogramUav);
         _runner.Context.CSSetConstantBuffer(0, constantBuffer);

         var buildGroupsX = (uint)Math.Ceiling(rectWidth / 16.0);
         var buildGroupsY = (uint)Math.Ceiling(rectHeight / 16.0);
         _runner.Context.Dispatch(buildGroupsX, buildGroupsY, 1);
         _runner.Context.CSSetUnorderedAccessView(0, null); // Unbind

         // C. Readback histogram and find top 2 on CPU.
         var cpuHistogram = _runner.ReadBackBuffer<uint>(histogramBuffer);
         results[i] = FindTopTwoOnCpu(cpuHistogram);
      }

      return results;
   }

   private TopTwoColors FindTopTwoOnCpu(uint[] histogram)
   {
      uint top_idx1 = 0,
           top_idx2 = 0;
      uint top_cnt1 = 0,
           top_cnt2 = 0;

      for (uint i = 0; i < histogram.Length; i++)
      {
         var count = histogram[i];
         if (count > top_cnt1)
         {
            top_cnt2 = top_cnt1;
            top_idx2 = top_idx1;
            top_cnt1 = count;
            top_idx1 = i;
         }
         else if (count > top_cnt2)
         {
            top_cnt2 = count;
            top_idx2 = i;
         }
      }

      if (top_cnt2 == 0)
         top_idx2 = top_idx1;

      return new() { MostFrequent = DequantizeColor(top_idx1), SecondMostFrequent = DequantizeColor(top_idx2) };
   }

   // Helper to dequantize on CPU
   private Float4 DequantizeColor(uint index)
   {
      var r = (index >> 10) & 0x1F;
      var g = (index >> 5) & 0x1F;
      var b = index & 0x1F;

      return new()
      {
         R = r / 31.0f,
         G = g / 31.0f,
         B = b / 31.0f,
         A = 1.0f,
      };
   }

   public void Dispose()
   {
      GC.SuppressFinalize(this);
   }
}