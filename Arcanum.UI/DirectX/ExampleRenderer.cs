using System.IO;
using System.Numerics;
using System.Reflection;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Arcanum.UI.DirectX;

public readonly struct VertexPositionColor(in Vector3 position, in Color4 color)
{
    public static readonly unsafe uint SizeInBytes = (uint)sizeof(VertexPositionColor);

    public readonly Vector3 Position = position;
    public readonly Color4 Color = color;
}

public class ExampleRenderer : ID3DRenderer
{
    private const int TriangleCount = 5_000;
    private ID3D11Device? _device;
    private ID3D11DeviceContext? _context;
    private IDXGISwapChain? _swapChain;
    private ID3D11RenderTargetView? _renderTargetView;
    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11Buffer? _vertexBuffer;
    private uint _vertexCount;
    
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
            new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
        ];

        var vertexShaderByteCode = ID3DRenderer.CompileBytecode("ExampleTriangle.hlsl", "VSMain", "vs_5_0");
        var pixelShaderByteCode = ID3DRenderer.CompileBytecode("ExampleTriangle.hlsl", "PSMain", "ps_5_0");

        _vertexShader = _device.CreateVertexShader(vertexShaderByteCode.Span);
        _pixelShader = _device.CreatePixelShader(pixelShaderByteCode.Span);
        _inputLayout = _device.CreateInputLayout(inputElementDescs, vertexShaderByteCode.Span);
        //_constantBuffer = _device.CreateBuffer(new((uint)Unsafe.SizeOf<Constants>(), BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));

            var vertices = new List<VertexPositionColor>(TriangleCount * 3);

        // Define the total area to be covered by the triangles.
        const float areaWidth = 2.0f;
        const float areaHeight = 2.0f;
        const float startX = -1.0f;
        const float startY = -1.0f;

        if (TriangleCount > 0)
        {
            var numQuads = (TriangleCount + 1) / 2;
            var divisionsX = (int)Math.Ceiling(Math.Sqrt(numQuads));
            var divisionsY = (int)Math.Ceiling((double)numQuads / divisionsX);

            var quadWidth = areaWidth / divisionsX;
            var quadHeight = areaHeight / divisionsY;

            var trianglesGenerated = 0;

            for (var y = 0; y < divisionsY && trianglesGenerated < TriangleCount; ++y)
            {
                for (var x = 0; x < divisionsX && trianglesGenerated < TriangleCount; ++x)
                {
                    var currentX = startX + x * quadWidth;
                    var currentY = startY + y * quadHeight;

                    var bottomLeft = new Vector3(currentX, currentY, 0.0f);
                    var bottomRight = new Vector3(currentX + quadWidth, currentY, 0.0f);
                    var topRight = new Vector3(currentX + quadWidth, currentY + quadHeight, 0.0f);
                    var topLeft = new Vector3(currentX, currentY + quadHeight, 0.0f);

                    // First triangle (bottom-left, bottom-right, top-left) - CCW
                    vertices.Add(new(bottomLeft, new(1.0f, 0.0f, 0.0f)));
                    vertices.Add(new(topLeft, new(0.0f, 0.0f, 1.0f)));
                    vertices.Add(new(bottomRight, new(0.0f, 1.0f, 0.0f)));
                    trianglesGenerated++;

                    if (trianglesGenerated >= TriangleCount)
                    {
                        break;
                    }

                    // Second triangle (bottom-right, top-left, top-right) - Now also CCW!
                    vertices.Add(new(bottomRight, new(0.0f, 1.0f, 0.0f)));
                    // ---- The order of the next two lines has been swapped ----
                    vertices.Add(new(topLeft, new(0.0f, 0.0f, 1.0f)));
                    vertices.Add(new(topRight, new(1.0f, 0.0f, 0.0f)));
                    trianglesGenerated++;
                }
            }
        }
        _vertexCount = (uint)vertices.Count;
        _vertexBuffer = _device.CreateBuffer(vertices.ToArray(), BindFlags.VertexBuffer);
        _context.RSSetViewport(new Viewport(width, height));
        _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _context.VSSetShader(_vertexShader);
        _context.PSSetShader(_pixelShader);
        _context.IASetInputLayout(_inputLayout);
        _context.IASetVertexBuffer(0, _vertexBuffer, VertexPositionColor.SizeInBytes);
        _context.OMSetBlendState(null);

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