#region

using System.Windows;
using System.Windows.Media;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public sealed partial class LabeledSlider
{
   public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledSlider), new("Transfer %"));

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(double),
                                  typeof(LabeledSlider),
                                  new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

   public static readonly DependencyProperty SliderForegroundProperty =
      DependencyProperty.Register(nameof(SliderForeground), typeof(Brush), typeof(LabeledSlider), new(Brushes.Blue));

   public LabeledSlider()
   {
      InitializeComponent();
   }

   public string Label
   {
      get => (string)GetValue(LabelProperty);
      set => SetValue(LabelProperty, value);
   }

   public double Value
   {
      get => (double)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public Brush SliderForeground
   {
      get => (Brush)GetValue(SliderForegroundProperty);
      set => SetValue(SliderForegroundProperty, value);
   }
}