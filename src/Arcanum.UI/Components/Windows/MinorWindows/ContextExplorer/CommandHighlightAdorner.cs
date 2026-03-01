using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Arcanum.UI.Components.Windows.MinorWindows.ContextExplorer;

public class CommandHighlightAdorner : Adorner
{
   private static readonly SolidColorBrush FillBrush = Brushes.Transparent;
   private static readonly Pen OutlinePen = new(new SolidColorBrush(Colors.Red), 2);

   public CommandHighlightAdorner(UIElement adornedElement) : base(adornedElement)
   {
      IsHitTestVisible = false;

      OutlinePen.Freeze();
      FillBrush.Freeze();
   }

   protected override void OnRender(DrawingContext drawingContext)
   {
      var rect = new Rect(AdornedElement.RenderSize);
      drawingContext.DrawRoundedRectangle(FillBrush, OutlinePen, rect, 2, 2);
   }
}