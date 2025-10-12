using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.UI.DirectX;

namespace Arcanum.UI.Components.UserControls;

public partial class MapControl : UserControl
{
    private D3D11HwndHost _d3dHost;
    
    public MapControl()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    public void SetupRendering(Polygon[] polygons)
    {
        if (!IsLoaded)
        {
            throw new InvalidOperationException("MapControl must be loaded before calling SetupRendering");
        }
        _d3dHost = new (new ExampleRenderer());
        HwndHostContainer.Child = _d3dHost;

        //_overlay = new OverlayWindow { Owner = this };

        // Use LayoutUpdated for initial positioning and then rely on location/size changed events
        DataContext = _d3dHost;
        //_overlay.Show();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // TODO check for data
    }
    
    private void HwndHostContainer_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Console.WriteLine("Right click");
    }
}