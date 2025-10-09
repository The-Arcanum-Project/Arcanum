using System.Windows;
using Arcanum.Core.CoreSystems.NUI.GraphDisplay;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class GraphWindow
{
   public GraphWindow()
   {
      InitializeComponent();
   }

   public static void ShowWindow(NodeGraph graph)
   {
      var window = new GraphWindow { Owner = Application.Current.MainWindow, };
      graph.DrawToCanvas(window.MainCanvas);
      window.Show();
   }
}