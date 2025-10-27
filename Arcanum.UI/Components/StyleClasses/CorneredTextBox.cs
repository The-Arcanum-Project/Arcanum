using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Arcanum.UI.Components.StyleClasses;

/// <summary>
/// A TextBox with rounded corners.
/// </summary>
public class CorneredTextBox : TextBox
{
   public CornerRadius CornerRadiusValue
   {
      get => (CornerRadius)GetValue(CornerRadiusProperty);
      set => SetValue(CornerRadiusProperty, value);
   }

   public static readonly DependencyProperty CornerRadiusProperty =
      DependencyProperty.Register(nameof(CornerRadiusValue),
                                  typeof(CornerRadius),
                                  typeof(CorneredTextBox),
                                  new(new CornerRadius(3)));

   public static readonly DependencyProperty HighlightOnFocusProperty =
      DependencyProperty.Register(nameof(HighlightOnFocus), typeof(bool), typeof(CorneredTextBox), new(true));

   public static readonly DependencyProperty MaxAspectRatioProperty =
      DependencyProperty.Register(nameof(MaxAspectRatio), typeof(double), typeof(CorneredTextBox), new(double.NaN));

   public static readonly DependencyProperty UseDebouncingProperty =
      DependencyProperty.Register(nameof(UseDebouncing), typeof(bool), typeof(CorneredTextBox), new(false));

   public static readonly DependencyProperty DebounceDelayProperty =
      DependencyProperty.Register(nameof(DebounceDelay), typeof(int), typeof(CorneredTextBox), new(300));

   public int DebounceDelay
   {
      get => (int)GetValue(DebounceDelayProperty);
      set => SetValue(DebounceDelayProperty, value);
   }

   public bool UseDebouncing
   {
      get => (bool)GetValue(UseDebouncingProperty);
      set => SetValue(UseDebouncingProperty, value);
   }

   public static readonly RoutedEvent DebouncedTextChangedEvent =
      EventManager.RegisterRoutedEvent(name: nameof(DebouncedTextChanged),
                                       routingStrategy: RoutingStrategy.Bubble,
                                       handlerType: typeof(RoutedEventHandler),
                                       ownerType: typeof(CorneredTextBox));

   public event RoutedEventHandler DebouncedTextChanged
   {
      add => AddHandler(DebouncedTextChangedEvent, value);
      remove => RemoveHandler(DebouncedTextChangedEvent, value);
   }

   public double MaxAspectRatio
   {
      get => (double)GetValue(MaxAspectRatioProperty);
      set => SetValue(MaxAspectRatioProperty, value);
   }

   protected override void OnMouseMove(MouseEventArgs e)
   {
      base.OnMouseMove(e);
      e.Handled = true;
   }

   private string _lastCommittedText = string.Empty;

   public CorneredTextBox()
   {
      Loaded += OnLoaded;
   }

   private void OnLoaded(object sender, RoutedEventArgs e)
   {
      _lastCommittedText = Text;
   }

   private void CommitChange()
   {
      if (Text == _lastCommittedText)
         return;

      _lastCommittedText = Text;
      var args = new RoutedEventArgs(DebouncedTextChangedEvent);
      RaiseEvent(args);
   }

   protected override void OnKeyUp(KeyEventArgs e)
   {
      base.OnKeyUp(e);

      if (e.Key == Key.Escape)
      {
         Text = _lastCommittedText;
         return;
      }

      if (!UseDebouncing)
         return;

      if (e.Key is Key.Enter or Key.Tab)
         CommitChange();
   }

   protected override void OnLostFocus(RoutedEventArgs e)
   {
      base.OnLostFocus(e);

      if (!UseDebouncing)
         return;

      CommitChange();
   }

   protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
   {
      base.OnRenderSizeChanged(sizeInfo);

      if (MaxAspectRatio is <= 0 or Double.NaN)
         return;

      var aspect = ActualWidth / ActualHeight;
      if (aspect > MaxAspectRatio)
         Width = ActualHeight * MaxAspectRatio;
   }

   public bool HighlightOnFocus
   {
      get => (bool)GetValue(HighlightOnFocusProperty);
      set => SetValue(HighlightOnFocusProperty, value);
   }
}