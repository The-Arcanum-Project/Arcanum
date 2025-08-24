using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class DecimalBaseNumericUpDown
{
   public DecimalBaseNumericUpDown()
   {
      InitializeComponent();
      PreviewKeyDown += NudTextBox_PreviewKeyDown;
   }

   public static readonly DependencyProperty InnerBorderThicknessProperty =
      DependencyProperty.Register(nameof(InnerBorderThickness),
                                  typeof(Thickness),
                                  typeof(DecimalBaseNumericUpDown),
                                  new(default(Thickness)));

   public Thickness InnerBorderThickness
   {
      get => (Thickness)GetValue(InnerBorderThicknessProperty);
      set => SetValue(InnerBorderThicknessProperty, value);
   }

   public static readonly DependencyProperty InnerBorderBrushProperty =
      DependencyProperty.Register(nameof(InnerBorderBrush),
                                  typeof(Brush),
                                  typeof(DecimalBaseNumericUpDown),
                                  new(default(Brush)));

   public Brush InnerBorderBrush
   {
      get => (Brush)GetValue(InnerBorderBrushProperty);
      set => SetValue(InnerBorderBrushProperty, value);
   }

   public decimal MinValue
   {
      get => (decimal)GetValue(MinValueProperty);
      set => SetValue(MinValueProperty, value);
   }

   public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue),
                                  typeof(decimal),
                                  typeof(DecimalBaseNumericUpDown),
                                  new FrameworkPropertyMetadata(new decimal(0), OnMinMaxChanged));

   public decimal MaxValue
   {
      get => (decimal)GetValue(MaxValueProperty);
      set => SetValue(MaxValueProperty, value);
   }

   public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue),
                                  typeof(decimal),
                                  typeof(DecimalBaseNumericUpDown),
                                  new FrameworkPropertyMetadata(new decimal(10000), OnMinMaxChanged));

   public decimal Value
   {
      get => (decimal)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(decimal),
                                  typeof(DecimalBaseNumericUpDown),
                                  new FrameworkPropertyMetadata(new decimal(10),
                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                OnValueChanged));

   public decimal StepSize
   {
      get => (decimal)GetValue(StepSizeProperty);
      set => SetValue(StepSizeProperty, value);
   }

   public static readonly DependencyProperty StepSizeProperty =
      DependencyProperty.Register(nameof(StepSize),
                                  typeof(decimal),
                                  typeof(DecimalBaseNumericUpDown),
                                  new FrameworkPropertyMetadata(new decimal(0.1)));

   private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (DecimalBaseNumericUpDown)d;

      if (control.MinValue > control.MaxValue)
         control.MinValue = control.MaxValue;

      if (control.Value < control.MinValue)
         control.Value = control.MinValue;
      else if (control.Value > control.MaxValue)
         control.Value = control.MaxValue;
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (DecimalBaseNumericUpDown)d;
      var newValue = Math.Clamp((decimal)e.NewValue, control.MinValue, control.MaxValue);
      //
      // // Update TextBox text only if different
      var newValString = newValue.ToString("0.##########", CultureInfo.InvariantCulture);
      if (!control.NudTextBox.Text.Equals(newValString))
         control.NudTextBox.Text = newValString;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      if (decimal.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) && number < MaxValue)
         SetCurrentValue(ValueProperty, number + StepSize);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      if (decimal.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) && number > MinValue)
         SetCurrentValue(ValueProperty, number - StepSize);
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;

      var fullText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                            .Insert(textBox.SelectionStart, e.Text);

      e.Handled = !decimal.TryParse(fullText,
                                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                    CultureInfo.InvariantCulture,
                                    out _);
   }

   private void NudTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
   {
      // Allow editing keys
      if (e.Key is Key.Back or Key.Delete or Key.Left or Key.Right or Key.Tab or Key.Enter)
         return;

      // Block anything that's not a number or editing
      if (!IsTextInputKey(e.Key))
         e.Handled = true;
   }

   private bool IsTextInputKey(Key key)
   {
      return (key >= Key.D0 && key <= Key.D9) ||
             (key >= Key.NumPad0 && key <= Key.NumPad9) ||
             key == Key.OemPeriod ||
             key == Key.Decimal ||
             key == Key.Subtract ||
             key == Key.OemMinus;
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (NudTextBox.Text == string.Empty)
         return;

      if (decimal.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) &&
          number >= MinValue &&
          number <= MaxValue)
      {
         SetCurrentValue(ValueProperty, number);
      }
      else
      {
         // Revert text to last valid value
         NudTextBox.Text = Value.ToString(CultureInfo.InvariantCulture);
         NudTextBox.SelectionStart = NudTextBox.Text.Length;
      }
   }

   private void NudTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
   {
      if (e.Delta > 0)
         NUDButtonUP_Click(sender, e);
      else
         NUDButtonDown_Click(sender, e);
   }
}