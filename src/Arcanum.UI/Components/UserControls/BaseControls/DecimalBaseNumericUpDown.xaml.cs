using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class DecimalBaseNumericUpDown
{
   public const string INTERMEDIATE_STATE_STRING = "";

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

   public decimal? Value
   {
      get => (decimal?)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(decimal?),
                                  typeof(DecimalBaseNumericUpDown),
                                  new FrameworkPropertyMetadata(null,
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
      var control = (DecimalBaseNumericUpDown)d;
      var newValue = (decimal?)e.NewValue;

      var newText = newValue.HasValue
                       ? Math.Clamp(newValue.Value, control.MinValue, control.MaxValue)
                             .ToString("0.##########", CultureInfo.InvariantCulture)
                       : INTERMEDIATE_STATE_STRING;

      if (control.NudTextBox.Text != newText)
         control.NudTextBox.Text = newText;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      var currentValue = Value ?? (MinValue > 0 ? MinValue : 0);
      if (currentValue < MaxValue)
         SetCurrentValue(ValueProperty, currentValue + StepSize);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      var currentValue = Value ?? (MinValue > 0 ? MinValue : 0);
      if (currentValue > MinValue)
         SetCurrentValue(ValueProperty, currentValue - StepSize);
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;
      var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                .Insert(textBox.SelectionStart, e.Text);

      // Allow a single '-' if min value is negative.
      if (proposedText == "-" && MinValue < 0)
      {
         e.Handled = false;
         return;
      }

      // Allow a single '.' if the text doesn't already have one.
      if (proposedText == "." && !textBox.Text.Contains('.'))
      {
         e.Handled = false;
         return;
      }

      e.Handled = !decimal.TryParse(proposedText, CultureInfo.InvariantCulture, out _);
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
      if (NudTextBox.Text == INTERMEDIATE_STATE_STRING || string.IsNullOrEmpty(NudTextBox.Text))
      {
         SetCurrentValue(ValueProperty, null);
         return;
      }

      if (decimal.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number))
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
                           ? Value.Value.ToString("0.##########", CultureInfo.InvariantCulture)
                           : INTERMEDIATE_STATE_STRING;
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
}