using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class FloatNumericUpDown
{
   private const string INTERMEDIATE_TEXT = "";
   public event Action<float?>? ValueChanged;

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
      ((FloatNumericUpDown)d).UpdateText(force: true);
   }

   public static readonly DependencyProperty InnerBorderThicknessProperty =
      DependencyProperty.Register(nameof(InnerBorderThickness),
                                  typeof(Thickness),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(new Thickness(1)));

   public Thickness InnerBorderThickness
   {
      get => (Thickness)GetValue(InnerBorderThicknessProperty);
      set => SetValue(InnerBorderThicknessProperty, value);
   }

   public static readonly DependencyProperty InnerBorderBrushProperty =
      DependencyProperty.Register(nameof(InnerBorderBrush),
                                  typeof(Brush),
                                  typeof(FloatNumericUpDown),
                                  new FrameworkPropertyMetadata(null));

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
                                                                OnValueChanged,
                                                                CoerceValue));

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
         control.CoerceValue(ValueProperty);
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (FloatNumericUpDown)d;

      control.UpdateText(force: false);

      control.ValueChanged?.Invoke((float?)e.NewValue);
   }

   private void UpdateText(bool force)
   {
      if (!Value.HasValue)
      {
         NudTextBox.Text = INTERMEDIATE_TEXT;
         return;
      }

      // Calculate the standard formatted text (e.g. "123.45")
      var formattedText = Value.Value.ToString(StringFormat, CultureInfo.InvariantCulture);

      // If the user is typing (Focused), don't overwrite their text 
      // if it is numerically equivalent to the value.
      // This allows "123." or "123.4" to exist without becoming "123.40" immediately.
      if (!force && NudTextBox.IsFocused)
         if (float.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var currentTextVal))
            if (Math.Abs(currentTextVal - Value.Value) < 0.00001f)
               return;

      // Update text if forced, or if values differ scroll/button click, or not focused
      if (NudTextBox.Text != formattedText)
      {
         var caretIndex = NudTextBox.CaretIndex;
         NudTextBox.Text = formattedText;

         if (NudTextBox.IsFocused)
            NudTextBox.CaretIndex = Math.Clamp(caretIndex, 0, NudTextBox.Text.Length);
      }
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      ChangeValue(StepSize);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      ChangeValue(-StepSize);
   }

   private void ChangeValue(float delta)
   {
      var current = (decimal)(Value ?? (MinValue > 0 ? MinValue : 0));
      var step = (decimal)delta;

      var result = (float)(current + step);

      SetCurrentValue(ValueProperty, result);
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;
      var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                .Insert(textBox.SelectionStart, e.Text);

      var isDecimalChar = e.Text is "." or ",";
      var alreadyHasDecimal = textBox.Text.Contains('.') || textBox.Text.Contains(',');

      if ((e.Text == "-" && MinValue < 0) || (isDecimalChar && !alreadyHasDecimal))
      {
         e.Handled = false;
         return;
      }

      // We replace , with . just for the check so TryParse works logic-wise
      e.Handled = !float.TryParse(proposedText.Replace(',', '.'), CultureInfo.InvariantCulture, out _);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (string.IsNullOrEmpty(NudTextBox.Text) || NudTextBox.Text == "-" || NudTextBox.Text == ".")
         return;

      var currentText = NudTextBox.Text;

      // Automatically replace ',' with '.' while typing
      if (currentText.Contains(','))
      {
         var caretIndex = NudTextBox.CaretIndex;
         currentText = currentText.Replace(',', '.');

         NudTextBox.Text = currentText;
         NudTextBox.CaretIndex = caretIndex;
      }

      if (float.TryParse(currentText, CultureInfo.InvariantCulture, out var number))
         if (Value == null || Math.Abs(Value.Value - number) > 0.000001f)
            SetCurrentValue(ValueProperty, number);
   }

   private void NudTextBox_KeyDown(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Enter)
      {
         UpdateText(force: true);
         e.Handled = true;
      }
   }

   private void NudTextBox_LostFocus(object sender, RoutedEventArgs e)
   {
      // Force "F2" formatting to clean up inputs like "123." or "123.4" -> "123.40"
      UpdateText(force: true);
   }

   private void NudTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
   {
      e.Handled = true;
      if (!NudTextBox.IsFocused)
         NudTextBox.Focus();

      if (e.Delta > 0)
         ChangeValue(StepSize);
      else
         ChangeValue(-StepSize);
   }

   private void NudTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
   {
      NudTextBox.SelectAll();
   }

   private static object? CoerceValue(DependencyObject d, object? baseValue)
   {
      if (baseValue == null)
         return null;

      var control = (FloatNumericUpDown)d;
      var value = (float)baseValue;

      // Rounding based on StringFormat (e.g. "F2" -> 2 decimal places)
      var precision = 2;
      if (!string.IsNullOrEmpty(control.StringFormat) &&
          control.StringFormat.Length > 1 &&
          int.TryParse(control.StringFormat.AsSpan(1), out var p))
         precision = p;

      value = (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);

      if (value < control.MinValue)
         return control.MinValue;
      if (value > control.MaxValue)
         return control.MaxValue;

      return value;
   }
}