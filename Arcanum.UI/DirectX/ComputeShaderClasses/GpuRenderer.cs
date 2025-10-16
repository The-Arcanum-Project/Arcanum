using SharpGen.Runtime;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

public class GpuRenderer : IDisposable
{
   private readonly ID3D11Device _device;
   private readonly ID3D11DeviceContext _context;

   public GpuRenderer()
   {
      D3D11.D3D11CreateDevice(null,
                              DriverType.Hardware,
                              DeviceCreationFlags.None,
                              [FeatureLevel.Level_11_0],
                              out _device,
                              out _context)
           .CheckError();
   }

   public ID3D11Device Device => _device;

   public ID3D11Buffer CreateBuffer<T>(ReadOnlySpan<T> data, BindFlags bindFlags) where T : unmanaged
   {
      return _device.CreateBuffer(data, bindFlags);
   }

   public ID3D11Buffer CreateConstantBuffer(uint sizeInBytes)
   {
      if (sizeInBytes % 16 != 0)
         throw new ArgumentException("Constant buffer size must be a multiple of 16.", nameof(sizeInBytes));

      var desc = new BufferDescription
      {
         ByteWidth = sizeInBytes,
         Usage = ResourceUsage.Dynamic,
         BindFlags = BindFlags.ConstantBuffer,
         CPUAccessFlags = CpuAccessFlags.Write,
      };
      return _device.CreateBuffer(desc);
   }

   public void UpdateBuffer<T>(ID3D11Buffer buffer, in T data) where T : unmanaged
   {
      var mapped = _context.Map(buffer, 0, MapMode.WriteDiscard);
      try
      {
         Marshal.StructureToPtr(data, mapped.DataPointer, false);
      }
      finally
      {
         _context.Unmap(buffer, 0);
      }
   }

   public ID3D11Texture2D CreateRenderTarget(uint width, uint height)
   {
      var desc = new Texture2DDescription
      {
         Width = width,
         Height = height,
         MipLevels = 1,
         ArraySize = 1,
         Format = Format.R8G8B8A8_UNorm,
         SampleDescription = new(1, 0),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
      };
      return _device.CreateTexture2D(desc);
   }

   public void Render(
      ID3D11RenderTargetView renderTarget,
      ID3D11VertexShader vertexShader,
      ID3D11PixelShader pixelShader,
      ID3D11InputLayout inputLayout,
      ID3D11Buffer[] vertexBuffers,
      uint[] strides,
      ID3D11Buffer indexBuffer,
      ID3D11Buffer constantBuffer,
      PrimitiveTopology topology,
      uint indexCount,
      uint instanceCount)
   {
      var desc = renderTarget.Resource.As<ID3D11Texture2D>().Description;
      var viewport = new Viewport(0, 0, desc.Width, desc.Height, 0.0f, 1.0f);

      // --- Set Pipeline State ---
      _context.IASetInputLayout(inputLayout);

      var offsets = new uint[vertexBuffers.Length];
      _context.IASetVertexBuffers(0,
                                  (uint)vertexBuffers.Length,
                                  vertexBuffers,
                                  strides.AsSpan(),
                                  offsets.AsSpan());

      _context.IASetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
      _context.IASetPrimitiveTopology(topology);
      _context.VSSetShader(vertexShader);
      _context.VSSetConstantBuffer(0, constantBuffer);
      _context.RSSetViewport(viewport);
      _context.PSSetShader(pixelShader);
      _context.OMSetRenderTargets(renderTarget);

      // --- Clear and Draw ---
      _context.ClearRenderTargetView(renderTarget, new(0.0f, 0.0f, 0.0f));
      _context.DrawIndexedInstanced(indexCount, instanceCount, 0, 0, 0);

      // --- Cleanup State ---
      _context.OMGetRenderTargets(0, [], out _); // This really wierd and false?
   }

   public void ReadbackTextureAndSave(ID3D11Texture2D sourceTexture, string outputPath, uint bytesPerPixel = 4)
   {
      var desc = sourceTexture.Description;
      var stagingDesc = new Texture2DDescription
      {
         Width = desc.Width,
         Height = desc.Height,
         MipLevels = 1,
         ArraySize = 1,
         Format = desc.Format,
         SampleDescription = new(1, 0),
         Usage = ResourceUsage.Staging,
         BindFlags = BindFlags.None,
         CPUAccessFlags = CpuAccessFlags.Read,
      };
      using var stagingTexture = _device.CreateTexture2D(stagingDesc);

      _context.CopyResource(stagingTexture, sourceTexture);

      var mapped = _context.Map(stagingTexture, 0);
      try
      {
         var width = desc.Width;
         var height = desc.Height;
         var sourceRowPitch = mapped.RowPitch;
         var destinationStride = (int)(width * bytesPerPixel);

         var tightlyPackedPixels = new byte[width * height * bytesPerPixel];

         if (sourceRowPitch == destinationStride)
         {
            Marshal.Copy(mapped.DataPointer, tightlyPackedPixels, 0, tightlyPackedPixels.Length);
         }
         else
         {
            for (var y = 0; y < height; y++)
            {
               var targetPtr = (IntPtr)(mapped.DataPointer + (y * sourceRowPitch));
               Marshal.Copy(targetPtr,
                            tightlyPackedPixels,
                            y * destinationStride,
                            destinationStride);
            }
         }

         var image = Image.LoadPixelData<Rgba32>(tightlyPackedPixels, (int)width, (int)height);
         image.SaveAsPng(outputPath);
      }
      finally
      {
         _context.Unmap(stagingTexture, 0);
      }
   }

   public void Dispose()
   {
      _context.Dispose();
      _device.Dispose();
      GC.SuppressFinalize(this);
   }
}