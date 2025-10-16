namespace Arcanum.UI.DirectX.ComputeShaderClasses;

using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

public class GpuComputeRunner : IDisposable
{
   private readonly ID3D11Device _device;
   private readonly ID3D11DeviceContext _context;

   public GpuComputeRunner()
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
   public ID3D11DeviceContext Context => _context;

   /// <summary>
   /// Creates a Texture2D from an ImageSharp Image&lt;Rgba32&gt;.
   /// The texture will have a format of R8G8B8A8_UNorm.
   /// </summary>
   /// <param name="image"></param>
   /// <returns></returns>
   public unsafe ID3D11Texture2D CreateTexture2D(Image<Rgba32> image)
   {
      var pixelData = new Rgba32[image.Width * image.Height];
      image.CopyPixelDataTo(pixelData);
      var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
      try
      {
         var textureDesc = new Texture2DDescription(Format.R8G8B8A8_UNorm,
                                                    (uint)image.Width,
                                                    (uint)image.Height,
                                                    1,
                                                    1);
         var subresourceData = new SubresourceData(handle.AddrOfPinnedObject().ToPointer(), (uint)image.Width * 4);
         return _device.CreateTexture2D(textureDesc, subresourceData);
      }
      finally
      {
         handle.Free();
      }
   }

   /// <summary>
   /// Creates a structured buffer from the provided data.
   /// The buffer will have BindFlags of ShaderResource and MiscFlags of BufferStructured.
   /// </summary>
   /// <param name="data"></param>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   public ID3D11Buffer CreateStructuredBuffer<T>(ReadOnlySpan<T> data) where T : unmanaged
   {
      var bufferDesc = new BufferDescription
      {
         ByteWidth = (uint)(data.Length * Marshal.SizeOf<T>()),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.ShaderResource,
         StructureByteStride = (uint)Marshal.SizeOf<T>(),
         MiscFlags = ResourceOptionFlags.BufferStructured,
      };
      return _device.CreateBuffer(data, bufferDesc);
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

   public unsafe T[] ReadBackBuffer<T>(ID3D11Buffer buffer) where T : unmanaged
   {
      var stagingDesc = buffer.Description;
      stagingDesc.Usage = ResourceUsage.Staging;
      stagingDesc.BindFlags = BindFlags.None;
      stagingDesc.CPUAccessFlags = CpuAccessFlags.Read;
      stagingDesc.MiscFlags = ResourceOptionFlags.None;

      using var stagingBuffer = _device.CreateBuffer(stagingDesc);
      _context.CopyResource(stagingBuffer, buffer);

      var mapped = _context.Map(stagingBuffer, 0, MapMode.Read);
      try
      {
         var sizeInBytes = buffer.Description.ByteWidth;
         var dataSpan = new ReadOnlySpan<byte>(mapped.DataPointer.ToPointer(), (int)sizeInBytes);
         var result = new T[sizeInBytes / sizeof(uint)];
         MemoryMarshal.Cast<byte, T>(dataSpan).CopyTo(result);
         return result;
      }
      finally
      {
         _context.Unmap(stagingBuffer, 0);
      }
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

   /// <summary>
   /// Creates a read-write structured buffer with the specified number of elements.
   /// The buffer will have BindFlags of ShaderResource and UnorderedAccess, and MiscFlags of BufferStructured.
   /// </summary>
   /// <param name="elementCount"></param>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   public ID3D11Buffer CreateReadWriteStructuredBuffer<T>(int elementCount) where T : unmanaged
   {
      var bufferDesc = new BufferDescription
      {
         ByteWidth = (uint)(elementCount * Marshal.SizeOf<T>()),
         Usage = ResourceUsage.Default,
         BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
         StructureByteStride = (uint)Marshal.SizeOf<T>(),
         MiscFlags = ResourceOptionFlags.BufferStructured,
      };
      return _device.CreateBuffer(bufferDesc);
   }

   /// <summary>
   /// Creates a Shader Resource View (SRV) for the given resource.
   /// Supports ID3D11Buffer and ID3D11Texture2D.
   /// For buffers, it assumes a structured buffer with unknown format.
   /// </summary>
   /// <param name="resource"></param>
   /// <returns></returns>
   /// <exception cref="ArgumentException"></exception>
   public ID3D11ShaderResourceView CreateShaderResourceView(ID3D11Resource resource)
   {
      switch (resource)
      {
         case ID3D11Buffer buffer:
         {
            var desc = new ShaderResourceViewDescription(buffer,
                                                         Format.Unknown,
                                                         0,
                                                         buffer.Description.ByteWidth /
                                                         buffer.Description.StructureByteStride);
            return _device.CreateShaderResourceView(buffer, desc);
         }
         case ID3D11Texture2D:
            return _device.CreateShaderResourceView(resource);
         default:
            throw new ArgumentException("Resource must be a Buffer or Texture2D.", nameof(resource));
      }
   }

   /// <summary>
   /// Creates an Unordered Access View (UAV) for the given buffer.
   /// The buffer must be a structured buffer with BindFlags including UnorderedAccess.
   /// </summary>
   /// <param name="buffer"></param>
   /// <returns></returns>
   public ID3D11UnorderedAccessView CreateUnorderedAccessView(ID3D11Buffer buffer)
   {
      return _device.CreateUnorderedAccessView(buffer,
                                               new UnorderedAccessViewDescription(buffer,
                                                Format.Unknown,
                                                0,
                                                buffer.Description.ByteWidth /
                                                buffer.Description.StructureByteStride));
   }

   /// <summary>
   /// Compiles and executes a compute shader with the specified parameters.
   /// The output is read back from the GPU into a managed array of TOutput.
   /// The outputBuffer must be a structured buffer with BindFlags including UnorderedAccess.
   /// The shaderResourceViews are bound starting at slot 0.
   /// The compute shader is dispatched with the specified thread group counts.
   /// After execution, the output buffer is copied to a staging buffer for CPU readback.
   /// Finally, the data is mapped and copied into a managed array which is returned.
   /// </summary>
   /// <param name="shaderPath">The path to the compute shader file, relative to the ComputeShaders directory.</param>
   /// <param name="entryPoint">The entry point function name in the shader.</param>
   /// <param name="threadGroupsX">The number of thread groups to dispatch in the X dimension.</param>
   /// <param name="threadGroupsY">The number of thread groups to dispatch in the Y dimension.</param>
   /// <param name="threadGroupsZ">The number of thread groups to dispatch in the Z dimension.</param>
   /// <param name="outputBuffer">The GPU buffer to write output data to. Must be a structured buffer with UnorderedAccess bind flag.</param>
   /// <param name="shaderResourceViews">The shader resource views to bind to the compute shader, starting at slot 0.</param>
   /// <typeparam name="TOutput">The type of the output data. Must be an unmanaged type.</typeparam>
   /// <returns>The output data read back from the GPU as an array of TOutput.</returns>
   public unsafe TOutput[] Execute<TOutput>(
      string shaderPath,
      string entryPoint,
      uint threadGroupsX,
      uint threadGroupsY,
      uint threadGroupsZ,
      ID3D11Buffer outputBuffer,
      params ID3D11ShaderResourceView[] shaderResourceViews) where TOutput : unmanaged
   {
      var shaderByteCodeResult = ID3DRenderer.CompileBytecode($"Compute.{shaderPath}", entryPoint, "cs_5_0");

      using var computeShader = _device.CreateComputeShader(shaderByteCodeResult.Span);
      using var uav = CreateUnorderedAccessView(outputBuffer);

      var stagingBufferDesc = outputBuffer.Description;
      stagingBufferDesc.Usage = ResourceUsage.Staging;
      stagingBufferDesc.BindFlags = BindFlags.None;
      stagingBufferDesc.CPUAccessFlags = CpuAccessFlags.Read;
      stagingBufferDesc.MiscFlags = ResourceOptionFlags.None;
      using var stagingBuffer = _device.CreateBuffer(stagingBufferDesc);

      _context.CSSetShader(computeShader);
      _context.CSSetShaderResources(0, shaderResourceViews);
      _context.CSSetUnorderedAccessViews(0, [uav]);

      _context.Dispatch(threadGroupsX, threadGroupsY, threadGroupsZ);

      _context.CSSetUnorderedAccessViews(0, [null!]);

      _context.CopyResource(stagingBuffer, outputBuffer);

      _context.CSSetShaderResources(0, new ID3D11ShaderResourceView[shaderResourceViews.Length]);
      _context.CSSetShader(null);

      var mapped = _context.Map(stagingBuffer, MapMode.Read);
      try
      {
         var sizeInBytes = outputBuffer.Description.ByteWidth;
         var gpuDataSpan = new ReadOnlySpan<byte>(mapped.DataPointer.ToPointer(), (int)sizeInBytes);

         var results = new TOutput[sizeInBytes / Marshal.SizeOf<TOutput>()];
         MemoryMarshal.Cast<byte, TOutput>(gpuDataSpan).CopyTo(results);
         return results;
      }
      finally
      {
         _context.Unmap(stagingBuffer);
      }
   }

   void IDisposable.Dispose()
   {
      _context.Dispose();
      _device.Dispose();
      GC.SuppressFinalize(this);
   }
}