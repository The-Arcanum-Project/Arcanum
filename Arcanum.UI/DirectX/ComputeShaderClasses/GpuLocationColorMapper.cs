using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

// Define a struct that matches the HLSL Rect struct byte-for-byte.
[StructLayout(LayoutKind.Sequential)]
public struct GpuRect
{
   public int Left;
   public int Top;
   public int Right;
   public int Bottom;

   public override string ToString()
   {
      return $"L:{Left,4} T:{Top,4} R:{Right,4} B:{Bottom,4}";
   }
}

// Define a struct for the output color.
[StructLayout(LayoutKind.Sequential)]
public struct Float4
{
   public float R,
                G,
                B,
                A;
}

public class GpuLocationColorMapper : IDisposable
{
   private readonly ID3D11Device _device;
   private readonly ID3D11DeviceContext _context;

   public GpuLocationColorMapper()
   {
      // 1. Create the D3D11 Device and Context.
      // D3D11.D3D11CreateDevice will pick the default adapter.
      // We request FeatureLevel_11_0 for compute shader support.
      D3D11.D3D11CreateDevice(null,
                              DriverType.Hardware,
                              DeviceCreationFlags.None,
                              [FeatureLevel.Level_11_0],
                              out _device,
                              out _context)
           .CheckError();
   }

   public Float4[] MapColorsToLocations(Image<Rgba32> image, GpuRect[] locationRects)
   {
      // 2. Compile the Compute Shader
      // This reads the HLSL file and compiles it to bytecode at runtime.
      var shaderByteCode = ID3DRenderer.CompileBytecode("Compute.AverageColor.hlsl",
                                                        "main", // Entry point function
                                                        "cs_5_0"); // Compile target: Compute Shader 5.0

      using var computeShader = _device.CreateComputeShader(shaderByteCode.Span);

      // 3. Load the Image and Create GPU Resources
      // Use ImageSharp to load the PNG into a managed byte array.

      var pixelData = new Rgba32[image.Width * image.Height];
      image.CopyPixelDataTo(pixelData);
      var pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
      var textureDesc = new Texture2DDescription
      {
         Width = (uint)image.Width,
         Height = (uint)image.Height,
         MipLevels = 1,
         ArraySize = 1,
         Format = Format.R8G8B8A8_UNorm, // 8-bit per channel, normalized [0,1]
         SampleDescription = new(1, 0),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.ShaderResource,
         CPUAccessFlags = CpuAccessFlags.None,
      }; // as before
      var subresourceData = new SubresourceData(pixelDataHandle.AddrOfPinnedObject(), (uint)(image.Width * 4));
      using var gpuTexture = _device.CreateTexture2D(textureDesc, subresourceData);
      pixelDataHandle.Free();
      using var srv = _device.CreateShaderResourceView(gpuTexture);

      var boundsBufferDesc = new BufferDescription
      {
         ByteWidth = (uint)(locationRects.Length * Marshal.SizeOf<GpuRect>()),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.ShaderResource,
         StructureByteStride = (uint)Marshal.SizeOf<GpuRect>(),
         MiscFlags = ResourceOptionFlags.BufferStructured, // Corrected from MiscFlags
      };
      using var gpuBoundsBuffer = _device.CreateBuffer(locationRects.AsSpan(), boundsBufferDesc);
      var boundsSrvDesc =
         new ShaderResourceViewDescription(gpuBoundsBuffer, Format.Unknown, 0, (uint)locationRects.Length);
      using var boundsSrv = _device.CreateShaderResourceView(gpuBoundsBuffer, boundsSrvDesc);

      var outputBufferDesc = new BufferDescription
      {
         ByteWidth = (uint)(locationRects.Length * Marshal.SizeOf<Float4>()),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
         StructureByteStride = (uint)Marshal.SizeOf<Float4>(),
         MiscFlags = ResourceOptionFlags.BufferStructured, // Corrected from MiscFlags
      };
      using var gpuOutputBuffer = _device.CreateBuffer(outputBufferDesc);
      var uavDesc = new UnorderedAccessViewDescription(gpuOutputBuffer, Format.Unknown, 0, (uint)locationRects.Length);
      using var uav = _device.CreateUnorderedAccessView(gpuOutputBuffer, uavDesc);

      var stagingBufferDesc = new BufferDescription
      {
         ByteWidth = outputBufferDesc.ByteWidth,
         Usage = ResourceUsage.Staging,
         CPUAccessFlags = CpuAccessFlags.Read,
      };
      using var stagingBuffer = _device.CreateBuffer(stagingBufferDesc);

      // 4. Set up the Pipeline and Dispatch
      var srvs = new[] { srv, boundsSrv };
      _context.CSSetShaderResources(0, srvs);
      var uavs = new[] { uav };
      _context.CSSetUnorderedAccessViews(0, uavs);

      _context.CSSetShader(computeShader); // Set the shader just before dispatch

      var threadGroupsX = (uint)Math.Ceiling(locationRects.Length / 64.0);
      _context.Dispatch(threadGroupsX, 1, 1);

      var nullUavs = new ID3D11UnorderedAccessView[] { null };
      _context.CSSetUnorderedAccessViews(0, nullUavs);

      // 5. Copy Results from GPU to CPU
      // This copy is now safe to execute.
      _context.CopyResource(stagingBuffer, gpuOutputBuffer);

      // Unbind the rest of the resources
      var nullSrvs = new ID3D11ShaderResourceView?[] { null, null };
      _context.CSSetShaderResources(0, nullSrvs);
      _context.CSSetShader(null);

      // Map, Readback, and Unmap (Your code here is correct)
      var mapped = _context.Map(stagingBuffer, MapMode.Read);
      try
      {
         unsafe
         {
            var sizeInBytes = locationRects.Length * Marshal.SizeOf<Float4>();
            var gpuDataSpan = new ReadOnlySpan<byte>(mapped.DataPointer.ToPointer(), sizeInBytes);
            var results = new Float4[locationRects.Length];
            MemoryMarshal.Cast<byte, Float4>(gpuDataSpan).CopyTo(results);
            return results;
         }
      }
      finally
      {
         _context.Unmap(stagingBuffer);
      }
   }

   public void Dispose()
   {
      _context.Dispose();
      _device.Dispose();
   }
}