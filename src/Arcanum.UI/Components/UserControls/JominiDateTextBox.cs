using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.UserControls;

public partial class JominiDateTextBox : CorneredTextBox
{
   /// <summary>
   /// A helper struct to store the position of a date/time segment (e.g., "yyyy") within the Format string.
   /// </summary>
   private struct DateSegment
   {
      public string Name { get; set; }
      public int StartIndex { get; set; }
      public int Length { get; set; }
   }

   private string? _valueOnMouseEnter;
   private bool _hasScrolledSinceEnter;
   private readonly List<DateSegment> _segments = [];

   #region Format Dependency Property

   public static readonly DependencyProperty FormatProperty =
      DependencyProperty.Register(nameof(Format),
                                  typeof(string),
                                  typeof(JominiDateTextBox),
                                  new("yyyy.MM.dd", OnFormatChanged));

   /// <summary>
   /// Gets or sets the string format used to display and parse the JominiDate.
   /// Example: "yyyy.MM.dd"
   /// </summary>
   public string Format
   {
      get => (string)GetValue(FormatProperty);
      set => SetValue(FormatProperty, value);
   }

   private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is JominiDateTextBox sdtb)
         sdtb.ParseFormat();
   }

   #endregion

   public JominiDateTextBox()
   {
      ParseFormat();

      MouseEnter += OnMouseEnter;
      MouseLeave += OnMouseLeave;
      Unloaded += OnUnloaded;

      ToolTip = "Use mouse wheel to change date.\nHold Ctrl to change by 10, Shift to change by 5.";
   }

   private void OnUnloaded(object sender, RoutedEventArgs e)
   {
      MouseEnter -= OnMouseEnter;
      MouseLeave -= OnMouseLeave;
      Unloaded -= OnUnloaded;
   }

   private void OnMouseLeave(object sender, MouseEventArgs e)
   {
      if (!_hasScrolledSinceEnter)
         return;

      if (Text != _valueOnMouseEnter)
         GetBindingExpression(TextProperty)?.UpdateSource();

      _hasScrolledSinceEnter = false;
      _valueOnMouseEnter = null;
   }

   private void OnMouseEnter(object sender, MouseEventArgs e)
   {
      _valueOnMouseEnter = Text;
      _hasScrolledSinceEnter = false;
   }

   /// <summary>
   /// Analyzes the Format string to identify the character positions of year, month, and day.
   /// </summary>
   private void ParseFormat()
   {
      _segments.Clear();
      var formatString = Format;

      var matches = JominiDateRegex().Matches(formatString);

      foreach (var match in matches.Cast<Match>())
         _segments.Add(new()
         {
            Name = match.Value,
            StartIndex = match.Index,
            Length = match.Length,
         });
   }

   /// <summary>
   /// Overrides the mouse wheel event to handle date manipulation.
   /// </summary>
   protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
   {
      base.OnPreviewMouseWheel(e);
      if (!IsMouseOver)
         return;

      var mousePosition = e.GetPosition(this);

      var charIndex = GetCharacterIndexFromPoint(mousePosition, true);
      var activeSegment = _segments.FirstOrDefault(s =>
                                                      charIndex >= s.StartIndex &&
                                                      charIndex < s.StartIndex + s.Length);

      if (activeSegment.Name == null)
         return;

      if (!JominiDate.TryParse(Text, out var currentDate))
         return;

      var changeValue = 1;
      var isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
      var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      if (isCtrlPressed)
         changeValue *= 10;
      else if (isShiftPressed)
         changeValue *= 5;
      var increment = e.Delta > 0 ? changeValue : -1 * changeValue;

      var segmentName = activeSegment.Name;
      if (segmentName.StartsWith("y", StringComparison.OrdinalIgnoreCase))
         currentDate.AddYears(increment);
      else if (segmentName.StartsWith("M", StringComparison.OrdinalIgnoreCase))
         currentDate.AddMonths(increment);
      else if (segmentName.StartsWith("d", StringComparison.OrdinalIgnoreCase))
         currentDate.AddDays(increment);

      var originalCaretIndex = CaretIndex;
      Text = currentDate.FormatJominiDate(Format);
      if (IsKeyboardFocusWithin)
         CaretIndex = originalCaretIndex;

      _hasScrolledSinceEnter = true;
      e.Handled = true;
   }

   [GeneratedRegex("(y+|M+|d+)")]
   private static partial Regex JominiDateRegex();
}