using System.Drawing;

namespace Common.UI.Map;

public interface IMapInterface
{
   public void PanTo(float x, float y);
   public void PanTo(int x, int y);
   public void PanTo(RectangleF rect, float marginFraction = 3f);
}