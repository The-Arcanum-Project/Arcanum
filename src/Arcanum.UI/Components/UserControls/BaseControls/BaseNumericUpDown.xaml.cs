using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class BaseNumericUpDown
{
   public const string INTERMEDIATE_TEXT = "";

   public BaseNumericUpDown()
   {
      InitializeComponent();

      NudTextBox.LostFocus += NudTextBox_LostFocus;
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

   public int? Value
   {
      get => (int?)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value),
                                  typeof(int?),
                                  typeof(BaseNumericUpDown),
                                  new FrameworkPropertyMetadata(null,
                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                OnValueChanged,
                                                                CoerceValue));

   private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (BaseNumericUpDown)d;

      control.CoerceValue(ValueProperty);
   }

   private static object? CoerceValue(DependencyObject d, object? baseValue)
   {
      if (baseValue == null)
         return null;

      var control = (BaseNumericUpDown)d;
      var value = (int)baseValue;

      return Math.Clamp(value, control.MinValue, control.MaxValue);
   }

   private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (BaseNumericUpDown)d;
      var newValue = (int?)e.NewValue;

      var newText = newValue.HasValue
                       ? newValue.Value.ToString(CultureInfo.InvariantCulture)
                       : INTERMEDIATE_TEXT;

      if (control.NudTextBox.Text != newText)
         control.NudTextBox.Text = newText;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      var currentValue = Value ?? (MinValue > 0 ? MinValue : 0);

      SetCurrentValue(ValueProperty, currentValue + 1);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      var currentValue = Value ?? (MinValue > 0 ? MinValue : 0);

      SetCurrentValue(ValueProperty, currentValue - 1);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (string.IsNullOrEmpty(NudTextBox.Text) || NudTextBox.Text == INTERMEDIATE_TEXT)
      {
         if (Value != null)
            SetCurrentValue(ValueProperty, null);
         return;
      }

      // Allow user to type a negative sign without immediate reversion
      if (NudTextBox.Text == "-")
         return;

      if (!int.TryParse(NudTextBox.Text, CultureInfo.InvariantCulture, out var number))
         return;

      // CRITICAL GUARD: Only set the value if it's actually different.
      // This prevents the update "bounce" from happening.
      if (Value == null || Value.Value != number)
         SetCurrentValue(ValueProperty, number);
   }

   private void NudTextBox_LostFocus(object sender, RoutedEventArgs e)
   {
      RevertText();
   }

   private void RevertText()
   {
      NudTextBox.Text = Value.HasValue ? Value.Value.ToString(CultureInfo.InvariantCulture) : INTERMEDIATE_TEXT;
      NudTextBox.SelectionStart = NudTextBox.Text.Length;
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;
      var currentText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
      var proposedText = currentText.Insert(textBox.SelectionStart, e.Text);

      // Allow typing a '-' only at the beginning
      if (proposedText == "-")
      {
         e.Handled = false;
         return;
      }

      e.Handled = !int.TryParse(proposedText, out _);
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