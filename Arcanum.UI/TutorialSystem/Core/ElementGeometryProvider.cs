using System.Windows;
using System.Windows.Media;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.TutorialSystem.Core;

public class ElementGeometryProvider : IGeometryProvider
{
    public Thickness Padding;
    
    public FrameworkElement Element;

    public ElementGeometryProvider(FrameworkElement element, Thickness padding = default)
    {
        Padding = padding;
        Element = element;
    }

    public Geometry GetGeometry(UIElement adornerElement)
    {
        var rectangle = TutorialUtil.GetControlGeometry(Element, adornerElement);
        Rect r = rectangle.Rect;

// Expand by padding and move origin so padding is applied inside the rectangle
        r = new Rect(
            r.X - Padding.Left,
            r.Y - Padding.Top,
            r.Width + Padding.Left + Padding.Right,
            r.Height + Padding.Top + Padding.Bottom
        );

        rectangle.Rect = r;
        return rectangle;
    }
}