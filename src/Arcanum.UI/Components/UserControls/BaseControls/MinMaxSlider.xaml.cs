#region

using System.Windows;

#endregion

namespace Arcanum.UI.Components.UserControls.BaseControls;

public sealed partial class MinMaxSlider
{
   public static readonly DependencyProperty MinimumProperty =
      DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(MinMaxSlider), new(0.0));

   public static readonly DependencyProperty MaximumProperty =
      DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(MinMaxSlider), new(100.0));

   public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue),
                                  typeof(double),
                                  typeof(MinMaxSlider),
                                  new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMinValueChanged));

   public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue),
                                  typeof(double),
                                  typeof(MinMaxSlider),
                                  new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMaxValueChanged));

   public MinMaxSlider()
   {
      InitializeComponent();
   }

   // --- Minimum Range (e.g. -100) ---
   public double Minimum
   {
      get => (double)GetValue(MinimumProperty);
      set => SetValue(MinimumProperty, value);
   }

   // --- Maximum Range (e.g. 100) ---
   public double Maximum
   {
      get => (double)GetValue(MaximumProperty);
      set => SetValue(MaximumProperty, value);
   }

   // --- The Selected Min Value ---
   public double MinValue
   {
      get => (double)GetValue(MinValueProperty);
      set => SetValue(MinValueProperty, value);
   }

   // --- The Selected Max Value ---
   public double MaxValue
   {
      get => (double)GetValue(MaxValueProperty);
      set => SetValue(MaxValueProperty, value);
   }

   private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var ctrl = (MinMaxSlider)d;
      var newVal = (double)e.NewValue;

      // Clamp: Min cannot be greater than Max
      if (newVal > ctrl.MaxValue)
         ctrl.MinValue = ctrl.MaxValue;
   }

   private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var ctrl = (MinMaxSlider)d;
      var newVal = (double)e.NewValue;

      // Clamp: Max cannot be less than Min
      if (newVal < ctrl.MinValue)
         ctrl.MaxValue = ctrl.MinValue;
   }
}