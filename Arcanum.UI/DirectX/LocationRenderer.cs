using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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


public class LocationRenderer : ID3DRenderer
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
    
    private ID3D11Buffer? _colorLookupBuffer;
    private ID3D11ShaderResourceView? _colorLookupView;
    
    private VertexPositionId2D[] _vertices;
    private Color4[] _polygonColors;
    
    private Vector2 _pan = Vector2.Zero;
    private float _zoom = 1.0f;
    private Point _lastMousePosition;
    private bool _isPanning;
    private readonly (int,int) _imageSize;
    private readonly Polygon[] _polygons;

    private Border? _parent;
    private uint _vertexCount;

    public LocationRenderer(Polygon[] polygons, (int,int) imageSize)
    {
        _imageSize = imageSize;
        _polygons = polygons;
        _polygonColors = new Color4[polygons.Length];
        _vertices = GetVertices().ToArray();
    }

    public List<VertexPositionId2D> GetVertices()
    {
        var vertices = new List<VertexPositionId2D>(3 * _polygons.Length);
        var aspectRatio = _imageSize.Item2 / (float)_imageSize.Item1;
        var random = new Random(0);
        for (var i = 0; i < _polygons.Length; i++)
        {
            var polygon = _polygons[i];
            // Generate a random color for the polygon
            _polygonColors[i] = new(
                random.NextSingle(),
                random.NextSingle(),
                random.NextSingle()); // Full opacity
            //_polygonColors[i] = new(255,255,255, 100);

            // Triangulate the polygon using a simple fan method
            var indices = polygon.Indices;
            var triangleVertices = polygon.Vertices;
            for (var j = 0; j < indices.Length; j += 3)
            {
                var v0 = triangleVertices[indices[j]];
                var v1 = triangleVertices[indices[j + 1]];
                var v2 = triangleVertices[indices[j + 2]];
                vertices.Add(new(new(v0.X / _imageSize.Item1, aspectRatio * (1 - v0.Y / _imageSize.Item2)),
                    (uint)i));
                vertices.Add(new(new(v1.X / _imageSize.Item1, aspectRatio * (1 - v1.Y / _imageSize.Item2)),
                    (uint)i));
                vertices.Add(new(new(v2.X / _imageSize.Item1, aspectRatio * (1 - v2.Y / _imageSize.Item2)),
                    (uint)i));
            }
        }

        return vertices;
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
            BufferDescription = new((uint)width,(uint) height, new(60, 1), Format.R8G8B8A8_UNorm),
            OutputWindow = hwnd,
            Windowed = true,
            SampleDescription = new(1, 0),
            SwapEffect = SwapEffect.Discard,
            BufferUsage = Usage.RenderTargetOutput
        };

        D3D11.D3D11CreateDeviceAndSwapChain(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.None,
            [FeatureLevel.Level_11_0],
            swapChainDesc,
            out _swapChain!,
            out _device!,
            out _,
            out _context!
        ).CheckError();
        
        using (var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0))
        {
            _renderTargetView = _device.CreateRenderTargetView(backBuffer);
        }
        
        InputElementDescription[] inputElementDescs =
        [
            new ("POSITION", 0, Format.R32G32_Float, 0, 0),
            new ("POLYGON_ID", 0, Format.R32_UInt, 8, 0)
        ];

        var vertexShaderByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "VSMain", "vs_5_0");
        var pixelShaderByteCode = ID3DRenderer.CompileBytecode("Triangle.hlsl", "PSMain", "ps_5_0");

        _vertexShader = _device.CreateVertexShader(vertexShaderByteCode.Span);
        _pixelShader = _device.CreatePixelShader(pixelShaderByteCode.Span);
        _inputLayout = _device.CreateInputLayout(inputElementDescs, vertexShaderByteCode.Span);
        _constantBuffer = _device.CreateBuffer(new((uint)Unsafe.SizeOf<Constants>(), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
        _vertexBuffer = _device.CreateBuffer(_vertices.ToArray(), BindFlags.VertexBuffer);
        var colorBufferDesc = new BufferDescription
        {
            // Total size of the buffer in bytes.
            ByteWidth = (uint) (_polygonColors.Length * Unsafe.SizeOf<Color4>()),
        
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
        _vertices = null!;
        _polygonColors = null!;
        
        _context.RSSetViewport(new (width, height));
        _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _context.VSSetShader(_vertexShader);
        _context.VSSetConstantBuffer(0, _constantBuffer);
        _context.PSSetShader(_pixelShader);
        _context.PSSetShaderResource(0, _colorLookupView);
        _context.IASetInputLayout(_inputLayout);
        _context.IASetVertexBuffer(0, _vertexBuffer, VertexPositionId2D.SizeInBytes);
        _context.OMSetBlendState(null);
    }

    public void Render()
    {
        unsafe
        {
            if (_renderTargetView == null || _context == null || _swapChain == null || _parent == null) return;

            var aspectRatio = (float)(_parent.ActualWidth / _parent.ActualHeight);
            var view = Matrix4x4.CreateTranslation(_pan.X, _pan.Y, 0);
            var projection = Matrix4x4.CreateOrthographic(2.0f * aspectRatio / _zoom, 2.0f / _zoom, -1.0f, 1.0f);
            _constants.WorldViewProjection = Matrix4x4.Transpose(view * projection);

            var mapped = _context.Map(_constantBuffer, MapMode.WriteDiscard);
            Unsafe.Write(mapped.DataPointer.ToPointer(), _constants);
            _context.Unmap(_constantBuffer);

            _context.ClearRenderTargetView(_renderTargetView, new (0.1f, 0.1f, 0.1f));

            _context!.OMSetRenderTargets(_renderTargetView);
            _context.Draw(_vertexCount,0);
            _swapChain!.Present(1, PresentFlags.None);
        }
    }

    public void SetupEvents(Border parent)
    {
        parent.MouseWheel += MouseWheel;
        parent.MouseLeftButtonDown += MouseLeftButtonDown;
        parent.MouseLeftButtonUp += MouseLeftButtonUp;
        parent.MouseMove += MouseMove;
        parent.MouseRightButtonDown += OnMouseRightButtonDown;
        _parent = parent;
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
    
    private void MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _zoom *= e.Delta > 0 ? 1.1f : 1 / 1.1f;
    }

    private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not IInputElement surface) return;
        _isPanning = true;
        _lastMousePosition = e.GetPosition(surface);
        surface.CaptureMouse();
    }

    private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not IInputElement surface) return;
        _isPanning = false;
        surface.ReleaseMouseCapture();
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || sender is not FrameworkElement surface) return;

        var currentMousePosition = e.GetPosition(surface);
        var delta = currentMousePosition - _lastMousePosition;

        _pan.X += (float)(delta.X * 2 / (surface.ActualWidth * _zoom));
        _pan.Y -= (float)(delta.Y * 2 / (surface.ActualHeight * _zoom));

        _lastMousePosition = currentMousePosition;
    }

    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var list = new Color4[_polygons.Length];
        var rand = new Random();
        for (var i = 0; i < _polygons.Length; i++)
        {
           list[i] = new (rand.NextSingle(), rand.NextSingle(), rand.NextSingle());
        }
        UpdateColors(list);
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
                Unsafe.CopyBlockUnaligned(mapped.DataPointer.ToPointer(), pColors, (uint)(newColors.Length * Unsafe.SizeOf<Color4>()));
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