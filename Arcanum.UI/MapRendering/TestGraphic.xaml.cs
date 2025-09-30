using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Parsing.MapParsing;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.Wpf;
using Colors = System.Windows.Media.Colors;
using Point = System.Windows.Point;

namespace RenderingPain;

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

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class TestGraphic
{
    private ID3D11DeviceContext? _context;
    private const int TriangleCount = 5_000_000;
    private const int MaxFps = 60;
    private static readonly double s_minFrameTime = 1.0 / MaxFps;

    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11Buffer? _vertexBuffer;
    private ID3D11Buffer? _constantBuffer;
    private Constants _constants;

    private int _frameCount;
    private double _elapsedSeconds;
    private readonly Stopwatch _stopwatch = new();
    private double _lastRenderTime;

    private ID3D11Buffer? _colorLookupBuffer;
    private ID3D11ShaderResourceView? _colorLookupView;
    private List<VertexPositionId2D> _vertices;
    private List<Color4> _polygonColors;

    private int _numberOfPolygons = 0;
    
    private Vector2 _pan = Vector2.Zero;
    private float _zoom = 1.0f;
    private Point _lastMousePosition;
    private bool _isPanning;
    private (int,int) imageSize;
    public TestGraphic(List<Polygon> polygons, MapTracing tracer)
    {
        imageSize = tracer.ImageSize;
        SetVertices(polygons);
        InitializeComponent();
    }
    
    private int triangleCounter = 0;

    public void SetVertices(List<Polygon> polygons)
    {
        _vertices = new(polygons.Count * 3); // Initial capacity
        _polygonColors = new(polygons.Count);
        var rand = new Random(0);
        var aspectRatio = imageSize.Item2 / (float)imageSize.Item1;
        var allPolygonVertices = new System.Collections.Concurrent.ConcurrentBag<(List<VertexPositionId2D> vertices, uint polygonId)>();

        // Pre-calculate colors
        for (var i = 0; i < polygons.Count; i++)
        {
            var r = (byte)rand.Next(0, 256);
            var g = (byte)rand.Next(0, 256);
            var b = (byte)rand.Next(0, 256);
            _polygonColors.Add(new(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f));
        }
        Parallel.For(
            0,
            polygons.Count,
            // Thread-local initialization
            () => new List<VertexPositionId2D>(),

            // Loop body
            (index, state, localList) =>
            {
                var polygon = polygons[index];
                var (triangleVertices, indices) = polygon.Tesselate();
                // Create vertex buffer
                for (var i = 0; i < indices.Count; i += 3)
                {
                    var v0 = triangleVertices[indices[i]];
                    var v1 = triangleVertices[indices[i + 1]];
                    var v2 = triangleVertices[indices[i + 2]];
                    localList.Add(new(new(v0.X / imageSize.Item1, aspectRatio * (1 - v0.Y / imageSize.Item2)),
                        (uint)index));
                    localList.Add(new(new(v1.X / imageSize.Item1, aspectRatio * (1 - v1.Y / imageSize.Item2)),
                        (uint)index));
                    localList.Add(new(new(v2.X / imageSize.Item1, aspectRatio * (1 - v2.Y / imageSize.Item2)),
                        (uint)index));
                }
                return localList;
            },

            // Finalizer: merge into global collection
            localList =>
            {
                lock (_vertices)
                    _vertices.AddRange(localList);
            }
        );
        triangleCounter = _vertices.Count / 3;
        _numberOfPolygons = polygons.Count;
    }

    private void DrawingSurface_LoadContent(object? sender, DrawingSurfaceEventArgs e)
    {
        _context = e.Context;
        InputElementDescription[] inputElementDescs =
        [
            new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0),
            new InputElementDescription("POLYGON_ID", 0, Format.R32_UInt, 8, 0)
        ];

        ReadOnlyMemory<byte> vertexShaderByteCode = CompileBytecode("Triangle.hlsl", "VSMain", "vs_5_0");
        ReadOnlyMemory<byte> pixelShaderByteCode = CompileBytecode("Triangle.hlsl", "PSMain", "ps_5_0");

        _vertexShader = e.Device.CreateVertexShader(vertexShaderByteCode.Span);
        _pixelShader = e.Device.CreatePixelShader(pixelShaderByteCode.Span);
        _inputLayout = e.Device.CreateInputLayout(inputElementDescs, vertexShaderByteCode.Span);
        _constantBuffer = e.Device.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Constants>(), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));

        _vertexBuffer = e.Device.CreateBuffer(_vertices.ToArray(), BindFlags.VertexBuffer);

        // Create and bind the color lookup table
        var colorBufferDesc = new BufferDescription
        {
            // Total size of the buffer in bytes.
            ByteWidth = (uint) (_polygonColors.Count * Unsafe.SizeOf<Color4>()),
        
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

        // 2. Create the buffer with the new description.
        // Note we are NOT providing initial data here. The buffer starts empty.
        _colorLookupBuffer = e.Device.CreateBuffer(colorBufferDesc);
        _colorLookupView = e.Device.CreateShaderResourceView(_colorLookupBuffer);

        // 3. Populate the buffer with the initial colors using our update method.
        UpdateColors(_polygonColors);


        _vertices.Clear();
        _vertices = null!;
        _polygonColors.Clear();
        _polygonColors = null;

        e.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        e.Context.VSSetShader(_vertexShader);
        e.Context.VSSetConstantBuffer(0, _constantBuffer);
        e.Context.PSSetShader(_pixelShader);
        e.Context.PSSetShaderResource(0, _colorLookupView); // Set the shader resource
        e.Context.IASetInputLayout(_inputLayout);
        e.Context.IASetVertexBuffer(0, _vertexBuffer, VertexPositionId2D.SizeInBytes);
        e.Context.OMSetBlendState(null);

        _stopwatch.Start();
    }

    private void DrawingSurface_UnloadContent(object sender, DrawingSurfaceEventArgs e)
    {
        _stopwatch.Stop();
        _vertexShader?.Dispose();
        _pixelShader?.Dispose();
        _inputLayout?.Dispose();
        _vertexBuffer?.Dispose();
        _constantBuffer?.Dispose();
        _colorLookupBuffer?.Dispose();
        _colorLookupView?.Dispose();
    }
    int renderCount = 0;
    private void DrawingSurface_Draw(object? sender, DrawEventArgs e)
    {
        if(renderCount++ % 5 != 0)
        {
            return;
        }
        renderCount = 1;

        unsafe
        {
            _frameCount++;
            var totalSeconds = _stopwatch.Elapsed.TotalSeconds;
            if (totalSeconds - _elapsedSeconds >= 1.0)
            {
                double fps = _frameCount / (totalSeconds - _elapsedSeconds);
                double frameTime = ((totalSeconds - _elapsedSeconds) * 1000.0) / _frameCount;
                Console.WriteLine($"FPS: {fps:F2}, Frame Time: {frameTime:F3} ms, Triangles: {triangleCounter}");

                _frameCount = 0;
                _elapsedSeconds = totalSeconds;
            }
            float aspectRatio = (float)(ActualWidth / ActualHeight);
            var view = Matrix4x4.CreateTranslation(_pan.X, _pan.Y, 0);
            var projection = Matrix4x4.CreateOrthographic(2.0f * aspectRatio / _zoom, 2.0f / _zoom, -1.0f, 1.0f);
            _constants.WorldViewProjection = Matrix4x4.Transpose(view * projection);

            var mapped = e.Context.Map(_constantBuffer, MapMode.WriteDiscard);
            Unsafe.Write(mapped.DataPointer.ToPointer(), _constants);
            e.Context.Unmap(_constantBuffer);

            e.Context.ClearRenderTargetView(e.Surface.ColorTextureView, new Color4(0.1f, 0.1f, 0.1f));

            if (e.Surface.DepthStencilView != null)
            {
                e.Context.ClearDepthStencilView(e.Surface.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            }

            e.Context.Draw(TriangleCount * 3, 0);
        }
    }

    private static ReadOnlyMemory<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
    {
        var uri = new Uri("/Arcanum_UI;component/MapRendering/Shaders/Triangle.hlsl", UriKind.RelativeOrAbsolute);
        using Stream stream = Application.GetResourceStream(uri).Stream;
        using StreamReader reader = new StreamReader(stream);
        string shaderSource = reader.ReadToEnd();
        
        
        
        return Compiler.Compile(shaderSource, entryPoint, shaderName, profile);
    }

    private void DrawingSurface_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _zoom *= e.Delta > 0 ? 1.1f : 1 / 1.1f;
    }

    private void DrawingSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not IInputElement surface) return;
        _isPanning = true;
        _lastMousePosition = e.GetPosition(surface);
        surface.CaptureMouse();
    }

    private void DrawingSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not IInputElement surface) return;
        _isPanning = false;
        surface.ReleaseMouseCapture();
    }

    private void DrawingSurface_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || sender is not FrameworkElement surface) return;

        var currentMousePosition = e.GetPosition(surface);
        var delta = currentMousePosition - _lastMousePosition;

        _pan.X += (float)(delta.X * 2 / (surface.ActualWidth * _zoom));
        _pan.Y -= (float)(delta.Y * 2 / (surface.ActualHeight * _zoom));

        _lastMousePosition = currentMousePosition;
    }
    
    public void UpdateColors(List<Color4> newColors)
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
                Unsafe.CopyBlockUnaligned(mapped.DataPointer.ToPointer(), pColors, (uint)(newColors.Count * Unsafe.SizeOf<Color4>()));
            }

            // 3. Unmap the buffer to commit the changes.
            _context.Unmap(_colorLookupBuffer);
        }
    }

    private void UIElement_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var list = new List<Color4>(_numberOfPolygons);
        var rand = new Random();
        for (var i = 0; i < _numberOfPolygons; i++)
        {
            list.Add(new(rand.Next(0, 256) / 255.0f, rand.Next(0, 256) / 255.0f, rand.Next(0, 256) / 255.0f, 1.0f));
        }
        UpdateColors(list);
    }
}