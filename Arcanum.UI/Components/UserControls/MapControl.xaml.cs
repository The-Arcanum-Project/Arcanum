using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _d3dHost = new D3D11HwndHost(new ExampleRenderer());
        HwndHostContainer.Child = _d3dHost;

        //_overlay = new OverlayWindow { Owner = this };

        // Use LayoutUpdated for initial positioning and then rely on location/size changed events
        DataContext = _d3dHost;
        //_overlay.Show();
    }
    private void HwndHostContainer_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        Console.WriteLine("Right click");
    }
}