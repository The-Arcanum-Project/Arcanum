using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arcanum.UI.Components.StyleClasses;

/// <summary>
/// A TextBox with rounded corners.
/// </summary>
public class CorneredTextBox : TextBox
{
    public CornerRadius CornerRadiusValue
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadiusValue), typeof(CornerRadius), typeof(CorneredTextBox), new (new CornerRadius(3)));
    
    public static readonly DependencyProperty HighlightOnFocusProperty =
        DependencyProperty.Register(nameof(HighlightOnFocus), typeof(bool), typeof(CorneredTextBox), new (true));

    public static readonly DependencyProperty MaxAspectRatioProperty = DependencyProperty.Register(
        nameof(MaxAspectRatio), typeof(double), typeof(CorneredTextBox), new PropertyMetadata(double.NaN));

    public double MaxAspectRatio
    {
        get { return (double)GetValue(MaxAspectRatioProperty); }
        set { SetValue(MaxAspectRatioProperty, value); }
    }
    
    // For some reason the textbox bubble the events from underneath up
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        e.Handled = true;
    }
    
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        if (MaxAspectRatio <= 0 || double.IsNaN(MaxAspectRatio))
            return;

        double aspect = ActualWidth / ActualHeight;
        
        // aspect * h = w
        // w / aspect = h
        // w 
        
        if (aspect > MaxAspectRatio)
        {
            // Clamp width so we don't exceed the max aspect ratio
            Width = ActualHeight * MaxAspectRatio;
        }
    }
    
    public bool HighlightOnFocus
    {
        get => (bool)GetValue(HighlightOnFocusProperty);
        set => SetValue(HighlightOnFocusProperty, value);
    }
    
    
    
}