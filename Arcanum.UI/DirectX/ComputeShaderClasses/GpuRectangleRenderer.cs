using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using MapFlags = Vortice.Direct3D11.MapFlags;

namespace Arcanum.UI.DirectX.ComputeShaderClasses;

// Matches the per-vertex input of the vertex shader
[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
   public Vector2 Position;
}

// Matches the InstanceData struct in the HLSL shader
[StructLayout(LayoutKind.Sequential)]
public struct InstanceData
{
   public Vector2 Offset;
   public Vector2 Size;
   public Float4 Color; // Using your existing Float4 struct
}

[StructLayout(LayoutKind.Sequential)]
struct Constants
{
   public Vector2 RenderTargetSize;

   // Add padding if necessary to make the total size a multiple of 16.
   // In this case, Vector2 is 8 bytes, so we need 8 bytes of padding.
   public Vector2 Padding;
}

public class GpuRectangleRenderer : IDisposable
{
   private readonly ID3D11Device _device;
   private readonly ID3D11DeviceContext _context;

   public GpuRectangleRenderer()
   {
      D3D11.D3D11CreateDevice(null,
                              DriverType.Hardware,
                              DeviceCreationFlags.None,
                              [FeatureLevel.Level_11_0],
                              out _device,
                              out _context)
           .CheckError();
   }

   public void RenderRectanglesToFile(string outputPath, int width, int height, GpuRect[] rects, Float4[] colors)
   {
      unsafe
      {
         // --- 1. Compile Shaders ---
         var vsByteCode = ID3DRenderer.CompileBytecode("DrawRects.hlsl", "VS", "vs_5_0");
         using var vertexShader = _device.CreateVertexShader(vsByteCode.Span);

         var psByteCode = ID3DRenderer.CompileBytecode("DrawRects.hlsl", "PS", "ps_5_0");
         using var pixelShader = _device.CreatePixelShader(psByteCode.Span);

         // --- 2. Create Render Target Resources ---
         var textureDesc = new Texture2DDescription
         {
            Width = (uint)width,
            Height = (uint)height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.R8G8B8A8_UNorm,
            SampleDescription = new(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource
         };
         using var renderTargetTexture = _device.CreateTexture2D(textureDesc);
         using var renderTargetView = _device.CreateRenderTargetView(renderTargetTexture);

         // --- 3. Create Input Assembler Resources (Buffers and Layout) ---

         // A simple quad made of 4 vertices (will be drawn as 2 triangles)
         var quadVertices = new Vertex[]
         {
            new() { Position = new(0, 0) }, // Top-left
            new() { Position = new(1, 0) }, // Top-right
            new() { Position = new(0, 1) }, // Bottom-left
            new() { Position = new(1, 1) } // Bottom-right
         };
         // We also need an index buffer to tell the GPU how to form triangles from the vertices
         var quadIndices = new ushort[] { 0, 1, 2, 2, 1, 3 };

         using var vertexBuffer = _device.CreateBuffer(quadVertices, BindFlags.VertexBuffer);
         using var indexBuffer = _device.CreateBuffer(quadIndices, BindFlags.IndexBuffer);

         // Create the per-instance data
         var instanceData = new InstanceData[rects.Length];
         for (int i = 0; i < rects.Length; i++)
         {
            instanceData[i] = new()
            {
               Offset = new(rects[i].Left, rects[i].Top),
               Size = new(rects[i].Right - rects[i].Left, rects[i].Bottom - rects[i].Top),
               Color = colors[i]
            };
         }

         using var instanceBuffer = _device.CreateBuffer(instanceData, BindFlags.VertexBuffer);

         var constBufferDesc = new BufferDescription
         {
            // ByteWidth MUST be a multiple of 16.
            ByteWidth = 16, // Marshal.SizeOf<Constants>() would also work and is safer.
            Usage = ResourceUsage.Dynamic,
            BindFlags = BindFlags.ConstantBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
         };
         using var constantBuffer = _device.CreateBuffer(constBufferDesc);
         var constants = new Constants { RenderTargetSize = new(width, height) };
         var mappedResource = _context.Map(constantBuffer, 0, MapMode.WriteDiscard);
         try
         {
            Marshal.StructureToPtr(constants, mappedResource.DataPointer, false);
         }
         finally
         {
            _context.Unmap(constantBuffer, 0);
         }

         // Create the Input Layout - This tells the GPU how our vertex and instance data is structured.
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
                                        1)
         };
         using var inputLayout = _device.CreateInputLayout(inputElements, vsByteCode.Span);

         // --- 4. Set Pipeline State ---
         _context.IASetInputLayout(inputLayout);
         _context.IASetVertexBuffer(0, vertexBuffer, (uint)Marshal.SizeOf<Vertex>());
         _context.IASetVertexBuffer(1, instanceBuffer, (uint)Marshal.SizeOf<InstanceData>());
         _context.IASetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
         _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
         _context.VSSetShader(vertexShader);
         _context.VSSetConstantBuffer(0, constantBuffer);
         _context.RSSetViewport(0, 0, width, height);
         _context.PSSetShader(pixelShader);
         _context.OMSetRenderTargets(renderTargetView);

         // --- 5. Clear and Draw ---
         _context.ClearRenderTargetView(renderTargetView, new(0.0f, 0.0f, 0.0f)); // Clear to black
         _context.DrawIndexedInstanced((uint)quadIndices.Length, (uint)rects.Length, 0, 0, 0);

         // --- 6. Readback the Result to CPU ---
         var stagingDesc = renderTargetTexture.Description;
         stagingDesc.Usage = ResourceUsage.Staging;
         stagingDesc.BindFlags = BindFlags.None;
         stagingDesc.CPUAccessFlags = CpuAccessFlags.Read;
         using var stagingTexture = _device.CreateTexture2D(stagingDesc);
         _context.CopyResource(stagingTexture, renderTargetTexture);

         var mapped = _context.Map(stagingTexture, 0);
         try
         {
            var image = Image.LoadPixelData<Rgba32>(new ReadOnlySpan<byte>(mapped.DataPointer.ToPointer(),
                                                                           (int)(height * mapped.RowPitch)),
                                                    (int)width,
                                                    (int)height);
            image.SaveAsPng(outputPath);
         }
         finally
         {
            _context.Unmap(stagingTexture, 0);
            _context.Unmap(constantBuffer);
         }
      }
   }

   public void Dispose()
   {
      _context.Dispose();
      _device.Dispose();
   }
}