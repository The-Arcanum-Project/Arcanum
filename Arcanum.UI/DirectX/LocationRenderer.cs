using System.Numerics;
using System.Runtime.InteropServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Arcanum.UI.DirectX;

public readonly struct VertexPositionId2D(in Vector2 position, uint polygonId)
{
    // The size is now smaller
    public static readonly unsafe uint SizeInBytes = (uint)sizeof(VertexPositionId2D);

    public readonly Vector2 Position = position; // Changed from Vector3
    public readonly uint PolygonId = polygonId;
}


[StructLayout(LayoutKind.Sequential)]
public struct Constants
{
    public Matrix4x4 WorldViewProjection;
}


public class LocationRenderer(List<Polygon> polygons) : ID3DRenderer
{
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
    private uint _vertexCount;
    
    private VertexPositionId2D[] _vertices;
    private Color4[] _polygonColors = new Color4[polygons.Count];
    
    private (int,int) imageSize;
    
    public List<VertexPositionId2D> GetVertices()
    {
        var vertices = new List<VertexPositionId2D>(3 * polygons.Count);
        var aspectRatio = imageSize.Item2 / (float)imageSize.Item1;
        var random = new Random(0);
        for (var i = 0; i < polygons.Count; i++)
        {
            var polygon = polygons[i];
            // Generate a random color for the polygon
            _polygonColors[i] = new(
                random.Next(),
                random.Next(),
                random.Next()); // Full opacity

            // Triangulate the polygon using a simple fan method
            var indices = polygon.Indices;
            var triangleVertices = polygon.Vertices;
            for (var j = 0; j < indices.Length; j += 3)
            {
                var v0 = triangleVertices[indices[i]];
                var v1 = triangleVertices[indices[i + 1]];
                var v2 = triangleVertices[indices[i + 2]];
                vertices.Add(new(new(v0.X / imageSize.Item1, aspectRatio * (1 - v0.Y / imageSize.Item2)),
                    (uint)i));
                vertices.Add(new(new(v1.X / imageSize.Item1, aspectRatio * (1 - v1.Y / imageSize.Item2)),
                    (uint)i));
                vertices.Add(new(new(v2.X / imageSize.Item1, aspectRatio * (1 - v2.Y / imageSize.Item2)),
                    (uint)i));
            }
        }

        return vertices;
    }

    public void Initialize(IntPtr hwnd, int width, int height)
    {
       

    }

    public void Render()
    {
        if (_renderTargetView == null || _context == null || _swapChain == null) return;
        
        _context!.OMSetRenderTargets(_renderTargetView);
        //_context!.ClearRenderTargetView(_renderTargetView, new(0f, 0f, 0f, 0f));
        _context.Draw(_vertexCount, 0);
        _swapChain!.Present(1, PresentFlags.None);
    }
    
    public void Resize(int width, int height)
    {
        // Guard against invalid size or uninitialized state
        if (width <= 0 || height <= 0 || _context == null || _swapChain == null || _device == null)
        {
            return;
        }

        // 1. Release the old render target view
        _renderTargetView?.Dispose();
        _context.Flush(); // Ensure all commands are executed before resizing

        // 2. Resize the swap chain buffers
        _swapChain.ResizeBuffers(1, (uint)width, (uint)height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

        // 3. Recreate the render target view from the new back buffer
        using (var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0))
        {
            _renderTargetView = _device.CreateRenderTargetView(backBuffer);
        }

        // 4. Set the new viewport
        _context.RSSetViewport(new Viewport(width, height));
    }
    
    public void Dispose()
    {
        _renderTargetView?.Dispose();
        _vertexBuffer?.Dispose();
        _inputLayout?.Dispose();
        _vertexShader?.Dispose();
        _pixelShader?.Dispose();
        _swapChain?.Dispose();
        _context?.Dispose();
        _device?.Dispose();
        GC.SuppressFinalize(this);
    }
}