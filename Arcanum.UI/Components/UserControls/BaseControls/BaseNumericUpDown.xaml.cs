using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class BaseNumericUpDown
{
   public BaseNumericUpDown()
   {
      InitializeComponent();
      NudTextBox.Text = Value.ToString();
   }

   public static readonly DependencyProperty InnerBorderThicknessProperty =
      DependencyProperty.Register(nameof(InnerBorderThickness),
                                  typeof(Thickness),
                                  typeof(BaseNumericUpDown),
                                  new(new Thickness(1, 1, 1, 1)));

   public Thickness InnerBorderThickness
   {
      get => (Thickness)GetValue(InnerBorderThicknessProperty);
      set => SetValue(InnerBorderThicknessProperty, value);
   }

   public static readonly DependencyProperty InnerBorderBrushProperty =
      DependencyProperty.Register(nameof(InnerBorderBrush),
                                  typeof(Brush),
                                  typeof(BaseNumericUpDown),
                                  new(default(Brush)));

   public Brush InnerBorderBrush
   {
      get => (Brush)GetValue(InnerBorderBrushProperty);
      set => SetValue(InnerBorderBrushProperty, value);
   }

   public int MinValue
   {
      get => (int)GetValue(MinValueProperty);
      set => SetValue(MinValueProperty, value);
   }

   public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue),
                                  typeof(int),
                                  typeof(BaseNumericUpDown),
                                  new FrameworkPropertyMetadata(0, OnMinMaxChanged));

   public int MaxValue
   {
      get => (int)GetValue(MaxValueProperty);
      set => SetValue(MaxValueProperty, value);
   }

   public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue),
                                  typeof(int),
                                  typeof(BaseNumericUpDown),
                                  new FrameworkPropertyMetadata(100000, OnMinMaxChanged));

   public int Value
   {
      get => (int)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(int),
                                  typeof(BaseNumericUpDown),
                                  new FrameworkPropertyMetadata(10,
                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                OnValueChanged));

   private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (BaseNumericUpDown)d;

      if (control.MinValue > control.MaxValue)
         control.MinValue = control.MaxValue;

      if (control.Value < control.MinValue)
         control.Value = control.MinValue;
      else if (control.Value > control.MaxValue)
         control.Value = control.MaxValue;
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (BaseNumericUpDown)d;
      var newValue = (int)e.NewValue;

      // Clamp value once here, but do NOT set Value again inside OnValueChanged
      if (newValue < control.MinValue)
         newValue = control.MinValue;
      else if (newValue > control.MaxValue)
         newValue = control.MaxValue;

      // Update TextBox text only if different
      if (control.NudTextBox.Text != newValue.ToString())
         control.NudTextBox.Text = newValue.ToString();
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      if (int.TryParse(NudTextBox.Text, out var number) && number < MaxValue)
         SetCurrentValue(ValueProperty, number + 1);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      if (int.TryParse(NudTextBox.Text, out var number) && number > MinValue)
         SetCurrentValue(ValueProperty, number - 1);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (NudTextBox.Text == string.Empty)
         return;

      if (int.TryParse(NudTextBox.Text, out var number) &&
          number >= MinValue &&
          number <= MaxValue)
      {
         SetCurrentValue(ValueProperty, number);
      }
      else
      {
         // Revert text to last valid value
         NudTextBox.Text = Value.ToString();
         NudTextBox.SelectionStart = NudTextBox.Text.Length;
      }
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;

      var fullText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                            .Insert(textBox.SelectionStart, e.Text);

      e.Handled = !int.TryParse(fullText, out _);
   }

   private void NudTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
   {
      if (e.Delta > 0)
         NUDButtonUP_Click(sender, e);
      else
         NUDButtonDown_Click(sender, e);
   }
}