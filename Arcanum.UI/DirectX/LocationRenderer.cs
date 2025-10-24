using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Color = System.Windows.Media.Color;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Vector = System.Numerics.Vector;

namespace Arcanum.UI.DirectX;

public readonly struct VertexPositionId2D(in Vector2 position, uint polygonId)
{
   public static readonly unsafe uint SizeInBytes = (uint)sizeof(VertexPositionId2D);

   // ReSharper disable once UnusedMember.Global
   public readonly Vector2 Position = position;

   // ReSharper disable once UnusedMember.Global
   public readonly uint PolygonId = polygonId;
}

public readonly struct BorderVertex(in Vector2 position, uint borderId, Vector2 texCoord)
{
   public static readonly unsafe uint SizeInBytes = (uint)sizeof(BorderVertex);
   public readonly Vector2 Position = position;
   public readonly Vector2 TexCoord = texCoord;
   public readonly uint BorderId = borderId;
}

public readonly struct BorderMeshData(BorderVertex[] vertices, int[] indices, BorderProperties[] borderProperties)
{
   public readonly BorderVertex[] Vertices = vertices;
   public readonly int[] Indices = indices;
   public readonly BorderProperties[] BorderProperties = borderProperties;
}

[StructLayout(LayoutKind.Sequential)]
public struct BorderProperties(Color4 color, uint styleIndex)
{
   public Color4 Color = color;
   public uint StyleIndex = styleIndex;
   // Add other properties as needed, ensure they are 4-byte aligned
   private readonly uint _padding1;
   private readonly uint _padding2;
   private readonly uint _padding3;
}

[StructLayout(LayoutKind.Sequential)]
public struct Constants
{
   public Matrix4x4 WorldViewProjection;
}

public class LocationRenderer(VertexPositionId2D[] vertices, Color4[] initColors, BorderVertex[] borderVertices, int[] borderIndices, BorderProperties[] initBorderProperties, float imageAspectRatio) : ID3DRenderer
{
   private static readonly Color4 ClearColor;

   private ID3D11Device? _device;
   private ID3D11DeviceContext? _context;
   private IDXGISwapChain? _swapChain;
   private ID3D11RenderTargetView? _renderTargetView;

   // Polygon resources
   private ID3D11VertexShader? _polygonVertexShader;
   private ID3D11PixelShader? _polygonPixelShader;
   private ID3D11InputLayout? _polygonInputLayout;
   private ID3D11Buffer? _polygonVertexBuffer;
   private ID3D11Buffer? _colorLookupBuffer;
   private ID3D11ShaderResourceView? _colorLookupView;

   // Border resources
   private ID3D11VertexShader? _borderVertexShader;
   private ID3D11PixelShader? _borderPixelShader;
   private ID3D11InputLayout? _borderInputLayout;
   private ID3D11Buffer? _borderVertexBuffer;
   private ID3D11Buffer? _borderIndexBuffer;
   private ID3D11Buffer? _borderPropertiesBuffer;
   private ID3D11ShaderResourceView? _borderPropertiesView;


   private ID3D11Buffer? _constantBuffer;
   private Constants _constants;

   private Color4[] _polygonColors = initColors;

   public Vector2 Pan = new(0.5f, 0.5f);

   public float Zoom = 1f;

