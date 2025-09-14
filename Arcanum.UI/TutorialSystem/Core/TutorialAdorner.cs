using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.TutorialSystem.Core;

public class TutorialAdorner : Adorner
{
    private readonly FrameworkElement? _targetControl;
    public bool BlockTargetInteraction = false;
    private readonly bool _disableInteraction;
    
    
    
    public TutorialAdorner(UIElement adornedElement, FrameworkElement targetControl, bool isInteractive) : base(adornedElement)
    {
        _targetControl = targetControl;
        // Make the adorner visible and able to receive mouse events.
        IsHitTestVisible = true;
        _disableInteraction = !isInteractive;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_targetControl is null) return;

        // Get the position of the control to highlight, relative to the adorned element.
        var position = _targetControl.TranslatePoint(new Point(0, 0), AdornedElement);
        var targetRect = new Rect(position, _targetControl.RenderSize);

        // Create a geometry for the overlay (a full rectangle with a cutout for the target).
        var fullRectGeometry = new RectangleGeometry(new Rect(AdornedElement.RenderSize));
        Geometry cutoutGeometry;
        if (_targetControl is Border border)
        {
            cutoutGeometry = new RectangleGeometry(targetRect, border.CornerRadius.TopLeft, border.CornerRadius.TopRight);
        }else
            cutoutGeometry = new RectangleGeometry(targetRect);
        
        var overlayGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, fullRectGeometry, cutoutGeometry);

        // Draw the semi-transparent black overlay.
        drawingContext.DrawGeometry(new SolidColorBrush(Color.FromArgb(180, 0, 0,0)), null, overlayGeometry);
        if (_disableInteraction)
            drawingContext.DrawGeometry(new SolidColorBrush(Color.FromArgb(20, 0x0c, 0x2e,0x31)), null, cutoutGeometry);
    }

    // Block mouse clicks outside the highlighted area.
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (_targetControl == null) return;
        var pt = e.GetPosition(this);
        var position = _targetControl.TranslatePoint(new Point(0, 0), AdornedElement);
        var targetRect = new Rect(position, _targetControl.RenderSize);

        // If the click is outside the target rectangle, handle the event
        // to prevent it from reaching the UI underneath.
        if (!targetRect.Contains(pt))
        {
            e.Handled = true;
        }
    }
}