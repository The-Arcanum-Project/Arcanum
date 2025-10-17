using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Arcanum.UI.DirectX;

public partial class D3D11HwndHost : HwndHost
{
    private readonly ID3DRenderer _renderer;
    private IntPtr _hwnd;
    private readonly Border _hwndHostContainer;

    private readonly DispatcherTimer _resizeTimer;
    private SizeChangedInfo _currentSizeInfo = null!;

    public D3D11HwndHost(ID3DRenderer renderer, Border hwndHostContainer)
    {
        _hwndHostContainer = hwndHostContainer;
        _renderer = renderer;
        Unloaded += OnUnloaded;
        Loaded += OnLoaded;
        _resizeTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _resizeTimer.Tick += ResizeRenderer;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _renderer.Initialize(_hwnd, (int)_hwndHostContainer.ActualWidth, (int)_hwndHostContainer.ActualHeight);
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = CreateWindowEx(
            0, "static", "",
            0x40000000 | 0x10000000, // WS_CHILD | WS_VISIBLE
            0, 0, 1, 1, // Start with 1x1 size
            hwndParent.Handle,
            IntPtr.Zero, IntPtr.Zero, 0);
     
        return new(this, _hwnd);
    }

    public void Invalidate()
    {
        _renderer.Render();
    }

    private void ResizeRenderer(object? sender, EventArgs eventArgs)
    {
        if (_currentSizeInfo.NewSize is { Width: > 0, Height: > 0 })
        {
            _renderer.EndResize((int)_currentSizeInfo.NewSize.Width, (int)_currentSizeInfo.NewSize.Height);
            _renderer.Render();
        }
        _resizeTimer.Stop();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _currentSizeInfo = sizeInfo;

        var newWidth = (int)sizeInfo.NewSize.Width;
        var newHeight = (int)sizeInfo.NewSize.Height;

        // Ensure we have a valid size
        if (newWidth <= 0 || newHeight <= 0)
            return;
        
        _renderer.Resize(newWidth, newHeight);
        // Start the debouncing timer
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
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
        switch (msg)
        {
            case WM_PAINT:
                handled = true;
                ValidateRect(hwnd, IntPtr.Zero);
                _renderer.Render();
                return IntPtr.Zero;
        }
        
        handled = false;
        return IntPtr.Zero;
    }
    
    #region pInvoke
        private const int WM_PAINT = 0x000F;
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial void ValidateRect(IntPtr hWnd, IntPtr lpRect);

        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr CreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, int style,
            int x, int y, int width, int height, IntPtr hwndParent, IntPtr hMenu, IntPtr hInst, IntPtr pvParam);

        [LibraryImport("user32.dll", EntryPoint = "DestroyWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyWindow(IntPtr hwnd);
    #endregion
    
}