using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace Arcanum.UI.TutorialSystem.Core;

public class TutorialAdorner : Adorner
{
    private readonly FrameworkElement? _interactiveTarget;
    private readonly List<FrameworkElement> _highlightTargets;
    
    private static readonly Brush OverlayBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
    private static readonly Brush NonInteractiveBrush = new SolidColorBrush(Colors.Transparent);
    
    public TutorialAdorner(UIElement adornedElement, List<FrameworkElement> highlightTargets,
        FrameworkElement? targetControl) : base(adornedElement)
    {
        _highlightTargets = highlightTargets;
        _interactiveTarget = targetControl;
    }

    public TutorialAdorner(UIElement adornedElement, List<FrameworkElement> highlightTargets) : this(adornedElement, highlightTargets, null)
    { }
    
    public TutorialAdorner(UIElement adornedElement, FrameworkElement interactiveTarget) : this(adornedElement, [], interactiveTarget)
    { }
    
    
    private Geometry GetControlGeometry(UIElement control)
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

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        
        var controlGeometry = GetControlGeometry(AdornedElement);
        Geometry? blockoutGeometry = null;
        foreach (var cutoutGeometry in _highlightTargets.Select(GetControlGeometry))
        {
            controlGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, controlGeometry, cutoutGeometry);
            blockoutGeometry = blockoutGeometry is null ? cutoutGeometry : new CombinedGeometry(GeometryCombineMode.Union, blockoutGeometry, cutoutGeometry);
        }
        
        Geometry? interactiveGeometry = null;
        
        if (_interactiveTarget is not null)
        {
            interactiveGeometry = GetControlGeometry(_interactiveTarget);
            controlGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, controlGeometry, interactiveGeometry);
            blockoutGeometry = blockoutGeometry is null ? null : new CombinedGeometry(GeometryCombineMode.Exclude, blockoutGeometry, interactiveGeometry);
        }

        // Draw the semi-transparent black overlay.
        drawingContext.DrawGeometry(OverlayBrush, null, controlGeometry);
        if (blockoutGeometry is not null)
            drawingContext.DrawGeometry(NonInteractiveBrush, null, blockoutGeometry);
        if (interactiveGeometry is not null)
            drawingContext.DrawGeometry(null, new(Brushes.Red, 2), interactiveGeometry);
    }

    // Block mouse clicks outside the highlighted area.
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (_interactiveTarget is null) return;
        var pt = e.GetPosition(this);
        var position = _interactiveTarget.TranslatePoint(new Point(0, 0), AdornedElement);
        var targetRect = new Rect(position, _interactiveTarget.RenderSize);

        // If the click is outside the target rectangle, handle the event
        // to prevent it from reaching the UI underneath.
        if (!targetRect.Contains(pt))
        {
            e.Handled = true;
        }
    }
}