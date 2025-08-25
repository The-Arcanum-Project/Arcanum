using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class FloatNumericUpDown
{
   public FloatNumericUpDown()
   {
      InitializeComponent();

      NudTextBox.Text = Value.ToString(StringFormat, CultureInfo.InvariantCulture);
   }

   public string StringFormat
   {
      get => (string)GetValue(StringFormatProperty);
      set => SetValue(StringFormatProperty, value);
   }

   public static readonly DependencyProperty StringFormatProperty =
      DependencyProperty.Register(nameof(StringFormat),
                                  typeof(string),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata("F2",
                                                                OnStringFormatChanged)); // Default to 2 decimal places

   private static void OnStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      // When the format changes, update the text
      var control = (FloatNumericUpDown)d;
      control.NudTextBox.Text = control.Value.ToString(control.StringFormat, CultureInfo.InvariantCulture);
   }

   public static readonly DependencyProperty InnerBorderThicknessProperty =
      DependencyProperty.Register(nameof(InnerBorderThickness),
                                  typeof(Thickness),
                                  typeof(FloatNumericUpDown),
                                  new(default(Thickness)));

   public Thickness InnerBorderThickness
   {
      get => (Thickness)GetValue(InnerBorderThicknessProperty);
      set => SetValue(InnerBorderThicknessProperty, value);
   }

   public static readonly DependencyProperty InnerBorderBrushProperty =
      DependencyProperty.Register(nameof(InnerBorderBrush),
                                  typeof(Brush),
                                  typeof(FloatNumericUpDown),
                                  new(default(Brush)));

   public Brush InnerBorderBrush
   {
      get => (Brush)GetValue(InnerBorderBrushProperty);
      set => SetValue(InnerBorderBrushProperty, value);
   }

   public float MinValue
   {
      get => (float)GetValue(MinValueProperty);
      set => SetValue(MinValueProperty, value);
   }

   public static readonly DependencyProperty MinValueProperty =
      DependencyProperty.Register(nameof(MinValue),
                                  typeof(float),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(0.0f, OnMinMaxChanged));

   public float MaxValue
   {
      get => (float)GetValue(MaxValueProperty);
      set => SetValue(MaxValueProperty, value);
   }

   public static readonly DependencyProperty MaxValueProperty =
      DependencyProperty.Register(nameof(MaxValue),
                                  typeof(float),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(100.0f, OnMinMaxChanged));

   public float Value
   {
      get => (float)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(float),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(10.0f,
                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                OnValueChanged, // PropertyChangedCallback
                                                                CoerceValue)); // CoerceValueCallback

   public float StepSize
   {
      get => (float)GetValue(StepSizeProperty);
      set => SetValue(StepSizeProperty, value);
   }

   public static readonly DependencyProperty StepSizeProperty =
      DependencyProperty.Register(nameof(StepSize),
                                  typeof(float),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(0.1f));

   private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (FloatNumericUpDown)d;

      if (control.MinValue > control.MaxValue)
         control.MinValue = control.MaxValue;

      if (control.Value < control.MinValue)
         control.Value = control.MinValue;
      else if (control.Value > control.MaxValue)
         control.Value = control.MaxValue;
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (FloatNumericUpDown)d;
      var newValue = Math.Clamp((float)e.NewValue, control.MinValue, control.MaxValue);
      //
      // // Update TextBox text only if different
      var newValString = newValue.ToString(CultureInfo.InvariantCulture);
      if (!control.NudTextBox.Text.Equals(newValString))
         control.NudTextBox.Text = newValString;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      if (float.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) && number < MaxValue)
         SetCurrentValue(ValueProperty, number + StepSize);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      if (float.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) && number > MinValue)
         SetCurrentValue(ValueProperty, number - StepSize);
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;

      var fullText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                            .Insert(textBox.SelectionStart, e.Text);

      e.Handled = !float.TryParse(fullText,
                                  NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                                  CultureInfo.InvariantCulture,
                                  out _);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (NudTextBox.Text == string.Empty)
         return;

      if (float.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number) &&
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

   /// <summary>
   /// This callback is executed BEFORE the value is set. It ensures the value is always
   /// clamped and rounded, preventing floating point errors from being stored.
   /// </summary>
   private static object CoerceValue(DependencyObject d, object baseValue)
   {
      var control = (FloatNumericUpDown)d;
      var value = (float)baseValue;

      // 1. Clamp the value
      value = Math.Clamp(value, control.MinValue, control.MaxValue);

      // 2. Round the value to a high precision to eliminate common arithmetic errors
      // Using decimal for rounding is more accurate for base-10 fractions.
      value = (float)Math.Round((decimal)value, 7); // 7 decimal places is plenty for most UI floats

      return value;
   }
}