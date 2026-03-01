using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Arcanum.UI.Components.StyleClasses;

public class SmartTextControl : Control
{
   public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register(nameof(Text),
                                  typeof(string),
                                  typeof(SmartTextControl),
                                  new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure));

   private static readonly DependencyPropertyKey DisplayedTextKey =
      DependencyProperty.RegisterReadOnly(nameof(DisplayedText),
                                          typeof(string),
                                          typeof(SmartTextControl),
                                          new(string.Empty));

   public static readonly DependencyProperty DisplayedTextProperty = DisplayedTextKey.DependencyProperty;

   static SmartTextControl()
   {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(SmartTextControl), new FrameworkPropertyMetadata(typeof(SmartTextControl)));
   }

   public string Text
   {
      get => (string)GetValue(TextProperty);
      set => SetValue(TextProperty, value);
   }

   public string DisplayedText
   {
      get => (string)GetValue(DisplayedTextProperty);
      private set => SetValue(DisplayedTextKey, value);
   }

   protected override Size ArrangeOverride(Size finalSize)
   {
      UpdateDisplayedText(finalSize.Width);
      return base.ArrangeOverride(finalSize);
   }

   protected override Size MeasureOverride(Size constraint)
   {
      if (!string.IsNullOrEmpty(Text))
      {
         if (CalculateFormattedText(Text, constraint.Width, out var formattedText) || formattedText == null)
            return base.MeasureOverride(constraint);

         var chromeWidth = Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right;
         var chromeHeight = Padding.Top + Padding.Bottom + BorderThickness.Top + BorderThickness.Bottom;

         var desiredWidth = Math.Ceiling(formattedText.Width) + chromeWidth + 2.0;
         var desiredHeight = Math.Ceiling(formattedText.Height) + chromeHeight;

         return new(Math.Min(desiredWidth, constraint.Width), Math.Min(desiredHeight, constraint.Height));
      }

      return base.MeasureOverride(constraint);
   }

   private void UpdateDisplayedText(double width)
   {
      if (string.IsNullOrEmpty(Text))
      {
         DisplayedText = string.Empty;
         return;
      }

      if (!CalculateFormattedText(Text, width, out var formattedText))
      {
         if (formattedText == null)
         {
            DisplayedText = Text;
            return;
         }

         var chromeWidth = Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right;
         var availableWidth = width - chromeWidth;

         var isTooLong = formattedText.Width > availableWidth - 1.0;

         var calculatedText = isTooLong ? GetPrefixedText(Text) : Text;

         if (DisplayedText != calculatedText)
            DisplayedText = calculatedText;
      }
   }

   private bool CalculateFormattedText(string textToMeasure, double width, out FormattedText? formattedText)
   {
      var chromeWidth = Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right;
      var availableWidth = width - chromeWidth;

      if (availableWidth <= 0)
      {
         formattedText = null;
         return true;
      }

      var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
      var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

      formattedText = new(textToMeasure,
                          CultureInfo.CurrentCulture,
                          FlowDirection.LeftToRight,
                          typeface,
                          FontSize,
                          Foreground,
                          dpi);

      return false;
   }

   private static string GetPrefixedText(string original)
   {
      var acronym = string.Concat(original.Where(char.IsUpper));
      if (string.IsNullOrEmpty(acronym) && original.Length >= 2)
         acronym = original[..2].ToUpper();

      return $"[{acronym}] {original}";
   }
}