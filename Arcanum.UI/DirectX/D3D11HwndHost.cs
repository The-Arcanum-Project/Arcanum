using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Arcanum.UI.DirectX;

public partial class D3D11HwndHost : HwndHost
{
    private readonly ID3DRenderer _renderer;
    private IntPtr _hwnd;
    private bool _isRendererInitialized;

    private readonly Stopwatch _stopwatch = new();
    private long _lastFrameTime;
    private int _frameCount;

    public static readonly DependencyProperty FpsProperty = DependencyProperty.Register(
        nameof(Fps), typeof(double), typeof(D3D11HwndHost), new(.0));

    public double Fps
    {
        get => (double)GetValue(FpsProperty);
        set => SetValue(FpsProperty, value);
    }

    public static readonly DependencyProperty FrameTimeProperty = DependencyProperty.Register(
        nameof(FrameTime), typeof(double), typeof(D3D11HwndHost), new(.0));

    public double FrameTime
    {
        get => (double)GetValue(FrameTimeProperty);
        set => SetValue(FrameTimeProperty, value);
    }
    
    // P/Invoke for SetWindowPos to resize the native window
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;

    public D3D11HwndHost(ID3DRenderer renderer)
    {
        _renderer = renderer;
        Unloaded += OnUnloaded;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        // Create the native window with a minimal size.
        // It will be resized correctly in OnRenderSizeChanged.
        _hwnd = CreateWindowEx(
            0, "static", "",
            0x40000000 | 0x10000000, // WS_CHILD | WS_VISIBLE
            0, 0, 1, 1, // Start with 1x1 size
            hwndParent.Handle,
            IntPtr.Zero, IntPtr.Zero, 0);

        // Start the rendering loop. The Render() method has null checks,
        // so it won't do anything until the renderer is initialized.
        _stopwatch.Start();
        _lastFrameTime = _stopwatch.ElapsedMilliseconds;
        CompositionTarget.Rendering += OnRendering;

        return new HandleRef(this, _hwnd);
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        // This will be called on every frame, but will only draw
        // after _isRendererInitialized is true.
        long currentTime = _stopwatch.ElapsedMilliseconds;
        long delta = currentTime - _lastFrameTime;
        _frameCount++;
        
        if (delta >= 1000) // Update once per second
        {
            Fps = _frameCount;
            FrameTime = (double)delta / _frameCount; // Average frame time over the last second
            _frameCount = 0;
            _lastFrameTime = currentTime;
        }
        _renderer.Render();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        var newWidth = (int)sizeInfo.NewSize.Width;
        var newHeight = (int)sizeInfo.NewSize.Height;
        
        // Ensure we have a valid size
        if (newWidth <= 0 || newHeight <= 0)
            return;

        // Resize the native window to match the HwndHost control size
        if (!SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, newWidth, newHeight, SWP_NOMOVE | SWP_NOZORDER))
        {
            Debugger.Break();
            return;   
        }

        if (!_isRendererInitialized)
        {
            // First time a valid size is received, initialize the renderer.
            _renderer.Initialize(_hwnd, newWidth, newHeight);
            _isRendererInitialized = true;
        }
        else
        {
           _renderer.Resize(newWidth, newHeight);
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        CompositionTarget.Rendering -= OnRendering;
        _renderer.Dispose();
        if (!DestroyWindow(hwnd.Handle))
        {
            throw new InvalidOperationException("Could not destroy window");
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Dispose();
    }

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = false;
        return IntPtr.Zero;
    }

    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr CreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width, int height, IntPtr hwndParent, IntPtr hMenu, IntPtr hInst, IntPtr pvParam);

    [LibraryImport("user32.dll", EntryPoint = "DestroyWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hwnd);
}