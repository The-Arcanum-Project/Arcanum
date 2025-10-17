using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Color = System.Windows.Media.Color;

namespace Arcanum.UI.DirectX;

public readonly struct VertexPositionId2D(in Vector2 position, uint polygonId)
{
   public static readonly unsafe uint SizeInBytes = (uint)sizeof(VertexPositionId2D);

   // ReSharper disable once UnusedMember.Global
   public readonly Vector2 Position = position;

   // ReSharper disable once UnusedMember.Global
   public readonly uint PolygonId = polygonId;
}

[StructLayout(LayoutKind.Sequential)]
public struct Constants
{
   public Matrix4x4 WorldViewProjection;
}

public class LocationRenderer(VertexPositionId2D[] vertices, Color4[] initColors, float imageAspectRatio) : ID3DRenderer
{
   private static readonly Color4 ClearColor;

   private ID3D11Device? _device;
   private ID3D11DeviceContext? _context;
   private IDXGISwapChain? _swapChain;
   private ID3D11RenderTargetView? _renderTargetView;

   private ID3D11VertexShader? _vertexShader;
   private ID3D11PixelShader? _pixelShader;
   private ID3D11InputLayout? _inputLayout;

   private ID3D11Buffer? _constantBuffer;
   private ID3D11Buffer? _vertexBuffer;
   private Constants _constants;

   private ID3D11Buffer? _colorLookupBuffer;
   private ID3D11ShaderResourceView? _colorLookupView;

   private Color4[] _polygonColors = initColors;

   public Vector2 Pan = new(0.5f, 0.5f);

   public float Zoom = 1f;

   private uint _vertexCount;
   private VertexPositionId2D[] _vertices = vertices;

   static LocationRenderer()
   {
      if (Application.Current.Resources["DefaultBackColor"] is Color color)
      {
         ClearColor = new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
      }
      else
      {
         ClearColor = new(1f, 0f, 1f);
      }
   }

   public static VertexPositionId2D[] CreateVertices(Polygon[] polygons, (int, int) imageSize)
   {
      var imageAspectRatio = (float)imageSize.Item2 / imageSize.Item1;
      var vertices = new List<VertexPositionId2D>(3 * polygons.Length);
      for (var i = 0; i < polygons.Length; i++)
      {
         var polygon = polygons[i];
         var indices = polygon.Indices; // TODO @Melco crashes with polygon = null
         var triangleVertices = polygon.Vertices;
         for (var j = 0; j < indices.Length; j += 3)
         {
            var v0 = triangleVertices[indices[j]];
            var v1 = triangleVertices[indices[j + 1]];
            var v2 = triangleVertices[indices[j + 2]];
            vertices.Add(new(new(v0.X / imageSize.Item1, imageAspectRatio * (1 - v0.Y / imageSize.Item2)),
                             (uint)i));
            vertices.Add(new(new(v1.X / imageSize.Item1, imageAspectRatio * (1 - v1.Y / imageSize.Item2)),
                             (uint)i));
            vertices.Add(new(new(v2.X / imageSize.Item1, imageAspectRatio * (1 - v2.Y / imageSize.Item2)),
                             (uint)i));
         }
      }

      return vertices.ToArray();
   }

   public void Resize(int newWidth, int newHeight)
   {
      SetOrthographicProjection(newWidth, newHeight);
   }

   public void Initialize(IntPtr hwnd, int width, int height)
   {
      if (width <= 0 || height <= 0)
      {
         return;
      }

      var swapChainDesc = new SwapChainDescription
      {
         BufferCount = 1,
         BufferDescription = new((uint)width, (uint)height, new(60, 1), Format.R8G8B8A8_UNorm),
         OutputWindow = hwnd,
         Windowed = true,
         SampleDescription = new(1, 0),
         SwapEffect = SwapEffect.Discard,
         BufferUsage = Usage.RenderTargetOutput
      };

      D3D11.D3D11CreateDeviceAndSwapChain(null,
                                          DriverType.Hardware,
                                          DeviceCreationFlags.None,
                                          [FeatureLevel.Level_11_0],
                                          swapChainDesc,
                                          out _swapChain!,
                                          out _device!,
                                          out _,
                                          out _context!)
           .CheckError();

      using (var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0))
      {
         _renderTargetView = _device.CreateRenderTargetView(backBuffer);
      }

      InputElementDescription[] inputElementDescs =
      [
         new("POSITION", 0, Format.R32G32_Float, 0, 0), new("POLYGON_ID", 0, Format.R32_UInt, 8, 0)
      ];

