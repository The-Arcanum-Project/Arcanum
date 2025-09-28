using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.Components.Behaviors;

public static partial class TextBoxHelper
{
   private static bool _isUpdating;

   public static readonly DependencyProperty ForceUppercaseProperty =
      DependencyProperty.RegisterAttached("ForceUppercase",
                                          typeof(bool),
                                          typeof(TextBoxHelper),
                                          new(false, OnForceUppercaseChanged));

   public static void SetForceUppercase(DependencyObject element, bool value)
   {
      element.SetValue(ForceUppercaseProperty, value);
   }

   public static bool GetForceUppercase(DependencyObject element)
   {
      return (bool)element.GetValue(ForceUppercaseProperty);
   }

   private static void OnForceUppercaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not TextBox textBox)
         return;

      textBox.TextChanged -= TextBox_TextChanged;

      if ((bool)e.NewValue)
         textBox.TextChanged += TextBox_TextChanged;
   }

   private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      if (_isUpdating)
         return;

      if (sender is not TextBox textBox)
         return;

      var currentText = textBox.Text;
      var upperText = currentText.ToUpperInvariant();

      if (currentText != upperText)
      {
         _isUpdating = true;

         var caretIndex = textBox.CaretIndex;
         textBox.Text = upperText;
         textBox.CaretIndex = caretIndex;
         _isUpdating = false;
      }
   }

   #region IsHexInput Attached Property

   // 1. Define the new Attached Property for Hex validation
   public static readonly DependencyProperty IsHexInputProperty =
      DependencyProperty.RegisterAttached("IsHexInput",
                                          typeof(bool),
                                          typeof(TextBoxHelper),
                                          new(false, OnIsHexInputChanged));

   public static void SetIsHexInput(DependencyObject element, bool value)
   {
      element.SetValue(IsHexInputProperty, value);
   }

   public static bool GetIsHexInput(DependencyObject element)
   {
      return (bool)element.GetValue(IsHexInputProperty);
   }

   private static void OnIsHexInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not TextBox textBox)
         return;

      textBox.PreviewTextInput -= TextBox_PreviewTextInput;
      DataObject.RemovePastingHandler(textBox, TextBox_Pasting);

      if ((bool)e.NewValue)
      {
         textBox.PreviewTextInput += TextBox_PreviewTextInput;
         DataObject.AddPastingHandler(textBox, TextBox_Pasting);
      }
   }

   private static void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
   {
      if (sender is not TextBox textBox)
         return;

      if (e.Text.Length == 0)
         return;

      var startsWithHash = textBox.Text.StartsWith('#');
      if (startsWithHash && textBox.Text.Length >= 7 ||
          !startsWithHash && textBox.Text.Length >= 6 ||
          !IsValidHexCharRegex().IsMatch(e.Text))
      {
         e.Handled = true;
         return;
      }

      if (e.Text == "#" && textBox.Text.Length > 0)
         e.Handled = true;
   }

   private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
   {
      if (!e.DataObject.GetDataPresent(DataFormats.Text))
      {
         e.CancelCommand();
         return;
      }

      if (sender is not TextBox textBox)
         return;

      var pastedText = (string)e.DataObject?.GetData(DataFormats.Text)!;

      var sanitizedText = SanitizationRegex().Replace(pastedText, "");

      if (string.IsNullOrEmpty(sanitizedText))
      {
         e.CancelCommand();
         return;
      }

      var proposedText = textBox.Text.Insert(textBox.CaretIndex, sanitizedText);

      if (proposedText.Length > textBox.MaxLength && textBox.MaxLength > 0 ||
          proposedText.Count(c => c == '#') > 1 ||
          (proposedText.Contains('#') && !proposedText.StartsWith('#')))
      {
         e.CancelCommand();
         return;
      }

      var sanitizedDataObject = new DataObject();
      sanitizedDataObject.SetData(DataFormats.Text, sanitizedText);
      e.DataObject = sanitizedDataObject;
   }

   [GeneratedRegex("[^0-9a-fA-F#]")]
   private static partial Regex SanitizationRegex();

   [GeneratedRegex("[0-9a-fA-F#]")]
   private static partial Regex IsValidHexCharRegex();

   #endregion
}