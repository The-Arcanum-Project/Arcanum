using System.Runtime.CompilerServices;

namespace Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;

/// <summary>
/// Interface for color-getting strategies. Implemented by zero-size structs for JIT inlining.
/// </summary>
public interface IColorGetter
{
    int GetColor(nint scan0, int stride, int width, int height, int x, int y);
}

/// <summary>
/// Gets pixel color without bounds checking. Use only when coordinates are guaranteed in-bounds.
/// </summary>
public readonly struct UncheckedColorGetter : IColorGetter
{
    private const int ALPHA = 255 << 24;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int GetColor(nint scan0, int stride, int width, int height, int x, int y)
    {
        var pixel = (byte*)scan0 + y * stride + x * 3;
        return ALPHA | pixel[2] | (pixel[1] << 8) | (pixel[0] << 16);
    }
}

/// <summary>
/// Gets pixel color with bounds checking. Returns OUTSIDE_COLOR for out-of-bounds coordinates.
/// </summary>
public readonly struct CheckedColorGetter : IColorGetter
{
    private const int ALPHA = 255 << 24;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int GetColor(nint scan0, int stride, int width, int height, int x, int y)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height)
            return MapTracing.OUTSIDE_COLOR;

        var pixel = (byte*)scan0 + y * stride + x * 3;
        return ALPHA | pixel[2] | (pixel[1] << 8) | (pixel[0] << 16);
    }
}