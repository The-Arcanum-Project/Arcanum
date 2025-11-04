using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class TextboxWithPlaceholder : UserControl
{
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(TextboxWithPlaceholder));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextboxWithPlaceholder));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    

    public TextboxWithPlaceholder()
    {
        InitializeComponent();
    }
}