   private uint _polygonVertexCount = (uint)vertices.Length;
   private uint _borderVertexCount = (uint)borderVertices.Length;
   private uint _borderIndexCount = (uint)borderIndices.Length;
   private int[] _borderIndices = borderIndices; 
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
         var indices = polygon.TriangleIndices; // TODO @Melco crashes with polygon = null
         var triangleVertices = polygon.Vertices;
         for (var j = 0; j < indices.Length; j += 3)
         {
            var v0 = triangleVertices[indices[j]];
            var v1 = triangleVertices[indices[j + 1]];
            var v2 = triangleVertices[indices[j + 2]];
            vertices.Add(new(new(v0.X / imageSize.Item1, imageAspectRatio * (1 - v0.Y / imageSize.Item2)),
                             (uint)polygon.ColorIndex));
            vertices.Add(new(new(v1.X / imageSize.Item1, imageAspectRatio * (1 - v1.Y / imageSize.Item2)),
                             (uint)polygon.ColorIndex));
            vertices.Add(new(new(v2.X / imageSize.Item1, imageAspectRatio * (1 - v2.Y / imageSize.Item2)),
                             (uint)polygon.ColorIndex));
         }
      }

      return vertices.ToArray();
   }
   
   private const float HALF_BORDER_WIDTH = 1f; // Half the width of the border in pixels
   
   private static Vector2 CalculateEndCapOffset(Vector2 p1, Vector2 p2)
   {
      var direction = Vector2.Normalize(p2 - p1);
      // The normal is perpendicular to the direction.
      var normal = new Vector2(-direction.Y, direction.X);
      return normal * HALF_BORDER_WIDTH;
   }

   /// <summary>
   /// Calculates the offset for a corner point ("miter join") to ensure the border
   /// has a continuous width without gaps.
   /// </summary>
   private static Vector2 CalculateMiterOffset(Vector2 pPrev, Vector2 pCurrent, Vector2 pNext)
   {
      // Get directions of the incoming and outgoing segments
      var dirIn = Vector2.Normalize(pCurrent - pPrev);
      var dirOut = Vector2.Normalize(pNext - pCurrent);

      // Get their normals
      var normalIn = new Vector2(-dirIn.Y, dirIn.X);
      var normalOut = new Vector2(-dirOut.Y, dirOut.X);

      // The miter vector is the average of the two normals, normalized.
      // This vector perfectly bisects the angle between the segments.
      var miterVec = Vector2.Normalize(normalIn + normalOut);

      // The length of the offset needs to be extended to meet the outer corner.
      // This is calculated using the dot product, which is related to the cosine of the angle.
      // We use a small epsilon to prevent division by zero if segments are collinear.
      var dot = Vector2.Dot(normalIn, miterVec);
      var miterLength = HALF_BORDER_WIDTH / Math.Max(dot, 0.0001f);
            
      return miterVec * miterLength;
   }
   
   public static BorderMeshData GenerateBorderVertices(List<PolygonParsing> polygons, (int, int) imageSize)
   {
      var imageAspectRatio = (float)imageSize.Item2 / imageSize.Item1;
      var totalBorderVertices = 0;
      var totalBorderIndices = 0;
      var borderMeshData = new List<BorderMeshData>();
      uint borderIdCounter = 0;
      var color = new Color4(0, 1, 0);
      foreach (var segment in polygons.SelectMany(polygon => polygon.Segments))
      {
         if (segment is not BorderSegmentDirectional { IsForward: true } borderSegment)
            continue;
            
         var points = borderSegment.Segment.Points;
         var pointCount = points.Count;
         if (pointCount < 2)
            continue; // Need at least two points to form a border
         
         float totalLength = 0;
         for (int i = 0; i < pointCount - 1; i++)
         {
            totalLength += Vector2.Distance(
               new Vector2(points[i].X, points[i].Y),
               new Vector2(points[i+1].X, points[i+1].Y)
            );
         }
         
         var vertices = new List<BorderVertex>(pointCount * 2);
         var indices = new List<int>((pointCount - 1) * 6);
         float currentDistance = 0;
         for (int i = 0; i < pointCount; i++)
         {
            if (i > 0)
            {
               currentDistance += Vector2.Distance(
                  new Vector2(points[i-1].X, points[i-1].Y),
                  new Vector2(points[i].X, points[i].Y)
               );
            }
            
            
            Vector2 currentPoint = new Vector2(points[i].X, points[i].Y);
            Vector2 offset;

            if (i == 0)
               offset = CalculateEndCapOffset(currentPoint, new Vector2(points[i + 1].X, points[i + 1].Y));
            else if (i == pointCount - 1)
               offset = CalculateEndCapOffset(new Vector2(points[i - 1].X, points[i - 1].Y), currentPoint);
            else
               offset = CalculateMiterOffset(new Vector2(points[i - 1].X, points[i - 1].Y), currentPoint, new Vector2(points[i + 1].X, points[i + 1].Y));

            // Calculate the final positions in pixel space first
            Vector2 leftVertexPos = currentPoint - offset;
            Vector2 rightVertexPos = currentPoint + offset;

            // NOW, convert the final positions to normalized screen space
            leftVertexPos = new Vector2(
               leftVertexPos.X / imageSize.Item1,
               imageAspectRatio * (1 - leftVertexPos.Y / imageSize.Item2)
            );
            rightVertexPos = new Vector2(
               rightVertexPos.X / imageSize.Item1,
               imageAspectRatio * (1 - rightVertexPos.Y / imageSize.Item2)
            );
            float u = (totalLength > 0.0001f) ? currentDistance / totalLength : 0;

            vertices.Add(new BorderVertex(leftVertexPos, borderIdCounter, new Vector2(u, 0.0f)));
            vertices.Add(new BorderVertex(rightVertexPos, borderIdCounter, new Vector2(u, 1.0f)));
         }

         // === 2. Generate Indices (Logic is identical to the previous answer) ===
         for (int i = 0; i < pointCount - 1; i++)
         {
            int baseIndex = i * 2;
            indices.Add(baseIndex);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 3);
         }
         borderMeshData.Add(new (vertices.ToArray(), indices.ToArray(), [new (color, 0)]));
         totalBorderVertices += vertices.Count;
         totalBorderIndices += indices.Count;
         borderIdCounter++;
      }

      var finalBorderVertices = new BorderVertex[totalBorderVertices];
      var finalBorderIndices = new int[totalBorderIndices];
      var finalBorderProperties = new BorderProperties[borderMeshData.Count];
      var indexOffset = 0;
      var vertexOffset = 0;

      for (var index = 0; index < borderMeshData.Count; index++)
      {
         var meshData = borderMeshData[index];
         Array.Copy(meshData.Vertices, 0, finalBorderVertices, vertexOffset, meshData.Vertices.Length);

         // CORRECTED LOGIC: Add the vertexOffset to each index
         for (int i = 0; i < meshData.Indices.Length; i++)
         {
            finalBorderIndices[indexOffset + i] = meshData.Indices[i] + vertexOffset;
         }

         Array.Copy(meshData.BorderProperties, 0, finalBorderProperties, index, 1);
    
         indexOffset += meshData.Indices.Length;
         vertexOffset += meshData.Vertices.Length;
      }


      return new (finalBorderVertices, finalBorderIndices, finalBorderProperties);
      
   }

   public void Resize(int newWidth, int newHeight)
   {
      SetOrthographicProjection(newWidth, newHeight);
   }

   public void Initialize(IntPtr hwnd, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

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

        D3D11.D3D11CreateDeviceAndSwapChain(null, DriverType.Hardware, DeviceCreationFlags.None,
            [FeatureLevel.Level_11_0], swapChainDesc, out _swapChain!, out _device!, out _, out _context!).CheckError();

        using (var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0))
        {
            _renderTargetView = _device.CreateRenderTargetView(backBuffer);
        }

        // Polygon Shaders and Resources
        var polygonVsByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "VSMain", "vs_5_0");
        var polygonPsByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "PSMain", "ps_5_0");
        _polygonVertexShader = _device.CreateVertexShader(polygonVsByteCode.Span);
        _polygonPixelShader = _device.CreatePixelShader(polygonPsByteCode.Span);

        InputElementDescription[] polygonInputElements = [new("POSITION", 0, Format.R32G32_Float, 0, 0), new("POLYGON_ID", 0, Format.R32_UInt, 8, 0)];
        _polygonInputLayout = _device.CreateInputLayout(polygonInputElements, polygonVsByteCode.Span);
        _polygonVertexBuffer = _device.CreateBuffer(_vertices, BindFlags.VertexBuffer);

        // Border Shaders and Resources
        var borderVsByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "BorderVSMain", "vs_5_0");
        var borderPsByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "BorderPSMain", "ps_5_0");
        _borderVertexShader = _device.CreateVertexShader(borderVsByteCode.Span);
        _borderPixelShader = _device.CreatePixelShader(borderPsByteCode.Span);
        _borderIndexBuffer = _device.CreateBuffer(_borderIndices, BindFlags.IndexBuffer);
        
        InputElementDescription[] borderInputElements =
        [
           // Offset 0: Position (Vector2, 8 bytes)
           new("POSITION", 0, Format.R32G32_Float, 0, 0),
    
           // Offset 8: TexCoord (Vector2, 8 bytes)
           // We use '8' here because TexCoord comes right after Position in your struct.
           new("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
    
           // Offset 16: BorderId (uint, 4 bytes)
           // We use '16' here because 8 (Position) + 8 (TexCoord) = 16.
           new("BORDER_ID", 0, Format.R32_UInt, 16, 0)
        ];
        _borderInputLayout = _device.CreateInputLayout(borderInputElements, borderVsByteCode.Span);
        _borderVertexBuffer = _device.CreateBuffer(borderVertices, BindFlags.VertexBuffer);


        _constantBuffer = _device.CreateBuffer(new((uint)Unsafe.SizeOf<Constants>(), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));

        // Polygon Color Buffer
        _colorLookupBuffer = _device.CreateBuffer(new BufferDescription((uint)(_polygonColors.Length * Unsafe.SizeOf<Color4>()), BindFlags.ShaderResource, ResourceUsage.Dynamic, CpuAccessFlags.Write, ResourceOptionFlags.BufferStructured, (uint)Unsafe.SizeOf<Color4>()));
        _colorLookupView = _device.CreateShaderResourceView(_colorLookupBuffer);
        UpdateColors(_polygonColors);

        // Border Properties Buffer
        _borderPropertiesBuffer = _device.CreateBuffer(new BufferDescription((uint)(initBorderProperties.Length * Unsafe.SizeOf<BorderProperties>()), BindFlags.ShaderResource, ResourceUsage.Dynamic, CpuAccessFlags.Write, ResourceOptionFlags.BufferStructured, (uint)Unsafe.SizeOf<BorderProperties>()));
        _borderPropertiesView = _device.CreateShaderResourceView(_borderPropertiesBuffer);
        UpdateBorderProperties(initBorderProperties);

        _context.RSSetViewport(new(width, height));
        _context.OMSetBlendState(null);
        SetOrthographicProjection(width, height);
    }
   public void Render()
   {
      if (_renderTargetView == null || _context == null || _swapChain == null)
         return;
      
      _context.ClearRenderTargetView(_renderTargetView, ClearColor);
      _context.OMSetRenderTargets(_renderTargetView);

      // === 1. Draw Polygons ===
      if (_polygonVertexCount > 0)
      {
         _context.IASetInputLayout(_polygonInputLayout);
         _context.IASetVertexBuffer(0, _polygonVertexBuffer!, VertexPositionId2D.SizeInBytes);
         _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
         _context.VSSetShader(_polygonVertexShader);
         _context.VSSetConstantBuffer(0, _constantBuffer);
         _context.PSSetShader(_polygonPixelShader);
         _context.PSSetShaderResource(0, _colorLookupView!);
         _context.Draw(_polygonVertexCount, 0);
      }

      // === 2. Draw Borders ===
      if (_borderIndexCount > 0)
      {
         _context.IASetInputLayout(_borderInputLayout);
         _context.IASetVertexBuffer(0, _borderVertexBuffer!, BorderVertex.SizeInBytes);
         _context.IASetIndexBuffer(_borderIndexBuffer, Format.R32_UInt, 0); // Use 32-bit uint for indices.
         _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
         _context.VSSetShader(_borderVertexShader);
         _context.VSSetConstantBuffer(0, _constantBuffer);
         _context.PSSetShader(_borderPixelShader);
         _context.PSSetShaderResource(0, _borderPropertiesView!);
         _context.DrawIndexed(_borderIndexCount, 0, 0);
      }
      
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

   public void UpdateColors(Color4[] newColors)
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
   
   public void UpdateBorderProperties(BorderProperties[] newProperties)
   {
      if (_context == null || _borderPropertiesBuffer == null) return;

      unsafe
      {
         var mapped = _context.Map(_borderPropertiesBuffer, MapMode.WriteDiscard);
         fixed (void* pProperties = newProperties)
         {
            Unsafe.CopyBlockUnaligned(mapped.DataPointer.ToPointer(), pProperties, (uint)(newProperties.Length * Unsafe.SizeOf<BorderProperties>()));
         }
         _context.Unmap(_borderPropertiesBuffer);
      }
   }

   public void Dispose()
   {
      _renderTargetView?.Dispose();
      _polygonVertexBuffer?.Dispose();
      _borderVertexBuffer?.Dispose();
      _constantBuffer?.Dispose();
      _colorLookupBuffer?.Dispose();
      _borderPropertiesBuffer?.Dispose();
      _colorLookupView?.Dispose();
      _borderPropertiesView?.Dispose();
      _polygonInputLayout?.Dispose();
      _borderInputLayout?.Dispose();
      _borderIndexBuffer?.Dispose();
      _polygonVertexShader?.Dispose();
      _polygonPixelShader?.Dispose();
      _borderVertexShader?.Dispose();
      _borderPixelShader?.Dispose();
      _swapChain?.Dispose();
      _context?.Dispose();
      _device?.Dispose();
      GC.SuppressFinalize(this);
   }
}