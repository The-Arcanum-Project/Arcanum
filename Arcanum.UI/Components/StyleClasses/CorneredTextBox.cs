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

    // For some reason the textbox bubble the events from underneath up
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        e.Handled = true;
    }
    
    public bool HighlightOnFocus
    {
        get => (bool)GetValue(HighlightOnFocusProperty);
        set => SetValue(HighlightOnFocusProperty, value);
    }

}