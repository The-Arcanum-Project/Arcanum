using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class FloatNumericUpDown
{
   public const string INTERMEDIATE_TEXT = "";

   public FloatNumericUpDown()
   {
      InitializeComponent();
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
      var control = (FloatNumericUpDown)d;
      control.NudTextBox.Text = control.Value?.ToString(control.StringFormat, CultureInfo.InvariantCulture) ??
                                INTERMEDIATE_TEXT;
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

   public float? Value
   {
      get => (float?)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(float?),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(null,
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

      if (control.Value.HasValue)
      {
         if (control.Value < control.MinValue)
            control.Value = control.MinValue;
         else if (control.Value > control.MaxValue)
            control.Value = control.MaxValue;
      }
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (FloatNumericUpDown)d;
      var newValue = (float?)e.NewValue;

      var newText = newValue.HasValue
                       ? newValue.Value.ToString(control.StringFormat, CultureInfo.InvariantCulture)
                       : INTERMEDIATE_TEXT;

      if (control.NudTextBox.Text != newText)
         control.NudTextBox.Text = newText;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      float currentValue = Value ?? (MinValue > 0 ? MinValue : 0);
      if (currentValue < MaxValue)
         SetCurrentValue(ValueProperty, currentValue + StepSize);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      float currentValue = Value ?? (MinValue > 0 ? MinValue : 0);
      if (currentValue > MinValue)
         SetCurrentValue(ValueProperty, currentValue - StepSize);
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;
      var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                .Insert(textBox.SelectionStart, e.Text);

      if (proposedText == INTERMEDIATE_TEXT && MinValue < 0 || proposedText == "." && !textBox.Text.Contains('.'))
      {
         e.Handled = false;
         return;
      }

      e.Handled = !float.TryParse(proposedText, CultureInfo.InvariantCulture, out _);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (NudTextBox.Text == INTERMEDIATE_TEXT || string.IsNullOrEmpty(NudTextBox.Text))
      {
         SetCurrentValue(ValueProperty, null);
         return;
      }

      if (float.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number))
      {
         if (number >= MinValue && number <= MaxValue)
            SetCurrentValue(ValueProperty, number);
         else
            RevertText();
      }
      else
      {
         RevertText();
      }
   }

   private void RevertText()
   {
      NudTextBox.Text = Value.HasValue
                           ? Value.Value.ToString(StringFormat, CultureInfo.InvariantCulture)
                           : INTERMEDIATE_TEXT;
      NudTextBox.SelectionStart = NudTextBox.Text.Length;
   }

   private void NudTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
   {
      if (e.Delta > 0)
         NUDButtonUP_Click(sender, e);
      else
         NUDButtonDown_Click(sender, e);
   }

   private void NudTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
   {
      NudTextBox.SelectAll();
   }

   /// <summary>
   /// This callback is executed BEFORE the value is set. It ensures the value is always
   /// clamped and rounded, preventing floating point errors from being stored.
   /// </summary>
   private static object? CoerceValue(DependencyObject d, object? baseValue)
   {
      if (baseValue == null)
         return null;

      var control = (FloatNumericUpDown)d;
      var value = (float)baseValue;

      var roundedValue = (float)Math.Round((decimal)value, 7);

      if (value >= control.MinValue && value <= control.MaxValue && Math.Abs(value - roundedValue) < 0.0001)
         return baseValue;

      var clampedValue = Math.Clamp(value, control.MinValue, control.MaxValue);
      return (float)Math.Round((decimal)clampedValue, 7);
   }
}