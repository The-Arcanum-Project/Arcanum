using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arcanum.UI.TutorialSystem.Core;

public static class TutorialUtil
{
    public static RectangleGeometry GetControlGeometry(UIElement control, UIElement AdornedElement)
    {
        var position = control.TranslatePoint(new Point(0, 0), AdornedElement);
        var targetRect = new Rect(position, control.RenderSize);
        if (control is Border border)
        {
            return new RectangleGeometry(targetRect, border.CornerRadius.TopLeft, border.CornerRadius.TopRight);
        }
        if (VisualTreeHelper.GetChild(control, 0) is Border { Parent: null } child)
        {
            return new RectangleGeometry(targetRect, child.CornerRadius.TopLeft, child.CornerRadius.TopRight);
        }
        
        
        return new RectangleGeometry(targetRect);
    }
}