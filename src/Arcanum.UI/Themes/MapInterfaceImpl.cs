using System.Drawing;
using Arcanum.UI.Components.UserControls.Map;
using Common.UI.Map;

namespace Arcanum.UI.Themes;

public class MapInterfaceImpl : IMapInterface
{
   public required MapControl MapControl { get; init; }

   public void PanTo(float x, float y)
   {
      MapControl.PanTo(x, y);
   }

   public void PanTo(int x, int y)
   {
      MapControl.PanTo(x, y);
   }

   public void PanTo(RectangleF rect, float marginFraction = 3f)
   {
      MapControl.EnsureVisibleWithMargin(rect, marginFraction);
   }
}