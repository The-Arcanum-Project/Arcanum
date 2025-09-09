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
                                                                OnValueChanged));

   private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var control = (BaseNumericUpDown)d;

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
      var control = (BaseNumericUpDown)d;
      var newValue = (int?)e.NewValue;
      string newText;

      if (newValue.HasValue)
      {
         var clampedValue = newValue.Value;
         if (clampedValue < control.MinValue)
            clampedValue = control.MinValue;
         else if (clampedValue > control.MaxValue)
            clampedValue = control.MaxValue;

         newText = clampedValue.ToString(CultureInfo.InvariantCulture);
      }
      else
      {
         // This is the "indeterminate" state. Display a dash.
         newText = INTERMEDIATE_TEXT;
      }

      // Update the TextBox text only if it's different to prevent cursor jumps
      if (control.NudTextBox.Text != newText)
         control.NudTextBox.Text = newText;
   }

   private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
   {
      int currentValue = Value ?? (MinValue > 0 ? MinValue : 0);

      if (currentValue < MaxValue)
         SetCurrentValue(ValueProperty, currentValue + 1);
   }

   private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
   {
      int currentValue = Value ?? (MinValue > 0 ? MinValue : 0);

      if (currentValue > MinValue)
         SetCurrentValue(ValueProperty, currentValue - 1);
   }

   private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (NudTextBox.Text == INTERMEDIATE_TEXT)
      {
         SetCurrentValue(ValueProperty, null);
         return;
      }

      if (string.IsNullOrEmpty(NudTextBox.Text))
      {
         // An empty textbox can also represent a null state.
         SetCurrentValue(ValueProperty, null);
         return;
      }

      if (int.TryParse(NudTextBox.Text, out var number))
      {
         // If the parsed number is within range, update the source value.
         if (number >= MinValue && number <= MaxValue)
            SetCurrentValue(ValueProperty, number);
         else // Number is out of range, revert to the last valid value.
            RevertText();
      }
      else // Text is not a valid number (and not a INTERMEDIATE_TEXT), revert.
      {
         RevertText();
      }
   }

   private void RevertText()
   {
      NudTextBox.Text = Value.HasValue ? Value.Value.ToString(CultureInfo.InvariantCulture) : INTERMEDIATE_TEXT;
      NudTextBox.SelectionStart = NudTextBox.Text.Length;
   }

   private void NudTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
   {
      var textBox = (TextBox)sender;
      var proposedText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                .Insert(textBox.SelectionStart, e.Text);

      e.Handled = !(int.TryParse(proposedText, out _) || (proposedText == INTERMEDIATE_TEXT && MinValue < 0));
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