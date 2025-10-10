using System.Drawing.Imaging;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Helper;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

public unsafe class MapTracing : IDisposable
{
    private const int ALPHA = 255 << 24;
    private const int OUTSIDE_COLOR = 0x000000;
    private int _width;
    private int _height;
    private int _stride;
    private IntPtr _scan0;
    private Bitmap _bitmap;
    private BitmapData _bitmapData;
    
    public Dictionary<(int, int), Node> NodeCache { get; } = new();

    public MapTracing(Bitmap bmp)
    {
        _bitmap = bmp;
        _bitmapData = _bitmap.LockBits(new(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
        _width = _bitmapData.Width;
        _height = _bitmapData.Height;
        _stride = _bitmapData.Stride;
        _scan0 = _bitmapData.Scan0;
    }
    
    private int GetColor(int x, int y)
    {
        var row = (byte*)_scan0 + y * _stride;
        var xTimesThree = x * 3;
        return ALPHA
               |
               (row[xTimesThree + 2] << 16)
               |
               (row[xTimesThree + 1] << 8)
               |
               row[xTimesThree];
    }
    
    private int GetColor(Vector2 pos)
    {
        return GetColor(pos.X, pos.Y);
    }

    private int GetColorWithOutsideCheck(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return OUTSIDE_COLOR;
        }

        return GetColor(x, y);
    }
    
    private int GetColorWithOutsideCheck(Vector2 pos)
    {
        return GetColorWithOutsideCheck(pos.X, pos.Y);
    }
    
    private void LinkNodes(Node a, Node b, BorderSegment segment)
    {
            a.Segments[2].Node = b;
            a.Segments[2].Segment = new(segment, true);
            b.Segments[0].Node = a;
            b.Segments[0].Segment = new(segment, false);
    }
    
    private void TraceImageEdges()
    {
        var lastColor = GetColorWithOutsideCheck(0, 0);
        
        Node? firstNode = null;
        var firstSegment = new BorderSegment();
        firstSegment.Points.Add(new (0, 0));
        
        Node? lastNode = null;
        
        var currentSegment = firstSegment;

        int x = 0, y = 1;
        
        // Left Edge (top to bottom)
        while (y < _height)
            FinalizeSegment(x, y++, Direction.East);
        
        currentSegment.Points.Add(new (x, y));
        
        // Bottom Edge (left to right)
        while (x < _width)
            FinalizeSegment(x++, y, Direction.North);
        
        currentSegment.Points.Add(new (x, y));
        
        // Right Edge (bottom to top)
        while (y > 0)
            FinalizeSegment(x, y--, Direction.West);
        
        currentSegment.Points.Add(new (x, y));
        
        // Top Edge (right to left)
        while (x > 0)
            FinalizeSegment(x--, y, Direction.South);
        
        currentSegment.Points.AddRange(firstSegment.Points);

        //TODO: @MelCo: This currently does not handle the case where there are no nodes on the border.
        if (lastNode == null || firstNode == null)
        {
            throw new InvalidOperationException(
                "No nodes were on the border of the image. We currently do not support maps without nodes on the border.");
        }

        LinkNodes(lastNode, firstNode, currentSegment);
        
        return;
        
        void FinalizeSegment(int x, int y, Direction direction)
        {
            var color = GetColor(x, y);
            
            if(lastColor == color)
                return;
            
            // Color changed, finalize the current segment and start a new one
            
            var newSegment = new BorderSegment();

            Node node = new(x, y, direction, true);
            
            // First node found
            
            if (lastNode is null)
                firstNode = node;
            else
                LinkNodes(lastNode, node, currentSegment);
            
            // Update caches and references
            
            NodeCache.Add((x, y), node);
            lastNode = node;
            currentSegment = newSegment;
            lastColor = color;
        }
    }

    public List<Polygon> Trace()
    {
        return [];
    }
    
    #region Disposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _bitmap.UnlockBits(_bitmapData);
            _bitmapData = null!;
            _bitmap = null!;
        }

        // If you had unmanaged memory that YOU allocated manually,
        // you'd release it here, regardless of the value of 'disposing'.

        _disposed = true;
    }

    #endregion
}