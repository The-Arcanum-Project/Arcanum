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
      var window = new GraphWindow();

      window.Loaded += (sender, _) =>
      {
         if (sender is not GraphWindow loadedWindow)
            return;

         graph.ApplyLayout((float)loadedWindow.MainCanvas.ActualWidth, (float)loadedWindow.MainCanvas.ActualHeight);
         graph.DrawToCanvas(loadedWindow.MainCanvas);
      };

      window.SizeChanged += (sender, _) =>
      {
         if (sender is not GraphWindow resizedWindow)
            return;
         if (!(resizedWindow.MainCanvas.ActualWidth > 0) || !(resizedWindow.MainCanvas.ActualHeight > 0))
            return;

         graph.ApplyLayout((float)resizedWindow.MainCanvas.ActualWidth, (float)resizedWindow.MainCanvas.ActualHeight);
         graph.DrawToCanvas(resizedWindow.MainCanvas);
      };

      window.Show();
   }
}