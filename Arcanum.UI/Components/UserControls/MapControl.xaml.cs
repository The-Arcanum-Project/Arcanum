using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.UI.DirectX;

namespace Arcanum.UI.Components.UserControls;

public partial class MapControl : UserControl
{
    private D3D11HwndHost _d3dHost;
    
    public MapControl()
    {
        InitializeComponent();
    }
    
    private void HwndHostContainer_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Console.WriteLine("Right click");
    }

    public async void SetupRenderingAsync(Polygon[] polygons, (int, int) imageSize)
    {
        if (!IsLoaded)
        {
            throw new InvalidOperationException("MapControl must be loaded before calling SetupRendering");
        }
        
        var renderer = await LocationRenderer.CreateAsync(polygons, imageSize);
        
        _d3dHost = new (renderer, HwndHostContainer);
        HwndHostContainer.Child = _d3dHost;

        //_overlay = new OverlayWindow { Owner = this };

        // Use LayoutUpdated for initial positioning and then rely on location/size changed events
        DataContext = _d3dHost;
    }
}