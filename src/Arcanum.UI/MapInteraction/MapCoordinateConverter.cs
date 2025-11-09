using System.Numerics;
using System.Windows;
using Arcanum.UI.Components.UserControls;
using Vector = System.Numerics.Vector;

namespace Arcanum.UI.MapInteraction;

public class MapCoordinateConverter(MapControl mapControl, (int width, int height) imageSize)
{
    public readonly float ImageAspectRatio = (float)imageSize.height / imageSize.width;

    public record struct PositionInformation(Vector2 Map, Vector2 Norm, Vector2 Ndc, Vector2 Pixel);

    public PositionInformation CurrentPosition;
    
    /// <summary>
    /// Invalidates the cached position information based on a new pixel point.
    /// </summary>
    /// <param name="pixelPoint">New point in pixel coordinates</param>
    public void InvalidateCache(Vector2 pixelPoint)
    {
        var ndcPoint = PixelToNdc(pixelPoint);
        var normPoint = NdcToNorm(ndcPoint);
        var mapPoint = NormToMap(normPoint);
        CurrentPosition = new (
            Map: mapPoint,
            Norm: normPoint,
            Ndc: ndcPoint,
            Pixel: pixelPoint
        );
        //TODO: @MelCo: Remove this later
        mapControl.CurrentPos = mapPoint;
    }

    public void InvalidateCache(Point pixelPoint)
    {
        InvalidateCache(new Vector2((float)pixelPoint.X, (float)pixelPoint.Y));
    }

    #region Composite Conversions

    public Vector2 MapToNdc(Vector2 mapPoint)
    {
        return NormToNdc(MapToNorm(mapPoint));
    }
    
    public Vector2 NdcToMap(Vector2 ndcPoint)
    {
        return NormToMap(NdcToNorm(ndcPoint));
    }
    
    public Vector2 PixelToMap(Vector2 pixelPoint)
    {
        return NormToMap(NdcToNorm(PixelToNdc(pixelPoint)));
    }

    public Vector2 MapToPixel(Vector2 mapPoint)
    {
        return MapToNdc(NdcToPixel(mapPoint));
    }

    #endregion

    #region Low-Level Conversions

    /// <summary>
    /// Converts a screen point to Normalized Device Coordinates (NDC).
    /// </summary>
    /// <remarks>
    /// NDC coordinates range from -1 to 1 in both X and Y axes.
    /// </remarks>
    /// <example>
    /// PixelToNdc(e.GetPosition(HwndHostContainer));
    /// </example>
    /// <param name="pixelPoint">Point relative the HwndHostContainer of the MapControl</param>
    /// <returns></returns>
    public Vector2 PixelToNdc(Vector2 pixelPoint)
    {
        return new(
            pixelPoint.X / (float)mapControl.HwndHostContainer.ActualWidth * 2.0f - 1.0f,
            1.0f - pixelPoint.Y / (float)mapControl.HwndHostContainer.ActualHeight * 2.0f
        );
    }

    /// <summary>
    /// Converts Normalized Device Coordinates (NDC) to a screen point.
    /// </summary>
    /// <param name="ndcPoint">Point in NDC coordinates</param>
    public Vector2 NdcToPixel(Vector2 ndcPoint)
    {
        return new Vector2(
            (ndcPoint.X + 1.0f) / 2.0f * (float)mapControl.HwndHostContainer.ActualWidth,
            (1.0f - ndcPoint.Y) / 2.0f * (float)mapControl.HwndHostContainer.ActualHeight
        );
    }

    /// <summary>
    /// Converts Normalized Device Coordinates (NDC) to map coordinates.
    /// </summary>
    /// <param name="ndcPoint">Point in NDC coordinates</param>
    /// <returns>>Point in absolute map coordinates</returns>
    public Vector2 NdcToNorm(Vector2 ndcPoint)
    {
        var width = (float)mapControl.HwndHostContainer.ActualWidth;
        var height = (float)mapControl.HwndHostContainer.ActualHeight;
        var aspectRatio = width / height;

        var zoomRatio = ImageAspectRatio / (mapControl.LocationRenderer.Zoom * 2);

        var worldX = ndcPoint.X * (aspectRatio * zoomRatio) + mapControl.LocationRenderer.Pan.X;
        var worldY = ndcPoint.Y * zoomRatio / ImageAspectRatio + 1f - mapControl.LocationRenderer.Pan.Y;

        return new(worldX, 1 - worldY);
    }

    public float NdcToMap(float ndcLength)
    {
        var width = (float)mapControl.HwndHostContainer.ActualWidth;
        var height = (float)mapControl.HwndHostContainer.ActualHeight;
        
        var zoomRatio = ImageAspectRatio / (mapControl.LocationRenderer.Zoom * 2);

        return ndcLength * zoomRatio * imageSize.width;
    }

    /// <summary>
    /// Converts map coordinates to Normalized Device Coordinates (NDC).
    /// </summary>
    /// <param name="mapPoint">Point in absolute map coordinates</param>
    /// <returns>>Point in NDC coordinates</returns>
    public Vector2 NormToNdc(Vector2 mapPoint)
    {
        var width = (float)mapControl.HwndHostContainer.ActualWidth;
        var height = (float)mapControl.HwndHostContainer.ActualHeight;
        var aspectRatio = width / height;
        
        var zoomRatio = ImageAspectRatio / (mapControl.LocationRenderer.Zoom * 2);
        
        var worldX = mapPoint.X;
        var worldY = 1f - mapPoint.Y;
        
        var ndcX = (worldX - mapControl.LocationRenderer.Pan.X) / (aspectRatio * zoomRatio);
        var ndcY = (worldY - 1f + mapControl.LocationRenderer.Pan.Y) * ImageAspectRatio / zoomRatio;
        
        return new(ndcX, ndcY);
    }
    
    public Vector2 NormToMap(Vector2 normPoint)
    {
        return new Vector2(
            normPoint.X * imageSize.width,
            normPoint.Y * imageSize.height
        );
    }
    
    public Vector2 MapToNorm(Vector2 mapPoint)
    {
        return new Vector2(
            mapPoint.X / imageSize.width,
            mapPoint.Y / imageSize.height
        );
    }

    #endregion
}