      var vertexShaderByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "VSMain", "vs_5_0");
      var pixelShaderByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "PSMain", "ps_5_0");

      _vertexShader = _device.CreateVertexShader(vertexShaderByteCode.Span);
      _pixelShader = _device.CreatePixelShader(pixelShaderByteCode.Span);
      _inputLayout = _device.CreateInputLayout(inputElementDescs, vertexShaderByteCode.Span);
      _constantBuffer = _device.CreateBuffer(new((uint)Unsafe.SizeOf<Constants>(),
                                                 BindFlags.ConstantBuffer,
                                                 ResourceUsage.Dynamic,
                                                 CpuAccessFlags.Write));
      _vertexBuffer = _device.CreateBuffer(_vertices, BindFlags.VertexBuffer);
      var colorBufferDesc = new BufferDescription
      {
         // Total size of the buffer in bytes.
         ByteWidth = (uint)(_polygonColors.Length * Unsafe.SizeOf<Color4>()),

         // This is the key: Usage must be Dynamic to allow mapping.
         Usage = ResourceUsage.Dynamic,

         // It's a ShaderResource so the Pixel Shader can read it.
         BindFlags = BindFlags.ShaderResource,

         // This is the second key: The CPU must have write access.
         CPUAccessFlags = CpuAccessFlags.Write,

         // For StructuredBuffer, these are required.
         MiscFlags = ResourceOptionFlags.BufferStructured,
         StructureByteStride = (uint)Unsafe.SizeOf<Color4>()
      };

      _colorLookupBuffer = _device.CreateBuffer(colorBufferDesc);
      _colorLookupView = _device.CreateShaderResourceView(_colorLookupBuffer);

      // 3. Populate the buffer with the initial colors using our update method.
      UpdateColors(_polygonColors);

      _vertexCount = (uint)_vertices.Length;
      _polygonColors = null!;
      _vertices = null!;

      _context.RSSetViewport(new(width, height));
      _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
      _context.VSSetShader(_vertexShader);
      _context.VSSetConstantBuffer(0, _constantBuffer);
      _context.PSSetShader(_pixelShader);
      _context.PSSetShaderResource(0, _colorLookupView);
      _context.IASetInputLayout(_inputLayout);
      _context.IASetVertexBuffer(0, _vertexBuffer, VertexPositionId2D.SizeInBytes);
      _context.OMSetBlendState(null);

      SetOrthographicProjection(width, height);
   }

   public void Render()
   {
      if (_renderTargetView == null || _context == null || _swapChain == null)
         return;

      _context.ClearRenderTargetView(_renderTargetView, ClearColor);

      _context!.OMSetRenderTargets(_renderTargetView);
      _context.Draw(_vertexCount, 0);
      _swapChain!.Present(1, PresentFlags.None);
   }

   public unsafe void SetOrthographicProjection(float width, float height)
   {
      var aspectRatio = width / height;

      var zoomRatio = imageAspectRatio / Zoom;
      
      var view = Matrix4x4.CreateTranslation(-1 * Pan.X, (Pan.Y - 1) * imageAspectRatio, 0);
      var projection = Matrix4x4.CreateOrthographic(zoomRatio * aspectRatio, zoomRatio, -1.0f, 1.0f);
      _constants.WorldViewProjection = Matrix4x4.Transpose(view * projection);
      if (_context == null)
         return;

      var mapped = _context.Map(_constantBuffer!, MapMode.WriteDiscard);
      Unsafe.Write(mapped.DataPointer.ToPointer(), _constants);
      _context.Unmap(_constantBuffer!);
   }

   public void EndResize(int width, int height)
   {
      // Guard against invalid size or uninitialized state
      if (width <= 0 || height <= 0 || _context == null || _swapChain == null || _device == null)
      {
         return;
      }

      // 1. Release the old render target view
      _renderTargetView?.Dispose();
      _context.Flush(); // Ensure all commands are executed before resizing

      // 2. EndResize the swap chain buffers
      _swapChain.ResizeBuffers(1, (uint)width, (uint)height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

      // 3. Recreate the render target view from the new back buffer
      using (var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0))
      {
         _renderTargetView = _device.CreateRenderTargetView(backBuffer);
      }

      // 4. Set the new viewport
      _context.RSSetViewport(new(width, height));
   }

   private void UpdateColors(Color4[] newColors)
   {
      // Ensure the context and buffer have been created and the color count matches.
      if (_context == null || _colorLookupBuffer == null)
      {
         // Or throw an exception
         return;
      }

      // The Map/Unmap pattern is the most efficient way to update the entire buffer's contents.
      unsafe
      {
         // 1. Map the buffer with WriteDiscard.
         // This tells the driver you are replacing the entire contents, which avoids GPU stalls.
         var mapped = _context.Map(_colorLookupBuffer, MapMode.WriteDiscard);

         // 2. Copy the new color data from the C# list into the GPU memory.
         var colorsArray = newColors.ToArray();
         fixed (void* pColors = colorsArray)
         {
            Unsafe.CopyBlockUnaligned(mapped.DataPointer.ToPointer(),
                                      pColors,
                                      (uint)(newColors.Length * Unsafe.SizeOf<Color4>()));
         }

         // 3. Unmap the buffer to commit the changes.
         _context.Unmap(_colorLookupBuffer);
      }
   }

   public void Dispose()
   {
      _renderTargetView?.Dispose();
      _vertexBuffer?.Dispose();
      _constantBuffer?.Dispose();
      _colorLookupBuffer?.Dispose();
      _colorLookupView?.Dispose();
      _inputLayout?.Dispose();
      _vertexShader?.Dispose();
      _pixelShader?.Dispose();
      _swapChain?.Dispose();
      _context?.Dispose();
      _device?.Dispose();
      GC.SuppressFinalize(this);
   }
}