using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Arcanum.UI.Components.UserControls;

public partial class SearchTextBox
{
   public Action<string> RequestSearch { get; set; } = null!;
   public Action SettingsOpened { get; set; } = null!;
   private readonly DispatcherTimer _searchTimer;

   public bool ShowTextUnderline
   {
      get => (bool)GetValue(ShowTextUnderlineProperty);
      set => SetValue(ShowTextUnderlineProperty, value);
   }

   public static readonly DependencyProperty ShowTextUnderlineProperty =
      DependencyProperty.Register(nameof(ShowTextUnderline),
                                  typeof(bool),
                                  typeof(SearchTextBox),
                                  new(false));

   public SearchTextBox()
   {
      _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
      _searchTimer.Tick += (_, _) =>
      {
         _searchTimer.Stop();

         if (RequestSearch == null)
            throw new InvalidOperationException("RequestSearch action is not set.");

         RequestSearch.Invoke(SearchInputTextBox.Text);
      };

      InitializeComponent();
   }

   private void SearchInputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
   {
      _searchTimer.Stop();
      _searchTimer.Start();
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      SettingsOpened.Invoke();
      RequestSearch.Invoke(SearchInputTextBox.Text);
   }
}